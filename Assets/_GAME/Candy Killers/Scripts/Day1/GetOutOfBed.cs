using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GetOutOfBed : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI storyTMP;
    [SerializeField] BlinkController blinkController;
    [SerializeField] Animator animator;
    [SerializeField] GameObject GetOutOfBedCamera;
    [SerializeField] GameManager gameManager;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip snd_GetOutOfBed;
    [SerializeField] TutorialManager tutorialManager;

    private void Start()
    {
        StartCoroutine(GetOutOfBedCoroutine());
    }

    public IEnumerator GetOutOfBedCoroutine()
    {
        blinkController.blinkAmount = 1;

        yield return new WaitForSeconds(1);

        Color textColor = Color.white;
        float alpha = 0f;
        textColor.a = alpha;

        storyTMP.text = "DAY 1";
        storyTMP.color = textColor;

        //FADE IN TEXT
        float duration = 1f;
        float startAlpha = 0f;
        float endAlpha = 1f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerpT = Mathf.Clamp01(t / duration);
            alpha = Mathf.Lerp(startAlpha, endAlpha, lerpT);
            textColor.a = alpha;
            storyTMP.color = textColor;

            yield return null;
        }

        textColor.a = endAlpha;
        storyTMP.color = textColor;

        //STOP BLINK AND FADE OUT
        StartCoroutine(StopBlink());

        yield return new WaitForSeconds(1f);

        //FADE OUT TEXT
        duration = 1f;
        startAlpha = 1f;
        endAlpha = 0f;
        t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerpT = Mathf.Clamp01(t / duration);
            alpha = Mathf.Lerp(startAlpha, endAlpha, lerpT);
            textColor.a = alpha;
            storyTMP.color = textColor;

            yield return null;
        }

        textColor.a = endAlpha;
        storyTMP.color = textColor;

        animator.SetBool("Play", true);

        yield return new WaitForSeconds(1f);

        audioSource.PlayOneShot(snd_GetOutOfBed);

        yield return new WaitForSeconds(1.3f);

        GetOutOfBedCamera.SetActive(false);

        yield return new WaitForSeconds(1f);

        List<string> dialogue = new List<string>();
        dialogue.Add("What is that noise?");
        dialogue.Add("Sounds like it's coming from downstairs.");
        gameManager.dialogueText.StartTeletype(dialogue, GameManager.GameMode.FPS);

        while(gameManager.currentGameMode != GameManager.GameMode.FPS)
        {
            yield return null;
        }

        tutorialManager.UpdateTeachingMode(TutorialManager.TeachStep.MoveAndLook);
    }

    public IEnumerator StopBlink()
    {
        //FADE IN TEXT
        float duration = 2f;
        float startBlink = 1f;
        float endBlink = 0f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerpT = Mathf.Clamp01(t / duration);
            lerpT = EasingLibrary.DoEase(EasingLibrary.EasingType.easeInBounce, lerpT);
            blinkController.blinkAmount = Mathf.Lerp(startBlink, endBlink, lerpT);
            yield return null;
        }

        blinkController.blinkAmount = endBlink;

        yield return new WaitForSeconds(0.25f);
        animator.enabled = true;
    }
}
