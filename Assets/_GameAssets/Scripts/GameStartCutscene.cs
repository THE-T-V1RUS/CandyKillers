using Hertzole.GoldPlayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartCutscene : MonoBehaviour
{
    [System.Serializable]
    public class Dialogue
    {
        public List<string> textList = new List<string>();
    }

    public GoldPlayerController playerController;
    public CoreGameManager gameManager;
    public Transform momTarget;
    public CanvasGroup blackoutCanvasGroup;
    public List<Dialogue> dialogueText;
    public float pitchMin_mom, pitchMax_mom, pitchMin_kid, pitchMax_kid;
    public List<AudioClip> mom_audioClips;
    public List<AudioClip> kid_audioClips;

    public CanvasGroup tutorial_0_CanvasGroup, tutorial_1_CanvasGroup;

    public float startingDelay;

    private bool hasPressedSpacebar = false;

    private void Update()
    {
        if (!hasPressedSpacebar && Input.GetKeyDown(KeyCode.Space))
        {
            hasPressedSpacebar = true;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        blackoutCanvasGroup.alpha = 1;
        playerController.Camera.CanLookAround = false;
        playerController.Movement.CanMoveAround = false;
        playerController.Camera.ForceLook(momTarget, 5);

        StartCoroutine(StartingCutsceneCoroutine());
    }

    IEnumerator StartingCutsceneCoroutine()
    {
        yield return new WaitForSeconds(startingDelay);

        float fadeDuration = 2f; // duration of the fade in seconds
        float timer = 0f;
        float startAlpha = blackoutCanvasGroup.alpha;
        float endAlpha = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);

            // Smooth interpolation
            blackoutCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            yield return null;
        }

        blackoutCanvasGroup.alpha = endAlpha;

        gameManager.dialogueController.minPitch = pitchMin_mom;
        gameManager.dialogueController.maxPitch = pitchMax_mom;
        gameManager.dialogueController.characterSoundEffects = new List<AudioClip>(mom_audioClips);
        gameManager.dialogueController.ShowSubtitles(dialogueText[0].textList, false);

        yield return new WaitForSeconds(0.25f);
        yield return new WaitUntil(() => !gameManager.dialogueController.isShowingSubtitles);
        yield return new WaitForSeconds(0.25f);

        gameManager.dialogueController.minPitch = pitchMin_kid;
        gameManager.dialogueController.maxPitch = pitchMax_kid;
        gameManager.dialogueController.characterSoundEffects = new List<AudioClip>(kid_audioClips);
        gameManager.dialogueController.ShowSubtitles(dialogueText[1].textList, false);

        yield return new WaitForSeconds(0.25f);
        yield return new WaitUntil(() => !gameManager.dialogueController.isShowingSubtitles);
        yield return new WaitForSeconds(0.25f);

        // Cutscene fade finished, you can enable player controls if desired
        playerController.Camera.StopForceLooking();
        playerController.Camera.CanLookAround = true;
        playerController.Movement.CanMoveAround = true;

        gameManager.PlaySoundEffect(gameManager.snd_Tutorial, 0.25f);

        fadeDuration = 2f; // duration of the fade in seconds
        timer = 0f;
        startAlpha = 0f;
        endAlpha = 1f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);

            // Smooth interpolation
            tutorial_0_CanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            yield return null;
        }
        tutorial_0_CanvasGroup.alpha = 1f;

        yield return new WaitForSeconds(5f);

        fadeDuration = 2f; // duration of the fade in seconds
        timer = 0f;
        startAlpha = 1f;
        endAlpha = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);

            // Smooth interpolation
            tutorial_0_CanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            yield return null;
        }
        tutorial_0_CanvasGroup.alpha = 0f;
    }

    public void ShowTutorial1()
    {
        StartCoroutine(ShowTutorial_1());
    }

    IEnumerator ShowTutorial_1()
    {
        hasPressedSpacebar = false;

        gameManager.PlaySoundEffect(gameManager.snd_Tutorial, 0.25f);

        float fadeDuration = 2f; // duration of the fade in seconds
        float timer = 0f;
        float startAlpha = 0f;
        float endAlpha = 1f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);

            // Smooth interpolation
            tutorial_1_CanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            yield return null;
        }

        tutorial_1_CanvasGroup.alpha = 1f;

        yield return new WaitUntil(() => hasPressedSpacebar);

        fadeDuration = 2f; // duration of the fade in seconds
        timer = 0f;
        startAlpha = 1f;
        endAlpha = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);

            // Smooth interpolation
            tutorial_1_CanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            yield return null;
        }
        tutorial_1_CanvasGroup.alpha = 0f;
    }
}
