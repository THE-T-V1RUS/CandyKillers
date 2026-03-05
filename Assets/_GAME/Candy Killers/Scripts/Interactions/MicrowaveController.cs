using Hertzole.GoldPlayer;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class MicrowaveController : MonoBehaviour
{
    [SerializeField] CinemachineCamera MicrowaveCam;
    [SerializeField] Transform MicrowaveDoor, FoodRotator;
    [SerializeField] GameObject BurritoOverlay, BurritoWorld, BurritoOverlaySteam, BurritoWorldSteam;
    [SerializeField] AudioSource MicrowaveAudioSource, MicrowaveCookAudioSource;
    [SerializeField] AudioClip snd_DoorOpen, snd_DoorClose, snd_SetPlate, snd_MicrowaveButton, snd_MicrowaveDone;
    [SerializeField] GoldPlayerController playerController;
    [SerializeField] GameManager gameManager;
    [SerializeField] Light microwaveLight;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] Cellphone cellphone;
    [SerializeField] Edit_Photos editPhotos;
    [SerializeField] GarageDoorBlocker garageDoorBlocker;
    public BoxCollider interactionCollider;
    private int interactionMode = 0;
    private bool isCooking;

    private void Update()
    {
        if (isCooking && FoodRotator != null)
        {
            FoodRotator.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
        }
    }

    public void MicrowaveInteractions()
    {
        switch (interactionMode)
        {
            default:
            case 0:
                CookBurrito();
                break;
            case 1:
                RetrieveBurrito();
                break;
        }

        interactionMode++;
    }

    public void CookBurrito()
    {
        StartCoroutine(CookBurritoCoroutine());
    }

    IEnumerator CookBurritoCoroutine()
    {
        interactionCollider.enabled = false;
        MicrowaveCam.enabled = true;
        playerController.Movement.CanMoveAround = false;
        playerController.Camera.CanLookAround = false;
        gameManager.ChangeGameMode(GameManager.GameMode.Cutscene);

        // Open microwave door
        MicrowaveAudioSource.clip = snd_DoorOpen;
        MicrowaveAudioSource.Play();

        float doorOpenDuration = 0.4f;
        Quaternion doorStartRot = MicrowaveDoor.localRotation;
        Quaternion doorEndRot = Quaternion.Euler(0f, 130f, 0f);
        float t = 0f;
        bool lightOn = false;

        while (t < doorOpenDuration)
        {
            t += Time.deltaTime;
            float lerpT = Mathf.Clamp01(t / doorOpenDuration);
            MicrowaveDoor.localRotation = Quaternion.Lerp(doorStartRot, doorEndRot, lerpT);

            // Turn on light once partially open
            if (!lightOn && lerpT > 0.2f)
            {
                microwaveLight.enabled = true;
                lightOn = true;
            }

            yield return null;
        }

        // Plate burrito
        BurritoOverlay.SetActive(false);
        BurritoWorld.SetActive(true);

        Vector3 startPos = BurritoWorld.transform.position;
        Vector3 endPos = FoodRotator.position;
        Quaternion startRot = BurritoWorld.transform.rotation;
        Quaternion endRot = FoodRotator.rotation;
        float plateMoveDuration = 0.7f;
        float p = 0f;

        while (p < plateMoveDuration)
        {
            p += Time.deltaTime;
            float lerpP = Mathf.Clamp01(p / plateMoveDuration);
            BurritoWorld.transform.position = Vector3.Lerp(startPos, endPos, lerpP);
            BurritoWorld.transform.rotation = Quaternion.Lerp(startRot, endRot, lerpP);
            yield return null;
        }

        // Snap into place and parent
        BurritoWorld.transform.position = endPos;
        BurritoWorld.transform.rotation = endRot;
        BurritoWorld.transform.SetParent(FoodRotator);

        MicrowaveAudioSource.PlayOneShot(snd_SetPlate);

        // Close microwave door
        doorStartRot = MicrowaveDoor.localRotation;
        doorEndRot = Quaternion.Euler(0f, 0f, 0f);
        t = 0f;
        bool closeSoundPlayed = false;

        while (t < doorOpenDuration)
        {
            t += Time.deltaTime;
            float lerpT = Mathf.Clamp01(t / doorOpenDuration);
            MicrowaveDoor.localRotation = Quaternion.Lerp(doorStartRot, doorEndRot, lerpT);

            float angle = Quaternion.Angle(MicrowaveDoor.localRotation, doorEndRot);
            if (!closeSoundPlayed && angle < 50f)
            {
                MicrowaveAudioSource.PlayOneShot(snd_DoorClose);
                closeSoundPlayed = true;
            }

            yield return null;
        }

        MicrowaveDoor.localRotation = doorEndRot;
        microwaveLight.enabled = false;
        MicrowaveAudioSource.PlayOneShot(snd_MicrowaveButton);

        yield return new WaitForSeconds(0.5f);

        MicrowaveCookAudioSource.Play();
        microwaveLight.enabled = true;
        isCooking = true;

        yield return new WaitForSeconds(0.25f);

        MicrowaveCam.enabled = false;
        gameManager.ChangeGameMode(GameManager.GameMode.Dialogue);

        List<string> dialogue = new List<string>();
        dialogue.Add("While that's cooking I can deal with those last few photos on the cellphone.");
        dialogue.Add("So much cropping and deleting, but I'm almost done with all of them.");

        gameManager.ChangeGameMode(GameManager.GameMode.FPS);
        gameManager.dialogueText.StartTeletype(dialogue);
        cellphone.SetInteractable(true);
        editPhotos.enabled = true;
    }

    public void FinishedCooking()
    {
        MicrowaveAudioSource.PlayOneShot(snd_MicrowaveDone);
        MicrowaveCookAudioSource.Stop();
        isCooking = false;
        microwaveLight.enabled = false;
        //instantly turn playerController.transform.rotation so its y rotation faces this transform
        Vector3 direction = transform.position - playerController.transform.position;
        direction.y = 0f; // Ignore vertical difference

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Vector3 euler = lookRotation.eulerAngles;
            playerController.transform.rotation = Quaternion.Euler(0f, euler.y, 0f);
        }

        playerController.Camera.ForceLook(BurritoWorld.transform.position);
    }

    public void RetrieveBurrito()
    {
        StartCoroutine(RetrieveBurritoCoroutine());
    }

    IEnumerator RetrieveBurritoCoroutine()
    {
        BurritoOverlaySteam.SetActive(true);
        BurritoWorldSteam.SetActive(true);
        interactionCollider.enabled = false;
        MicrowaveCam.enabled = true;
        playerController.Movement.CanMoveAround = false;
        playerController.Camera.CanLookAround = false;
        gameManager.ChangeGameMode(GameManager.GameMode.Cutscene);

        // Open microwave door
        MicrowaveAudioSource.clip = snd_DoorOpen;
        MicrowaveAudioSource.Play();

        float doorOpenDuration = 0.4f;
        Quaternion doorStartRot = MicrowaveDoor.localRotation;
        Quaternion doorEndRot = Quaternion.Euler(0f, 130f, 0f);
        float t = 0f;
        bool lightOn = false;

        while (t < doorOpenDuration)
        {
            t += Time.deltaTime;
            float lerpT = Mathf.Clamp01(t / doorOpenDuration);
            MicrowaveDoor.localRotation = Quaternion.Lerp(doorStartRot, doorEndRot, lerpT);

            // Turn on light once partially open
            if (!lightOn && lerpT > 0.2f)
            {
                microwaveLight.enabled = true;
                lightOn = true;
            }

            yield return null;
        }

        MicrowaveCam.enabled = false;
        Vector3 startPos = BurritoWorld.transform.position;
        Vector3 endPos = BurritoOverlay.transform.position;
        Quaternion startRot = BurritoWorld.transform.rotation;
        Quaternion endRot = BurritoOverlay.transform.rotation;
        float plateMoveDuration = 0.7f;
        float p = 0f;
        MicrowaveAudioSource.PlayOneShot(snd_SetPlate);
        while (p < plateMoveDuration)
        {
            p += Time.deltaTime;
            float lerpP = Mathf.Clamp01(p / plateMoveDuration);
            BurritoWorld.transform.position = Vector3.Lerp(startPos, endPos, lerpP);
            BurritoWorld.transform.rotation = Quaternion.Lerp(startRot, endRot, lerpP);
            yield return null;
        }

        // Snap into place and parent
        BurritoWorld.transform.position = endPos;
        BurritoWorld.transform.rotation = endRot;
        BurritoWorld.transform.SetParent(BurritoOverlay.transform.parent);

        // Plate burrito
        BurritoOverlay.SetActive(true);
        BurritoWorld.SetActive(false);

        // Close microwave door
        doorStartRot = MicrowaveDoor.localRotation;
        doorEndRot = Quaternion.Euler(0f, 0f, 0f);
        t = 0f;
        bool closeSoundPlayed = false;

        while (t < doorOpenDuration)
        {
            t += Time.deltaTime;
            float lerpT = Mathf.Clamp01(t / doorOpenDuration);
            MicrowaveDoor.localRotation = Quaternion.Lerp(doorStartRot, doorEndRot, lerpT);

            float angle = Quaternion.Angle(MicrowaveDoor.localRotation, doorEndRot);
            if (!closeSoundPlayed && angle < 50f)
            {
                MicrowaveAudioSource.PlayOneShot(snd_DoorClose);
                closeSoundPlayed = true;
            }

            yield return null;
        }

        MicrowaveDoor.localRotation = doorEndRot;
        microwaveLight.enabled = false;

        yield return new WaitForSeconds(0.5f);

        garageDoorBlocker.ToggleBlocker(false);
        gameManager.ChangeGameMode(GameManager.GameMode.Dialogue);

        List<string> dialogue = new List<string>();
        dialogue.Add("Better take this to the garage before it gets cold.");

        gameManager.ChangeGameMode(GameManager.GameMode.FPS);
        gameManager.dialogueText.StartTeletype(dialogue);
    }
}
