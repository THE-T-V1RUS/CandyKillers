using Hertzole.GoldPlayer;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Cellphone : MonoBehaviour
{
    bool isInteracting;
    [SerializeField] CinemachineCamera phoneCamera;
    [SerializeField] Transform CameraHead, PhoneTableLocation;
    [SerializeField] GoldPlayerController playerController;
    [SerializeField] float moveDuration = 1f;
    [SerializeField] AudioVibration audioVibration;
    [SerializeField] BoxCollider phoneCollider;
    GameManager gameManager;

    private Coroutine moveCoroutine;

    private void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        gameManager = GameManager.Instance;
        if(gameManager == null)
        {
            Initialize();
            return;
        }
    }

    public void InteractWithPhone()
    {
        if (!isInteracting)
        {
            StartInteraction();
        }
    }

    public void SetInteractable(bool canInteract)
    {
        phoneCollider.enabled = canInteract;
    }

    void StartInteraction()
    {
        isInteracting = true;
        phoneCamera.enabled = true;
        playerController.Movement.CanMoveAround = false;
        playerController.Camera.CanLookAround = false;
        playerController.Camera.ShouldLockCursor = false;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        gameManager.ChangeGameMode(GameManager.GameMode.Interact);

        // Get target transform
        Transform cam = CameraHead;
        Vector3 targetPos = cam.position + cam.forward * 0.25f + cam.up * 1.6f;
        Quaternion targetRot = Quaternion.LookRotation(-cam.forward, cam.up);
        targetRot.eulerAngles = new Vector3(-30f, targetRot.eulerAngles.y, targetRot.eulerAngles.z);

        // Smoothly move phone to target
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        audioVibration.ChangeVibrationAmount(0.001f);
        audioVibration.ChangeHighpassFilter(true);
        moveCoroutine = StartCoroutine(MoveToTarget(targetPos, targetRot, moveDuration));
    }

    public void StopInteraction()
    {
        StartCoroutine(StopInteractionCoroutine());
    }

    IEnumerator StopInteractionCoroutine()
    {
        isInteracting = false;
        phoneCamera.enabled = false;
        gameManager.ChangeGameMode(GameManager.GameMode.FPS);

        // Get target transform
        Transform cam = PhoneTableLocation;
        Vector3 targetPos = PhoneTableLocation.position;
        Quaternion targetRot = PhoneTableLocation.rotation;

        // Smoothly move phone to target
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveToTarget(targetPos, targetRot, moveDuration));

        yield return new WaitForSeconds(1f);
        
        if(gameManager.currentGameMode != GameManager.GameMode.Dialogue)
        {
            playerController.Movement.CanMoveAround = true;
            playerController.Camera.CanLookAround = true;
            playerController.Camera.ShouldLockCursor = true;
        }
    }

    IEnumerator MoveToTarget(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
    }
}
