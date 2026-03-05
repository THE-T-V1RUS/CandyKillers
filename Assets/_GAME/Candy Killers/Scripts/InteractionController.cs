using Hertzole.GoldPlayer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionController : MonoBehaviour
{
    public enum CurrentCursor
    {
        Normal,
        Interact,
        Inspect,
    }

    [SerializeField] Image img_Crosshair;
    [SerializeField] Sprite spr_NormalCrosshair, srp_InteractCrosshair, spr_InspectCrosshair;
    [SerializeField] TextMeshProUGUI tmp_interactionPrompt;
    [SerializeField] float interactDistance = 1.5f;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] GoldPlayerController playerController;

    [Header("Box Range Detection")]
    [SerializeField] private Vector3 interactionBoxSize = new Vector3(2f, 2f, 3f);
    [SerializeField] private Vector3 interactionBoxOffset = new Vector3(0f, 0f, 1.5f);
    [SerializeField] private bool useBoxRangeCheck = true;
    [SerializeField] private bool showDebugGizmos = true;

    public CurrentCursor currentCursor;

    private void Start()
    {
        img_Crosshair.sprite = spr_NormalCrosshair;
        img_Crosshair.color = new Color(1f, 1f, 1f, 0.15f);
        img_Crosshair.transform.localScale = Vector3.one * 0.15f;
    }

    private void Update()
    {
        tmp_interactionPrompt.text = string.Empty;
        
        if (!playerController.Movement.CanMoveAround || !playerController.Camera.CanLookAround)
        {
            SetNormalCrosshair();
            return;
        }

        // Use camera position and direction for proper first-person raycasting
        Vector3 rayOrigin = playerController.Camera.CameraHead.position;
        Vector3 rayDirection = playerController.Camera.CameraHead.forward;
        Ray ray = new Ray(rayOrigin, rayDirection);
        
        // Debug ray visualization
        Color rayColor = Color.red;
        
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer))
        {
            rayColor = Color.yellow; // Hit something in the layer
            
            if (hit.collider.CompareTag("Interactable"))
            {
                // Check if item is in box range (if enabled) BEFORE updating UI
                if (useBoxRangeCheck && !IsInInteractionRange(hit.collider.gameObject))
                {
                    rayColor = Color.yellow; // Hit interactable but not in range
                    SetNormalCrosshair();
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, rayColor);
                    return;
                }
                
                rayColor = Color.green; // Hit an interactable and in range
                
                // Get the Interactable component and update the prompt text
                var interactable = hit.transform.GetComponent<Interactable>();
                if (interactable != null)
                {
                    tmp_interactionPrompt.text = interactable.interactText;
                }
                
                SetInteractCrosshair();

                if (Input.GetMouseButtonDown(0))
                {
                    var obj_interaction = hit.transform.GetComponent<Interactable>();
                    if (obj_interaction == null) return;
                    obj_interaction.Interact();
                }
            }
            else
            {
                SetNormalCrosshair();
            }
            
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, rayColor);
        }
        else
        {
            SetNormalCrosshair();
            Debug.DrawRay(ray.origin, ray.direction * interactDistance, rayColor);
        tmp_interactionPrompt.text = string.Empty;
        }
    }

    void SetNormalCrosshair()
    {
        img_Crosshair.sprite = spr_NormalCrosshair;
        img_Crosshair.color = new Color(1f, 1f, 1f, 0.15f);
        img_Crosshair.transform.localScale = Vector3.one * 0.15f;
        currentCursor = CurrentCursor.Normal;
    }

    void SetInteractCrosshair()
    {
        img_Crosshair.sprite = srp_InteractCrosshair;
        img_Crosshair.color = new Color(1f, 1f, 1f, 0.5f);
        img_Crosshair.transform.localScale = Vector3.one * 0.15f;
        currentCursor = CurrentCursor.Interact;
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
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos)
            return;

        // Draw the interaction box
        Gizmos.color = Color.green;
        Vector3 boxCenter = transform.position + transform.TransformDirection(interactionBoxOffset);
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawWireCube(Vector3.zero, interactionBoxSize);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
