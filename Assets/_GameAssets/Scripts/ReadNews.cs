using Hertzole.GoldPlayer;
using System.Collections.Generic;
using UnityEngine;

public class ReadNews : MonoBehaviour
{
    public CoreGameManager gameManager;
    public int newsID = 0;
    public GameObject newsPaperPanel;
    public List<GameObject> newsPapers;
    public bool isReading = false;
    public GoldPlayerController playerController;
    public CursorController cursorController;
    public AudioClip snd_StartReading, snd_StopReading;

    private float delay = 0;

    private void Update()
    {
        if(isReading)
        {
            if(delay < 0.25f)
            {
                delay += Time.deltaTime;
                return;
            }

            if (Input.anyKeyDown)
            {
                StopReadNews();
            }
        }
    }

    public void StartReadNews()
    {
        delay = 0;
        gameManager.PlaySoundEffect(snd_StartReading, 0.3f);
        playerController.Camera.CanLookAround = false;
        playerController.Movement.CanMoveAround = false;
        cursorController.enabled = false;
        newsPaperPanel.SetActive(true);
        newsPapers[newsID].SetActive(true);
        isReading = true;
    }

    public void StopReadNews()
    {
        gameManager.PlaySoundEffect(snd_StopReading, 0.3f);
        playerController.Camera.CanLookAround = true;
        playerController.Movement.CanMoveAround = true;
        cursorController.enabled = true;
        newsPaperPanel.SetActive(false);
        newsPapers[newsID].SetActive(false);
        isReading = false;
    }
}
