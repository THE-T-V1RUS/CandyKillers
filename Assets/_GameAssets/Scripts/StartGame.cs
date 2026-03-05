using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    public CoreGameManager gameManager;

    public AudioClip startButton;

    public Button button, quitButton;

    public CanvasGroup blackoutCanvas, cursorCanvas;

    public GameObject player, startCutscene, titleScreen, enableCursorObj;

    private void Start()
    {
        #if UNITY_WEBGL
            // Hide the quit button in WebGL
            if (quitButton != null)
            {
                quitButton.transform.gameObject.SetActive(false);
                button.transform.position = quitButton.transform.position;
            }
        #else
            // Ensure it's active on other platforms
            if (quitButton != null)
                quitButton.transform.gameObject.SetActive(true);
        #endif
    }

    public void StartGameButtonPressed()
    {
        StartCoroutine(StartGameCoroutine());
    }

    IEnumerator StartGameCoroutine()
    {
        enableCursorObj.SetActive(false);

        yield return new WaitForEndOfFrame();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        button.interactable = false;
        gameManager.PlaySoundEffect(startButton, 0.5f);

        while(blackoutCanvas.alpha < 1)
        {
            blackoutCanvas.alpha += Time.deltaTime * 0.4f;
            yield return null;
        }

        cursorCanvas.alpha = 1;

        player.SetActive(true);
        titleScreen.SetActive(false);

        yield return new WaitForSeconds(2f);

        startCutscene.SetActive(true);

        this.gameObject.SetActive(false);
    }

    public void QuitButton()
    {
        Application.Quit();
    }
}
