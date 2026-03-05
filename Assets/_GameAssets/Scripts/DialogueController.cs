using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class DialogueController : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup subtitlesCanvasGroup;
    [SerializeField] private TextMeshProUGUI subtitlesTMP;
    [SerializeField] private GameObject nextLineObj; // Shown when waiting for input
    [SerializeField] private CoreGameManager gameManager;
    public AudioSource audioSource;
    public int lastSound = 0;
    public List<AudioClip> characterSoundEffects;
    public float minPitch, maxPitch;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float charRevealDelay = 0.05f;

    public bool isShowingSubtitles { get; private set; }

    private Coroutine subtitleRoutine;
    private bool isRevealing = false;
    private bool skipRequested = false;

    private void Start()
    {
        StartCoroutine(GetGameManagerRoutine());
    }

    private IEnumerator GetGameManagerRoutine()
    {
        while (gameManager == null)
        {
            gameManager = CoreGameManager.Instance;
            yield return null;
        }
        Debug.Log("GameManager found: " + gameManager.name);
    }

    private void Update()
    {
        // Player requests skip
        if (isRevealing && Input.anyKeyDown && Time.timeScale > 0)
        {
            skipRequested = true;
        }
    }

    // --- Single line API ---
    public void ShowSubtitles(string text, bool enablePlayerAfterText = true)
    {
        StopCoroutines();
        subtitleRoutine = StartCoroutine(SubtitleStarterSingle(text, enablePlayerAfterText));
    }

    // --- Multiple lines API ---
    public void ShowSubtitles(List<string> lines, bool enablePlayerAfterText = true)
    {
        StopCoroutines();
        subtitleRoutine = StartCoroutine(SubtitleStarterMultiple(lines, enablePlayerAfterText));
    }

    private IEnumerator SubtitleStarterSingle(string text, bool enablePlayerAfterText)
    {
        yield return null;
        yield return StartCoroutine(SubtitleSequenceSingle(text, enablePlayerAfterText));
        subtitleRoutine = null;
    }

    private IEnumerator SubtitleStarterMultiple(List<string> lines, bool enablePlayerAfterText)
    {
        yield return null;
        yield return StartCoroutine(SubtitleSequenceMultiple(lines, enablePlayerAfterText));
        subtitleRoutine = null;
    }

    private IEnumerator SubtitleSequenceSingle(string text, bool enablePlayerAfterText)
    {
        gameManager.TogglePlayerMovement(false);
        gameManager.TogglePlayerRotation(false);
        isShowingSubtitles = true;

        yield return StartCoroutine(FadeCanvas(true));
        yield return StartCoroutine(RevealText(text));

        // Wait for fresh input (not carried from skip)
        yield return StartCoroutine(WaitForFreshKeyPress());

        yield return StartCoroutine(FadeCanvas(false));
        isShowingSubtitles = false;
        subtitlesTMP.text = string.Empty;

        if (enablePlayerAfterText)
        {
            gameManager.TogglePlayerMovement(true);
            gameManager.TogglePlayerRotation(true);
        }
    }

    private IEnumerator SubtitleSequenceMultiple(List<string> lines, bool enablePlayerAfterText = true)
    {
        gameManager.TogglePlayerMovement(false);
        gameManager.TogglePlayerRotation(false);
        isShowingSubtitles = true;

        yield return StartCoroutine(FadeCanvas(true));

        for (int i = 0; i < lines.Count; i++)
        {
            yield return StartCoroutine(RevealText(lines[i]));

            nextLineObj?.SetActive(true);
            yield return StartCoroutine(WaitForFreshKeyPress());
            nextLineObj?.SetActive(false);
        }

        yield return StartCoroutine(FadeCanvas(false));
        isShowingSubtitles = false;
        subtitlesTMP.text = string.Empty;

        if (enablePlayerAfterText)
        {
            gameManager.TogglePlayerMovement(true);
            gameManager.TogglePlayerRotation(true);
        }
    }

    // --- Reveal Text ---
    private IEnumerator RevealText(string text)
    {
        yield return new WaitForEndOfFrame();

        isRevealing = true;
        skipRequested = false;
        subtitlesTMP.text = text;
        subtitlesTMP.ForceMeshUpdate();

        TMP_TextInfo textInfo = subtitlesTMP.textInfo;
        Color32[] newVertexColors;

        // Hide all characters
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;

            int matIndex = textInfo.characterInfo[i].materialReferenceIndex;
            int vertIndex = textInfo.characterInfo[i].vertexIndex;
            newVertexColors = textInfo.meshInfo[matIndex].colors32;

            for (int j = 0; j < 4; j++)
                newVertexColors[vertIndex + j].a = 0;
        }
        subtitlesTMP.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

        // Reveal characters one by one
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (skipRequested)
            {
                // Instantly show all characters
                for (int k = 0; k < textInfo.characterCount; k++)
                {
                    if (!textInfo.characterInfo[k].isVisible) continue;
                    int matIndex2 = textInfo.characterInfo[k].materialReferenceIndex;
                    int vertIndex2 = textInfo.characterInfo[k].vertexIndex;
                    newVertexColors = textInfo.meshInfo[matIndex2].colors32;
                    for (int j = 0; j < 4; j++)
                        newVertexColors[vertIndex2 + j].a = 255;
                }
                subtitlesTMP.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                // Reset reveal state and consume input
                isRevealing = false;
                skipRequested = false;
                yield return new WaitWhile(() => Input.anyKey && Time.timeScale > 0);
                yield break;
            }

            if (!textInfo.characterInfo[i].isVisible) continue;

            int matIndex = textInfo.characterInfo[i].materialReferenceIndex;
            int vertIndex = textInfo.characterInfo[i].vertexIndex;
            newVertexColors = textInfo.meshInfo[matIndex].colors32;

            for (int j = 0; j < 4; j++)
                newVertexColors[vertIndex + j].a = 255;

            subtitlesTMP.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            PlayCharacterSound();

            yield return new WaitForSeconds(charRevealDelay);
        }

        isRevealing = false;
    }

    private void PlayCharacterSound()
    {
        if (characterSoundEffects == null || audioSource == null || characterSoundEffects.Count == 0)
            return;

        int index = (characterSoundEffects.Count > 1)
            ? Random.Range(0, characterSoundEffects.Count)
            : 0;

        while (index == lastSound && characterSoundEffects.Count > 1)
            index = Random.Range(0, characterSoundEffects.Count);

        lastSound = index;

        audioSource.Stop();
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.clip = characterSoundEffects[index];
        audioSource.Play();
    }

    private IEnumerator FadeCanvas(bool fadeIn)
    {
        float start = subtitlesCanvasGroup.alpha;
        float end = fadeIn ? 1f : 0f;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            subtitlesCanvasGroup.alpha = Mathf.Lerp(start, end, timer / fadeDuration);
            yield return null;
        }

        subtitlesCanvasGroup.alpha = end;
    }

    // --- NEW HELPER ---
    private IEnumerator WaitForFreshKeyPress()
    {
        // Wait for any currently held key to be released
        yield return new WaitWhile(() => Input.anyKey && Time.timeScale > 0);

        // Now wait for the next *fresh* press
        yield return new WaitUntil(() => Input.anyKeyDown && Time.timeScale > 0);
    }

    public void StopCoroutines()
    {
        isShowingSubtitles = false;

        if (subtitleRoutine != null)
        {
            StopCoroutine(subtitleRoutine);
            subtitleRoutine = null;
        }

        if (subtitlesCanvasGroup != null)
            subtitlesCanvasGroup.alpha = 0f;

        if (subtitlesTMP != null)
        {
            subtitlesTMP.text = string.Empty;
            subtitlesTMP.ForceMeshUpdate();
            subtitlesTMP.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

            try { subtitlesTMP.canvasRenderer.Clear(); } catch { }
            Canvas.ForceUpdateCanvases();

            subtitlesTMP.enabled = false;
            subtitlesTMP.enabled = true;
        }
    }
}
