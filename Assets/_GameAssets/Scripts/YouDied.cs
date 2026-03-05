using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class YouDied : MonoBehaviour
{
    public CoreGameManager gameManager;
    public CanvasGroup youDiedGroup, blackoutGroup, uiGroup, buttonsGroup;
    public Animator animator;
    public AudioSource ambience;
    public Button menuButton;
    public GameObject gameOverObj, carSceneObj, rewindObj;
    public Material screenOverlayMat;

    private void OnEnable()
    {
        StartCoroutine(TurnUpAmbience());
        StartCoroutine(ShowGameOver());
    }

    IEnumerator TurnUpAmbience()
    {
        gameManager.canPause = false;

        while (ambience.volume < 0.2f)
        {
            ambience.volume += Time.deltaTime * 0.5f;
            yield return null;
        }

        animator.SetBool("Start", true);
    }

    IEnumerator ShowGameOver()
    {
        blackoutGroup.alpha = 1;
        youDiedGroup.alpha = 0;
        uiGroup.alpha = 0;

        while (blackoutGroup.alpha > 0)
        {
            blackoutGroup.alpha -= Time.deltaTime * 0.5f;
            yield return null;
        }

        while (youDiedGroup.alpha < 1)
        {
            youDiedGroup.alpha += Time.deltaTime * 0.5f;
            yield return null;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        menuButton.interactable = true;

        while (buttonsGroup.alpha < 1)
        {
            buttonsGroup.alpha += Time.deltaTime * 0.5f;
            yield return null;
        }
    }

    public void retry()
    {
        StartCoroutine(restartCarScene());
    }

    IEnumerator restartCarScene()
    {
        gameManager.RevealHints();

        youDiedGroup.alpha = 0f;
        rewindObj.SetActive(true);

        screenOverlayMat.SetFloat("_Enabled", 1f);

        yield return new WaitForSeconds(1);

        foreach(Transform candy in gameManager.candyObjs)
        {
            candy.gameObject.SetActive(true);
        }

        screenOverlayMat.SetFloat("_Enabled", 0f);

        blackoutGroup.alpha = 1f;

        uiGroup.alpha = 1f;

        carSceneObj.SetActive(true);

        rewindObj.SetActive(false);
        gameOverObj.SetActive(false);
    }
}
