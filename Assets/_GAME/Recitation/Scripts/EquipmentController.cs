using Hertzole.GoldPlayer;
using UnityEngine;
using Whisper.Samples;
using System.Collections.Generic;
using System.IO;
using TMPro;
using System.Collections;

public class EquipmentController : MonoBehaviour
{
    GoldPlayerController playerController;
    [SerializeField] Animator bookAnimator, micAnimator, headphonesAnimator, crossAnimator;
    [SerializeField] int currentEquipment = 0; // 0 = None, 1 = Mic, 2 = Book
    [SerializeField] MicrophoneDemo microphoneDemo;
    [SerializeField] TextMeshPro bookText_Page1, bookText_Page2;

    //CROSS EQUIPMENT
    [SerializeField] Material crossMaterial;
    [SerializeField] Transform crossParticleSystemTransform;
    [SerializeField] ParticleSystem crossFailParticleSystem;
    [SerializeField] Hovl_Laser crossLaser;
    [SerializeField] AudioSource crossAudioSource;
    [SerializeField] float targetCrossVolume = 0.5f;
    [SerializeField] AudioClip snd_PrayerSuccess, snd_PrayerFail, snd_PrayerCharge, snd_BurnPage;
    [SerializeField] float cross_screenShakeDuration = 1.4f;
    [SerializeField] float cross_screenShakeIntensity = 0.1f;

    //BOOK EQUIPMENT
    [SerializeField] string Phrase_1, Phrase_2;
    [SerializeField] int phraseDifficulty = 1; // 1 = Easy, 2 = Medium, 3 = Hard, 4 = Expert, 5 = Master
    [SerializeField] int scoreThreshold = 90;
    [SerializeField] BookController bookController;
    [SerializeField] MeshRenderer bookPage1_Renderer, bookPage2_Renderer;
    private Material bookPage1_Material, bookPage2_Material;
    [SerializeField] float pageDissolveDuration = 0.5f;
    [SerializeField] ParticleSystem bookSmokeParticleSystem;

    bool isRecording = false;
    bool isProcessing = false;
    bool isRecordingInterrupted = false;
    public bool blockPlayerInput = false;
    [SerializeField] string currentPhrase = "";
    int pendingEquipment = -1; // -1 means no pending switch
    
    private Coroutine crossCoroutine = null;
    private Coroutine crossAudioCoroutine = null;

    // CSV data
    private List<string> startPhrases = new List<string>();
    private List<string> endPhrases = new List<string>();
    
    // Tracking used phrases
    private List<int> unusedStartIndices = new List<int>();
    private List<int> unusedEndIndices = new List<int>();

    private void Start()
    {
        crossMaterial.SetColor("_glow_color", Color.black);
        crossParticleSystemTransform.localScale = Vector3.zero;
        playerController = GetComponent<GoldPlayerController>();
        LoadPrayersCSV();
        phraseDifficulty = 1;
        Phrase_1 = GeneratePhrase();
        phraseDifficulty = 2;
        Phrase_2 = GeneratePhrase();
        currentPhrase = Phrase_1;
        bookPage1_Material = bookPage1_Renderer.material;
        bookPage2_Material = bookPage2_Renderer.material;
        UpdateBookText();
    }

    private void Update()
    {
        AttemptSwitchEquipment();
        UseBook();
        CheckPendingEquipmentSwitch();
    }

    public string GetCurrentPhrase()
    {
        return currentPhrase;
    }

    public int GetCurrentEquipment()
    {
        return currentEquipment;
    }
    
    public Animator GetBookAnimator()
    {
        return bookAnimator;
    }
    
    public void ForceEquipment(int equipmentIndex)
    {
        currentEquipment = Mathf.Clamp(equipmentIndex, 0, 2);
        ChangeEquipment(currentEquipment);
    }

    private void UpdateBookText()
    {
        bookText_Page1.text = Phrase_1;
        bookText_Page2.text = Phrase_2;
    }

    void AttemptSwitchEquipment()
    {
        if (isRecording) return;
        if (isProcessing) return;
        if (blockPlayerInput) return;

        // Spacebar: Cycle through equipment (0 -> 1 -> 2 -> 0)
        if(Input.GetKeyDown(KeyCode.Space))
        {
            currentEquipment = (currentEquipment + 1) % 3;
            ChangeEquipment(currentEquipment);
            return;
        }

        // Mouse scroll wheel: Cycle through equipment
        if(Input.mouseScrollDelta.y > 0)
        {
            currentEquipment = (currentEquipment + 1) % 3;
            ChangeEquipment(currentEquipment);
            return;
        }
        else if(Input.mouseScrollDelta.y < 0)
        {
            currentEquipment = (currentEquipment - 1 + 3) % 3;
            ChangeEquipment(currentEquipment);
            return;
        }

        // Key 1: Toggle Book + Cross (equipment 2)
        if(Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            if (currentEquipment == 2)
            {
                // Already equipped, unequip
                currentEquipment = 0;
            }
            else
            {
                // Equip Book + Cross
                currentEquipment = 2;
            }
            ChangeEquipment(currentEquipment);
            return;
        }
        // Key 2: Toggle Headphones + Mic (equipment 1)
        else if(Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            if (currentEquipment == 1)
            {
                // Already equipped, unequip
                currentEquipment = 0;
            }
            else
            {
                // Equip Headphones + Mic
                currentEquipment = 1;
            }
            ChangeEquipment(currentEquipment);
            return;
        }
    }   

    void ChangeEquipment(int equipment)
    {
        // Stop any ongoing cross coroutines when switching equipment
        if (crossCoroutine != null)
        {
            StopCoroutine(crossCoroutine);
            crossCoroutine = null;
        }
        StopCrossAudioCoroutine();
        
        // Reset cross visual effects
        crossMaterial.SetColor("_glow_color", Color.black);
        crossParticleSystemTransform.localScale = Vector3.zero;
        
        switch(equipment)
        {
            default:
            case 0: // No Equipment
                // Reset recording flags when unequipping
                isRecording = false;
                isProcessing = false;
                isRecordingInterrupted = false;
                
                bookAnimator.SetBool("isEquipped", false);
                crossAnimator.SetBool("isEquipped", false);
                crossAnimator.SetBool("isUsing", false);
                micAnimator.SetBool("isEquipped", false);
                headphonesAnimator.SetBool("isEquipped", false);
                Recitation_AudioManager.Instance.SetAudioMode(Recitation_AudioManager.AudioMode.Normal);
                break;
            case 1: // Mic + Headphones Equipped
                // Reset recording flags when switching away from book
                isRecording = false;
                isProcessing = false;
                isRecordingInterrupted = false;
                
                bookAnimator.SetBool("isEquipped", false);
                crossAnimator.SetBool("isEquipped", false);
                crossAnimator.SetBool("isUsing", false);
                
                // Set pending equipment and let CheckPendingEquipmentSwitch handle it
                pendingEquipment = 1;
                break;
            case 2: // Book Equipped
                // Reset recording flags to ensure clean state
                isRecording = false;
                isProcessing = false;
                isRecordingInterrupted = false;
                
                crossAnimator.SetBool("isUsing", false);
                micAnimator.SetBool("isEquipped", false);
                headphonesAnimator.SetBool("isEquipped", false);
                
                // Set pending equipment and let CheckPendingEquipmentSwitch handle it
                pendingEquipment = 2;
                break;
        }
    }

    public void UseBook()
    {
        if (currentEquipment != 2) return;
        if (blockPlayerInput) return;

        if (Input.GetMouseButtonDown(1) && !isRecording && !isProcessing)
        {
            isRecordingInterrupted = false;
            microphoneDemo.ToggleRecording();
            crossAnimator.SetBool("isUsing", true);
            if (crossCoroutine != null) StopCoroutine(crossCoroutine);
            crossCoroutine = StartCoroutine(StartUsingCross());
            isRecording = true;
            return;
        }

        if (!Input.GetMouseButton(1) && isRecording && !isProcessing)
        {
            isRecording = false;
            isProcessing = true;
            Recitation_AudioManager.Instance.PlayInterruptibleSFX(snd_PrayerCharge);
            microphoneDemo.ToggleRecording();
            return;
        }

    }

    public void InterruptRecording()
    {
        // Mark as interrupted to prevent prayer results from triggering
        isRecordingInterrupted = true;
        
        // Stop recording if active
        if (isRecording)
        {
            microphoneDemo.CancelRecording();
        }
        
        // Stop cross effects
        if (crossCoroutine != null)
        {
            StopCoroutine(crossCoroutine);
            crossCoroutine = null;
        }
        
        StopCrossAudioCoroutine();
        
        // Reset cross visual effects
        crossMaterial.SetColor("_glow_color", Color.black);
        crossParticleSystemTransform.localScale = Vector3.zero;
        
        // Reset animator
        crossAnimator.SetBool("isUsing", false);
        
        // Reset flags
        isRecording = false;
        isProcessing = false;
        
        // Stop any interrupt audio
        Recitation_AudioManager.Instance.StopInterruptibleSFX();
    }

    void LoadPrayersCSV()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Prayers.csv");
        
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Prayers.csv not found at: {filePath}");
            return;
        }

        string[] lines = File.ReadAllLines(filePath);
        
        // Skip header line
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Parse CSV line (handle quoted fields)
            string[] columns = ParseCSVLine(line);
            
            if (columns.Length >= 2)
            {
                string startPhrase = columns[0].Trim();
                string endPhrase = columns[1].Trim();
                
                if (!string.IsNullOrEmpty(startPhrase))
                {
                    startPhrases.Add(startPhrase);
                }
                
                if (!string.IsNullOrEmpty(endPhrase))
                {
                    endPhrases.Add(endPhrase);
                }
            }
        }

        // Initialize unused indices
        ResetUnusedIndices();
        
        Debug.Log($"Loaded {startPhrases.Count} start phrases and {endPhrases.Count} end phrases");
    }

    string[] ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string currentField = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }
        
        result.Add(currentField);
        return result.ToArray();
    }

    void ResetUnusedIndices()
    {
        unusedStartIndices.Clear();
        unusedEndIndices.Clear();
        
        for (int i = 0; i < startPhrases.Count; i++)
        {
            unusedStartIndices.Add(i);
        }
        
        for (int i = 0; i < endPhrases.Count; i++)
        {
            unusedEndIndices.Add(i);
        }
    }

    public string GeneratePhrase()
    {
        if (startPhrases.Count == 0 || endPhrases.Count == 0)
        {
            Debug.LogError("No phrases loaded from CSV!");
            return "Error: No phrases loaded";
        }

        // Ensure we have enough start phrases for the difficulty
        int numStartPhrases = Mathf.Clamp(phraseDifficulty, 1, startPhrases.Count);
        
        // Reset if we don't have enough unused phrases
        if (unusedStartIndices.Count < numStartPhrases)
        {
            ResetUnusedIndices();
            Debug.Log("Reset start phrases - all have been used!");
        }
        
        if (unusedEndIndices.Count == 0)
        {
            ResetUnusedIndices();
            Debug.Log("Reset end phrases - all have been used!");
        }

        // Build the phrase
        string generatedPhrase = "";
        
        // Get random start phrases based on difficulty
        for (int i = 0; i < numStartPhrases; i++)
        {
            int randomIndex = Random.Range(0, unusedStartIndices.Count);
            int phraseIndex = unusedStartIndices[randomIndex];
            unusedStartIndices.RemoveAt(randomIndex);
            
            generatedPhrase += startPhrases[phraseIndex];
            
            // Add space between start phrases (not after last one)
            if (i < numStartPhrases - 1)
            {
                generatedPhrase += " ";
            }
        }
        
        // Get random end phrase
        int randomEndIndex = Random.Range(0, unusedEndIndices.Count);
        int endPhraseIndex = unusedEndIndices[randomEndIndex];
        unusedEndIndices.RemoveAt(randomEndIndex);
        
        generatedPhrase += " " + endPhrases[endPhraseIndex];
        
        return generatedPhrase;
    }

    IEnumerator StartUsingCross()
    {
        StartCrossAudioCoroutine();
        float duration = 3f;
        float elapsed = 0f;
        
        Color startColor = crossMaterial.GetColor("_glow_color");
        Color targetColor = Color.yellow;
        
        Vector3 startScale = crossParticleSystemTransform.localScale;
        Vector3 targetScale = Vector3.one * 0.045423f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Lerp color
            crossMaterial.SetColor("_glow_color", Color.Lerp(startColor, targetColor, t));
            
            // Lerp scale
            crossParticleSystemTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            
            yield return null;
        }
        
        // Ensure final values are set
        crossMaterial.SetColor("_glow_color", targetColor);
        crossParticleSystemTransform.localScale = targetScale;
        
        crossCoroutine = null;
    }

    IEnumerator StopUsingCross()
    {
        float duration = 0.25f;
        float elapsed = 0f;
        
        Color startColor = crossMaterial.GetColor("_glow_color");
        Color targetColor = Color.black;
        
        Vector3 startScale = crossParticleSystemTransform.localScale;
        Vector3 targetScale = Vector3.zero;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Lerp color
            crossMaterial.SetColor("_glow_color", Color.Lerp(startColor, targetColor, t));
            
            // Lerp scale
            crossParticleSystemTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            
            yield return null;
        }
        
        // Ensure final values are set
        crossMaterial.SetColor("_glow_color", targetColor);
        crossParticleSystemTransform.localScale = targetScale;
        
        crossCoroutine = null;
    }

    public void PrayerResults(int score)
    {
        // Don't process results if recording was interrupted (e.g., player was harmed)
        if (isRecordingInterrupted)
        {
            isRecordingInterrupted = false;
            return;
        }
        
        if (crossCoroutine != null) StopCoroutine(crossCoroutine);
        crossCoroutine = StartCoroutine(StopUsingCross());
        StopCrossAudioCoroutine();

        if (score >= scoreThreshold)
        {
            //Success
            Recitation_GameManager.Instance.cameraEffectsHandler.TriggerScreenShake(cross_screenShakeDuration, cross_screenShakeIntensity);
            Recitation_AudioManager.Instance.PlaySFX(snd_PrayerSuccess);
            StartCoroutine(crossLaserRoutine());
        }
        else
        {
            //Fail
            Recitation_AudioManager.Instance.StopInterruptibleSFX();
            Recitation_AudioManager.Instance.PlaySFX(snd_PrayerFail);
            isProcessing = false;
            crossFailParticleSystem.Play();
            crossAnimator.SetBool("isUsing", false);
        }
    }

    IEnumerator crossLaserRoutine()
    {
        crossLaser.isLaserActive = true;
        yield return new WaitForSeconds(0.25f);
        crossLaser.isLaserActive = false;
        yield return new WaitForSeconds(0.5f);
        crossAnimator.SetBool("isUsing", false);
        isProcessing = false;
    }

    IEnumerator StartCrossAudio()
    {
        crossAudioSource.volume = 0f;
        crossAudioSource.Play();
        float fadeDuration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            crossAudioSource.volume = Mathf.Lerp(0f, targetCrossVolume, t);
            yield return null;
        }
        
        crossAudioSource.volume = targetCrossVolume;
        crossAudioCoroutine = null;
    }

    public void StartCrossAudioCoroutine()
    {
        if (crossAudioCoroutine != null) StopCoroutine(crossAudioCoroutine);
        crossAudioCoroutine = StartCoroutine(StartCrossAudio());
    }

    public void StopCrossAudioCoroutine()
    {
        if (crossAudioCoroutine != null) StopCoroutine(crossAudioCoroutine);
        crossAudioSource.Stop();
    }

    void CheckPendingEquipmentSwitch()
    {
        if (pendingEquipment == -1) return;

        AnimatorStateInfo bookState = bookAnimator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo crossState = crossAnimator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo micState = micAnimator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo headphonesState = headphonesAnimator.GetCurrentAnimatorStateInfo(0);
        
        // Check if items are in "Unequipped" state OR transitioning TO "Unequipped"
        bool bookReady = (bookState.IsName("Unequipped") && !bookAnimator.IsInTransition(0)) || 
                        (bookAnimator.IsInTransition(0) && bookAnimator.GetNextAnimatorStateInfo(0).IsName("Unequipped"));
        bool crossReady = (crossState.IsName("Unequipped") && !crossAnimator.IsInTransition(0)) || 
                         (crossAnimator.IsInTransition(0) && crossAnimator.GetNextAnimatorStateInfo(0).IsName("Unequipped"));
        bool micReady = (micState.IsName("Unequipped") && !micAnimator.IsInTransition(0)) || 
                       (micAnimator.IsInTransition(0) && micAnimator.GetNextAnimatorStateInfo(0).IsName("Unequipped"));
        bool headphonesReady = (headphonesState.IsName("Unequipped") && !headphonesAnimator.IsInTransition(0)) || 
                              (headphonesAnimator.IsInTransition(0) && headphonesAnimator.GetNextAnimatorStateInfo(0).IsName("Unequipped"));
        
        switch (pendingEquipment)
        {
            case 1: // Waiting to equip Mic + Headphones
                if (bookReady && crossReady)
                {
                    micAnimator.SetBool("isEquipped", true);
                    headphonesAnimator.SetBool("isEquipped", true);
                    pendingEquipment = -1;
                }
                break;
                
            case 2: // Waiting to equip Book & Cross
                if (micReady && headphonesReady)
                {
                    crossAnimator.SetBool("isEquipped", true);
                    bookAnimator.SetBool("isEquipped", true);
                    Recitation_AudioManager.Instance.SetAudioMode(Recitation_AudioManager.AudioMode.Normal);
                    pendingEquipment = -1;
                }
                break;
        }
    }

    public void GenerateNextPrayer()
    {
        StartCoroutine(GenerateNextPrayerRoutine());
    }
    
    IEnumerator GenerateNextPrayerRoutine()
    {
        yield return new WaitForSeconds(1f);

        phraseDifficulty += 1;
        Material pageMaterialToUpdate = bookController.isFocusedOnPage1 ? bookPage1_Material : bookPage2_Material;

        if (currentPhrase == Phrase_1)
        {
            currentPhrase = Phrase_2;
            Phrase_1 = GeneratePhrase();
        }
        else
        {
            currentPhrase = Phrase_1;
            Phrase_2 = GeneratePhrase();
        }

        // Smoothly fade out the current page with ease-in
        float elapsed = 0f;
        float startValue = pageMaterialToUpdate.GetFloat("_Disolve_Amount");
        Recitation_AudioManager.Instance.PlaySFX(snd_BurnPage);
        bookSmokeParticleSystem.Play();

        while (elapsed < pageDissolveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / pageDissolveDuration;
            // Ease-in curve
            float easedT = t * t;
            pageMaterialToUpdate.SetFloat("_Disolve_Amount", Mathf.Lerp(startValue, 1f, easedT));
            yield return null;
        }
        
        pageMaterialToUpdate.SetFloat("_Disolve_Amount", 1f);

        UpdateBookText();
        bookController.isFocusedOnPage1 = !bookController.isFocusedOnPage1;

        // Wait 1 second
        yield return new WaitForSeconds(1f);
        
        // Instantly reset dissolve to show new content
        pageMaterialToUpdate.SetFloat("_Disolve_Amount", 0f);
    }
}
