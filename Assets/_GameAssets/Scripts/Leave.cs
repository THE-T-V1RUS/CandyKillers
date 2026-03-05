using Hertzole.GoldPlayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leave : MonoBehaviour
{
    public CoreGameManager gameManager;
    public GoldPlayerController playerController;
    public Transform lookAtTarget;
    public GameObject bucketCam;

    public List<string> kidLines_0;
    public List<string> momLines_0;

    public float pitchMin_mom, pitchMax_mom, pitchMin_kid, pitchMax_kid;
    public List<AudioClip> mom_audioClips;
    public List<AudioClip> kid_audioClips;

    public CanvasGroup blackoutGroup;
    public GameObject carScene_Obj, neighborhood_Obj;

    public AudioClip snd_Leave;

    public AudioSource ambienceAudio;

    public void ActivateLeaveTrigger()
    {
        StartCoroutine(SwitchScenes());
    }

    IEnumerator SwitchScenes()
    {
        gameManager.minCandyCountForBucketCam = 50;
        bucketCam.SetActive(false);
        playerController.Camera.CanLookAround = false;
        playerController.Movement.CanMoveAround = false;
        playerController.Camera.ForceLook(lookAtTarget, 5);

        yield return new WaitForSeconds(0.25f);
        while (gameManager.dialogueController.isShowingSubtitles)
            yield return null;

        gameManager.dialogueController.minPitch = pitchMin_mom;
        gameManager.dialogueController.maxPitch = pitchMax_mom;
        gameManager.dialogueController.characterSoundEffects = new List<AudioClip>(mom_audioClips);
        gameManager.dialogueController.ShowSubtitles(momLines_0, false);

        yield return new WaitForSeconds(0.25f);
        while (gameManager.dialogueController.isShowingSubtitles)
            yield return null;

        while(blackoutGroup.alpha < 1)
        {
            blackoutGroup.alpha += Time.deltaTime;
            yield return null;
        }

        gameManager.PlaySoundEffect(snd_Leave, 1);

        while (ambienceAudio.volume > 0)
        {
            ambienceAudio.volume -= Time.deltaTime * 0.2f;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(9);

        gameManager.MoveCandyToCarBucket();
        carScene_Obj.SetActive(true);
        neighborhood_Obj.SetActive(false);
    }
}
