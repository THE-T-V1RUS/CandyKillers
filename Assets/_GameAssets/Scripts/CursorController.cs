using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{
    public Sprite spr_CursorNormal, spr_CursorInteract;
    public Image img_Cursor;
    public TextMeshProUGUI tmp_cursorPrompt;
    public Camera playerCamera; // Assign the camera here
    public float interactDistance = 3f; // Max distance for interaction
    public LayerMask interactableLayer; // Layer for interactables

    private void Start()
    {
        // Hide default OS cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Calculate dynamic interact distance based on camera tilt
        float xRotation = playerCamera.transform.eulerAngles.x;

        // Convert 0-360 to -180 to 180 for easier calculations
        if (xRotation > 180f) xRotation -= 360f;

        // Map -90 (looking straight up) to 90 (looking straight down) into a multiplier range 0.5 to 2
        // Looking straight ahead (0) keeps distance normal
        float distanceMultiplier = 1f + Mathf.Clamp(xRotation / 90f, -1f, 1f);
        float currentInteractDistance = interactDistance * distanceMultiplier;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Default cursor
        tmp_cursorPrompt.text = "";
        img_Cursor.sprite = spr_CursorNormal;

        if (Physics.Raycast(ray, out hit, currentInteractDistance, interactableLayer))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                // Change cursor and show prompt
                img_Cursor.sprite = spr_CursorInteract;
                if (!string.IsNullOrEmpty(interactable.interactText))
                    tmp_cursorPrompt.text = interactable.interactText;

                // Handle left click
                if (Input.GetMouseButtonDown(0))
                {
                    interactable.Interact();
                }
            }
        }
    }
}
