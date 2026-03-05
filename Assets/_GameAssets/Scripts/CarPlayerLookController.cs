using Unity.Cinemachine;
using UnityEngine;

public class CarPlayerLookController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform; // The child camera
    public GameObject carCam;
    public CinemachineBrain brain;

    [Header("Look Settings")]
    public float brainTransitionTime = 0.5f;
    public float sensitivityX = 2f; // Mouse sensitivity for horizontal
    public float sensitivityY = 2f; // Mouse sensitivity for vertical
    public float maxYAngle = 80f;   // Limit left/right (yaw)
    public float maxXAngle = 80f;   // Limit up/down (pitch)

    private float currentYaw = 0f;   // Left/right rotation
    private float currentPitch = 0f; // Up/down rotation

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if(brain != null )
            brain.DefaultBlend.Time = brainTransitionTime;
    }

    private void Update()
    {
        if (!carCam.activeInHierarchy) return;

        float mouseX = Input.GetAxis("Mouse X") * sensitivityX;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivityY;

        // Apply yaw rotation to the parent (horizontal look)
        currentYaw += mouseX;
        currentYaw = Mathf.Clamp(currentYaw, -maxYAngle, maxYAngle);

        // Apply pitch rotation to the camera (vertical look)
        currentPitch -= mouseY;
        currentPitch = Mathf.Clamp(currentPitch, -maxXAngle, maxXAngle);

        // Rotate the parent object only on Y-axis
        transform.localRotation = Quaternion.Euler(0f, currentYaw, 0f);

        // Rotate the camera on X-axis (up/down)
        cameraTransform.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
    }
}
