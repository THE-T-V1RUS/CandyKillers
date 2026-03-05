using Hertzole.GoldPlayer;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class TitleRevealController : MonoBehaviour
{
    [SerializeField] GameObject TitleRevealCutscene;
    [SerializeField] GoldPlayerController playerController;
    [SerializeField] AudioSource audioSource_PlayerVoice;
    [SerializeField] AudioClip snd_Emma_day1_WalkIntoGarage;
    [SerializeField] Transform forceLookTarget;
    [SerializeField] GameManager gameManager;
    [SerializeField] CinemachineCamera playerCamera;
    [SerializeField] float targetFOV = 25f;

    bool isTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            StartCoroutine(IntroCoroutine());
        }
    }

    IEnumerator IntroCoroutine()
    {
        playerController.Movement.CanMoveAround = false;
        playerController.Camera.CanLookAround = false;
        playerController.Camera.ForceLook(forceLookTarget.position);
        audioSource_PlayerVoice.PlayOneShot(snd_Emma_day1_WalkIntoGarage);
        gameManager.ChangeGameMode(GameManager.GameMode.Cutscene);

        yield return new WaitForSeconds(2.5f);
        TitleRevealCutscene.SetActive(true);

        float startFOV = 60f;
        float endFOV = targetFOV;
        float t = 0f;
        float duration = 3.5f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerpT = Mathf.Clamp01(t / duration);
            playerCamera.Lens.FieldOfView = Mathf.Lerp(startFOV, endFOV, lerpT);
            yield return null;
        }

        playerCamera.Lens.FieldOfView = endFOV;

        yield return new WaitForSeconds(4f);

        startFOV = targetFOV;
        endFOV = 60f;
        t = 0f;
        duration = 5f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerpT = Mathf.Clamp01(t / duration);
            playerCamera.Lens.FieldOfView = Mathf.Lerp(startFOV, endFOV, lerpT);
            yield return null;
        }

        playerCamera.Lens.FieldOfView = endFOV;
        playerController.Movement.CanMoveAround = true;
        playerController.Camera.CanLookAround = true;
        gameManager.ChangeGameMode(GameManager.GameMode.FPS);
        playerController.Camera.StopForceLooking();

        this.gameObject.SetActive(false);
    }
}
