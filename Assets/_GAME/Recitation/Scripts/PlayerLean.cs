using UnityEngine;
using Hertzole.GoldPlayer;

public class PlayerLean : MonoBehaviour
{
    [Header("Lean Settings")]
    [SerializeField] private float leanAngle = 15f;
    [SerializeField] private float leanSpeed = 5f;
    [SerializeField] private float leanRadius = 0.3f; // Distance from rotation pivot
    [SerializeField] private KeyCode leanLeftKey = KeyCode.Q;
    [SerializeField] private KeyCode leanRightKey = KeyCode.E;
    
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    
    private float currentLeanAngle = 0f;
    private float targetLeanAngle = 0f;
    private Vector3 previousLeanOffset = Vector3.zero;
    private GoldPlayerController playerController;
    
    private void Start()
    {
        playerController = GetComponent<GoldPlayerController>();
        
        // If no camera transform assigned, try to find it
        if (cameraTransform == null && playerController != null && playerController.Camera.CameraHead != null)
        {
            cameraTransform = playerController.Camera.CameraHead;
        }
        
        if (cameraTransform == null)
        {
            Debug.LogError("PlayerLean: No camera transform found!");
            enabled = false;
        }
    }
    
    private void LateUpdate()
    {
        HandleLeanInput();
        ApplyLean();
    }
    
    private void HandleLeanInput()
    {
        bool leaningLeft = Input.GetKey(leanLeftKey);
        bool leaningRight = Input.GetKey(leanRightKey);
        
        if (leaningLeft && !leaningRight)
        {
            // Lean left
            targetLeanAngle = leanAngle;
        }
        else if (leaningRight && !leaningLeft)
        {
            // Lean right
            targetLeanAngle = -leanAngle;
        }
        else
        {
            // Return to center
            targetLeanAngle = 0f;
        }
    }
    
    private void ApplyLean()
    {
        // Smoothly interpolate to target lean angle
        currentLeanAngle = Mathf.Lerp(currentLeanAngle, targetLeanAngle, leanSpeed * Time.deltaTime);
        
        // Calculate position offset based on lean angle
        // This creates the natural arc motion of leaning to the side
        float angleInRadians = currentLeanAngle * Mathf.Deg2Rad;
        float xOffset = -Mathf.Sin(angleInRadians) * leanRadius; // Negative so left lean goes left
        float zOffset = (1f - Mathf.Cos(Mathf.Abs(angleInRadians))) * leanRadius;
        
        Vector3 currentLeanOffset = new Vector3(xOffset, 0f, zOffset);
        
        // Get current camera state (after GoldPlayer has updated it)
        Quaternion baseRotation = cameraTransform.localRotation;
        Vector3 basePosition = cameraTransform.localPosition;
        
        // Remove the previous lean offset to get the clean position
        basePosition -= previousLeanOffset;
        
        // Create rotation around Z axis
        Quaternion leanRotation = Quaternion.Euler(0f, 0f, currentLeanAngle);
        
        // Apply the rotation to the camera
        cameraTransform.localRotation = baseRotation * leanRotation;
        
        // Apply the new position offset (creates the peering motion)
        cameraTransform.localPosition = basePosition + currentLeanOffset;
        
        // Store for next frame
        previousLeanOffset = currentLeanOffset;
    }
    
    private void OnDisable()
    {
        // Reset lean when disabled
        if (cameraTransform != null)
        {
            // Remove the lean offset from current position
            cameraTransform.localPosition -= previousLeanOffset;
            
            // Remove the lean rotation
            Quaternion leanRotation = Quaternion.Euler(0f, 0f, currentLeanAngle);
            cameraTransform.localRotation = cameraTransform.localRotation * Quaternion.Inverse(leanRotation);
            
            // Reset tracking variables
            currentLeanAngle = 0f;
            targetLeanAngle = 0f;
            previousLeanOffset = Vector3.zero;
        }
    }
}
