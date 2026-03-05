using UnityEngine;
using System.Collections;

public class Intro : MonoBehaviour
{
    public CanvasGroup canvas0, canvas1, canvas2;
    public GameObject startGameObj;

    public CoreGameManager gameManager;

    public AudioClip sndSwell;

    public float fadeDuration = 1f;   // time to fade in/out
    public float showDuration = 2f;   // time to stay fully visible

    private void Start()
    {
        // Start the intro sequence
        if (gameManager.isDebugMode)
        {
            startGameObj.SetActive(true);
            gameObject.SetActive(false);
        }
        else
        {
            StartCoroutine(IntroSequence());
        }
    }

    private IEnumerator IntroSequence()
    {
        // Ensure all canvases start invisible
        canvas0.alpha = 0f;
        canvas1.alpha = 0f;
        canvas2.alpha = 0f;

        // Fade in/out each canvas in sequence
        yield return FadeCanvas(canvas0);
        yield return FadeCanvas(canvas1);
        yield return FadeCanvas(canvas2);

        // Enable start game object
        startGameObj.SetActive(true);

        // Disable this intro object
        gameObject.SetActive(false);
    }

    private IEnumerator FadeCanvas(CanvasGroup canvas)
    {
        if(canvas == canvas2)
            gameManager.PlaySoundEffect(sndSwell, 0.25f);

        // Fade in
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvas.alpha = Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }

        canvas.alpha = 1f;

        // Wait while fully visible
        yield return new WaitForSeconds(showDuration);

        // Fade out
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvas.alpha = Mathf.Clamp01(1f - (timer / fadeDuration));
            yield return null;
        }

        canvas.alpha = 0f;
    }
}
