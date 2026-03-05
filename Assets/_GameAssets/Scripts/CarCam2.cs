using UnityEngine;

public class PlayerLookController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform; // Child camera transform
    public GameObject bucketCam;

    [Header("Settings")]
    public float sensitivityX = 2f;
    public float sensitivityY = 2f;
    public float maxYaw = 80f;   // Left/right rotation limit (degrees)
    public float maxPitch = 80f; // Up/down rotation limit (degrees)

    private float yawDelta = 0f;
    private float pitchDelta = 0f;

    private Quaternion initialParentRot;
    private Quaternion initialCameraRot;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Store the initial local rotations to use as the "neutral" orientation
        initialParentRot = transform.localRotation;
        initialCameraRot = cameraTransform.localRotation;
    }

    private void Update()
    {
        if (!bucketCam.activeInHierarchy) return;

        float mouseX = Input.GetAxis("Mouse X") * sensitivityX;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivityY;

        // Apply mouse deltas
        yawDelta += mouseX;
        pitchDelta -= mouseY;

        // Clamp around starting rotation
        yawDelta = Mathf.Clamp(yawDelta, -maxYaw, maxYaw);
        pitchDelta = Mathf.Clamp(pitchDelta, -maxPitch, maxPitch);

        // Apply rotations relative to the starting orientation
        transform.localRotation = initialParentRot * Quaternion.Euler(0f, yawDelta, 0f);
        cameraTransform.localRotation = initialCameraRot * Quaternion.Euler(pitchDelta, 0f, 0f);
    }
}
