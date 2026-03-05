using Hertzole.GoldPlayer;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public CoreGameManager gameManager;
    public GameObject pauseUI;
    public GoldPlayerController playerController_neighborhood, playerController_car;
    public CursorController cursorController;
    public TextMeshProUGUI sensitivityTMP;
    public Slider sensitivitySlider;
    bool isPaused = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && gameManager.canPause)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void ResumeButtonPressed()
    {
        ResumeGame();
    }

    public void SetMouseSensitivityValues()
    {
        float value = sensitivitySlider.value;

        // value goes from 0 to 1
        // we map it to 0.5 → 2.5
        float mappedValue = Mathf.Lerp(0.25f, 3.75f, value);

        playerController_neighborhood.Camera.MouseSensitivity = new Vector2(mappedValue, mappedValue);
        playerController_car.Camera.MouseSensitivity = new Vector2(mappedValue, mappedValue);

        sensitivityTMP.text = mappedValue.ToString("F2");
    }

    public void PauseGame()
    {
        StopAllCoroutines();
        pauseUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        cursorController.enabled = false;
        playerController_neighborhood.enabled = false;
        playerController_car.enabled = false;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cursorController.enabled = true;
        pauseUI.SetActive(false);
        StartCoroutine(resumeTimeScale());
        playerController_car.enabled = true;
        playerController_neighborhood.enabled = true;
    }

    IEnumerator resumeTimeScale()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        Time.timeScale = 1f;
    }
}
