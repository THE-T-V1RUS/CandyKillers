using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Hertzole.GoldPlayer;
using TMPro;

public class EnterDemonDomain : MonoBehaviour
{
    [SerializeField] private Volume globalVolume;
    [SerializeField] private GoldPlayerController playerController;
    [SerializeField] private EquipmentController playerEquipmentController;

    [SerializeField] private Transform demonDomainCenter, demonDomainLookAt;

    [SerializeField] private TextMeshPro page1Text, page2Text;

    [SerializeField] private Interactable demonDomainInteractable;

    [SerializeField] private AudioClip snd_teleport, snd_demonRedaction;

    [SerializeField] private DemonController demonController;

    [SerializeField] [Range(0f, 1f)] private float redactionPercentage = 0.33f;

    private LensDistortion lensDistortion;

    [Header("Default Settings")]
    public float defaultLensDistortionIntensity = 0f;
    public float defaultLendsDistortionScale = 1f;
    public float demonDomainLensDistortionIntensity = -1f;
    public float demonDomainLensDistortionScale = 0.01f;

    private float currentLensDistortionIntensity;
    private float currentLensDistortionScale;

    void Awake()
    {        
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out lensDistortion);
        }

        currentLensDistortionIntensity = defaultLensDistortionIntensity;
        currentLensDistortionScale = defaultLendsDistortionScale;
    }

    public void EnterDemonDomainEffect()
    {
        demonDomainInteractable.enabled = false;
        StartCoroutine(EnterDemonDomainCoroutine());
    }

    IEnumerator EnterDemonDomainCoroutine()
    {
        playerEquipmentController.ForceEquipment(0);
        playerEquipmentController.blockPlayerInput = true;
        playerController.Movement.CanCrouch = false;
        playerController.Movement.CanMoveAround = false;
        playerController.Camera.CanLookAround = false;
        playerController.Camera.ForceLook(transform.position);

        Recitation_AudioManager.Instance.PlaySFX(snd_teleport);

        float duration = 2f; // Duration of the effects in seconds

        // slowly change lens distortion intensity to target value
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            currentLensDistortionIntensity = Mathf.Lerp(defaultLensDistortionIntensity, demonDomainLensDistortionIntensity, t);

            if (lensDistortion != null)
            {
                lensDistortion.intensity.value = currentLensDistortionIntensity;
            }

            yield return null;
        }

        // slowly change lens distortion scale to target value
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            currentLensDistortionScale = Mathf.Lerp(defaultLendsDistortionScale, demonDomainLensDistortionScale, t);

            if (lensDistortion != null)
            {
                lensDistortion.scale.value = currentLensDistortionScale;
            }

            yield return null;
        }

        //TELEPORT
        playerController.SetPosition(demonDomainCenter.position);
        playerController.Camera.ForceLook(demonDomainLookAt.position);


        //slowly restore lens distortion to default values
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            currentLensDistortionIntensity = Mathf.Lerp(demonDomainLensDistortionIntensity, defaultLensDistortionIntensity, t);
            currentLensDistortionScale = Mathf.Lerp(demonDomainLensDistortionScale, defaultLendsDistortionScale, t);

            if (lensDistortion != null)
            {
                lensDistortion.intensity.value = currentLensDistortionIntensity;
                lensDistortion.scale.value = currentLensDistortionScale;
            }

            yield return null;
        }

        playerEquipmentController.ForceEquipment(2);

        yield return new WaitForSeconds(1.5f);

        Recitation_AudioManager.Instance.PlaySFX(snd_demonRedaction);

        yield return new WaitForSeconds(snd_demonRedaction.length);

        demonController.StartMovingForwardAtSetSpeed();
        Recitation_AudioManager.Instance.ChangeAmbience(Recitation_AudioManager.Instance.amb_chase);

        playerEquipmentController.blockPlayerInput = false;
        playerController.Movement.CanCrouch = true;
        playerController.Movement.CanMoveAround = true;
        playerController.Camera.CanLookAround = true;
        playerController.Camera.StopForceLooking();

        // Animate font change for both pages simultaneously
        yield return StartCoroutine(AnimateFontChange());
    }
    
    IEnumerator AnimateFontChange()
    {
        float duration = 1f;
        
        // Get original text
        string originalPage1 = page1Text.text;
        string originalPage2 = page2Text.text;
        
        // Split into words
        string[] page1Words = originalPage1.Split(' ');
        string[] page2Words = originalPage2.Split(' ');
        
        int totalPage1Words = page1Words.Length;
        int totalPage2Words = page2Words.Length;
        
        // Calculate percentage of words to change
        int maxPage1Words = Mathf.FloorToInt(totalPage1Words * redactionPercentage);
        int maxPage2Words = Mathf.FloorToInt(totalPage2Words * redactionPercentage);
        
        // Track which words have been changed
        bool[] page1Changed = new bool[totalPage1Words];
        bool[] page2Changed = new bool[totalPage2Words];
        
        // Create shuffled indices for random order
        System.Collections.Generic.List<int> page1Indices = new System.Collections.Generic.List<int>();
        System.Collections.Generic.List<int> page2Indices = new System.Collections.Generic.List<int>();
        
        for (int i = 0; i < totalPage1Words; i++) page1Indices.Add(i);
        for (int i = 0; i < totalPage2Words; i++) page2Indices.Add(i);
        
        // Shuffle using Fisher-Yates
        for (int i = page1Indices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = page1Indices[i];
            page1Indices[i] = page1Indices[j];
            page1Indices[j] = temp;
        }
        
        for (int i = page2Indices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = page2Indices[i];
            page2Indices[i] = page2Indices[j];
            page2Indices[j] = temp;
        }
        
        int page1WordsChanged = 0;
        int page2WordsChanged = 0;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Calculate how many words should be changed by now (up to 50%)
            int targetPage1Words = Mathf.FloorToInt(t * maxPage1Words);
            int targetPage2Words = Mathf.FloorToInt(t * maxPage2Words);
            
            // Change words up to target count for page 1
            while (page1WordsChanged < targetPage1Words && page1WordsChanged < maxPage1Words)
            {
                page1Changed[page1Indices[page1WordsChanged]] = true;
                page1WordsChanged++;
            }
            
            // Change words up to target count for page 2
            while (page2WordsChanged < targetPage2Words && page2WordsChanged < maxPage2Words)
            {
                page2Changed[page2Indices[page2WordsChanged]] = true;
                page2WordsChanged++;
            }
            
            // Rebuild text with font tags for changed words
            string newPage1Text = BuildTextWithFontTags(page1Words, page1Changed);
            string newPage2Text = BuildTextWithFontTags(page2Words, page2Changed);
            
            page1Text.text = newPage1Text;
            page2Text.text = newPage2Text;
            
            yield return null;
        }
        
        // Ensure specified percentage of words are changed at the end
        for (int i = 0; i < maxPage1Words; i++) page1Changed[page1Indices[i]] = true;
        for (int i = 0; i < maxPage2Words; i++) page2Changed[page2Indices[i]] = true;
        
        page1Text.text = BuildTextWithFontTags(page1Words, page1Changed);
        page2Text.text = BuildTextWithFontTags(page2Words, page2Changed);
    }
    
    string BuildTextWithFontTags(string[] words, bool[] changed)
    {
        string result = "";
        
        for (int i = 0; i < words.Length; i++)
        {
            if (changed[i])
            {
                result += "<font=\"DevilsTongue-DOO09 SDF\">" + words[i] + "</font>";
            }
            else
            {
                result += words[i];
            }
            
            // Add space between words (except after last word)
            if (i < words.Length - 1)
            {
                result += " ";
            }
        }
        
        return result;
    }
}