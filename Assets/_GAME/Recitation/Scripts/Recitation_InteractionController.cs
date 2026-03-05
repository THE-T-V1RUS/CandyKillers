using UnityEngine;
using UnityEngine.InputSystem;

public class Recitation_InteractionController : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private Vector3 interactionBoxSize = new Vector3(2f, 2f, 3f);
    [SerializeField] private Vector3 interactionBoxOffset = new Vector3(0f, 0f, 1.5f);
    [SerializeField] private float maxInteractionDistance = 5f;
    [SerializeField] private LayerMask interactableLayer = -1; // All layers by default
    
    [Header("Raycast Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float raycastDistance = 5f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private Color gizmoColor = Color.green;
    [SerializeField] private Color raycastHitColor = Color.yellow;
    
    private Camera playerCamera;
    private GameObject currentLookTarget;
    
    private void Start()
    {
        // Get the camera if not assigned
        if (cameraTransform == null)
        {
            playerCamera = Camera.main;
            if (playerCamera != null)
                cameraTransform = playerCamera.transform;
        }
        else
        {
            playerCamera = cameraTransform.GetComponent<Camera>();
        }
    }
    
    private void Update()
    {
        // Check what we're looking at
        CheckLookTarget();
        
        // Check for left mouse click
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryInteract();
        }
    }
    
    private void CheckLookTarget()
    {
        currentLookTarget = null;
        
        if (cameraTransform == null)
            return;
        
        // Perform raycast from camera
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, raycastDistance, interactableLayer))
        {
            // Check if the hit object is tagged as Interactable
            if (hit.collider.CompareTag("Interactable"))
            {
                currentLookTarget = hit.collider.gameObject;
            }
        }
    }
    
    private void TryInteract()
    {
        if (currentLookTarget == null)
            return;
        
        // Check if the target is within the interaction box
        if (IsInInteractionRange(currentLookTarget))
        {
            // Perform interaction
            Interact(currentLookTarget);
        }
    }
    
    private bool IsInInteractionRange(GameObject target)
    {
        if (target == null)
            return false;
        
        // Calculate the center of the interaction box in world space
        Vector3 boxCenter = transform.position + transform.TransformDirection(interactionBoxOffset);
        
        // Get all colliders within the box
        Collider[] colliders = Physics.OverlapBox(boxCenter, interactionBoxSize / 2f, transform.rotation, interactableLayer);
        
        // Check if our target is in the box
        foreach (Collider col in colliders)
        {
            if (col.gameObject == target)
            {
                // Additional distance check
                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance <= maxInteractionDistance)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    private void Interact(GameObject target)
    {
        // Send interaction message to the target
        target.SendMessage("OnInteract", SendMessageOptions.DontRequireReceiver);
        
        // You can also use an interface if you prefer
        IInteractable interactable = target.GetComponent<IInteractable>();
        if (interactable != null)
        {
            interactable.OnInteract();
        }
        
        Debug.Log($"Interacted with: {target.name}");
    }
    
    // Public method to check if we're currently looking at an interactable
    public GameObject GetCurrentLookTarget()
    {
        return currentLookTarget;
    }
    
    // Public method to check if a specific object is in range
    public bool IsObjectInRange(GameObject obj)
    {
        return IsInInteractionRange(obj);
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos)
            return;
        
        // Draw the interaction box
        Gizmos.color = gizmoColor;
        Vector3 boxCenter = transform.position + transform.TransformDirection(interactionBoxOffset);
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawWireCube(Vector3.zero, interactionBoxSize);
        
        // Draw raycast line in play mode
        if (Application.isPlaying && cameraTransform != null)
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = currentLookTarget != null ? raycastHitColor : Color.red;
            Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * raycastDistance);
            
            // Draw a sphere at the look target
            if (currentLookTarget != null)
            {
                Gizmos.color = raycastHitColor;
                Gizmos.DrawWireSphere(currentLookTarget.transform.position, 0.3f);
            }
        }
    }
}
