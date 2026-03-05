using System.Security.Cryptography.X509Certificates;
using UnityEngine;

[ExecuteAlways]
public class BookController : MonoBehaviour
{
    [SerializeField] Transform bookLeft, bookRight;
    [SerializeField] Transform page1, page2;

    [Range(0f, 1f)]
    public float sliderValue = 0f; // 0 = closed, 1 = open

    float openAmount = 10f;
    float closedAmount = 90f;

    // Page rotation variables
    [SerializeField] private Quaternion focusedTargetRotation = Quaternion.Euler(0f, 177f, 0f);
    [SerializeField] private Quaternion unfocusedTargetRotation = Quaternion.Euler(0f, 180f, 0f);
    public bool isFocusedOnPage1 = true;
    [SerializeField] float pageRotationSpeed = 5f;

    void Update()
    {
        UpdateBookRotation();
        
        // Only update page rotations in play mode
        if (Application.isPlaying)
        {
            UpdatePageRotations();
        }
    }

    void OnValidate()
    {
        UpdateBookRotation();
    }

    void UpdateBookRotation()
    {
        if (bookLeft == null || bookRight == null) return;

        // Interpolate between closed and open amounts
        float currentAngle = Mathf.Lerp(closedAmount, openAmount, sliderValue);

        // Apply negative rotation to left book, positive to right book
        bookLeft.localRotation = Quaternion.Euler(0f, -currentAngle, 0f);
        bookRight.localRotation = Quaternion.Euler(0f, currentAngle, 0f);
    }

    void UpdatePageRotations()
    {
        if (page1 == null || page2 == null) return;

        // If book is closing (slider below 0.5), move both pages to 180 degrees
        if (sliderValue < 0.15f)
        {
            Quaternion closedRotation = Quaternion.Euler(0f, 180f, 0f);
            page1.localRotation = closedRotation;
            page2.localRotation = closedRotation;
        }
        else
        {
            // Determine target rotations based on focus
            Quaternion page1Target = isFocusedOnPage1 ? focusedTargetRotation : unfocusedTargetRotation;
            Quaternion page2Target = isFocusedOnPage1 ? unfocusedTargetRotation : focusedTargetRotation;

            // Smoothly interpolate to target rotations (interruptible)
            page1.localRotation = Quaternion.Slerp(page1.localRotation, page1Target, Time.deltaTime * pageRotationSpeed);
            page2.localRotation = Quaternion.Slerp(page2.localRotation, page2Target, Time.deltaTime * pageRotationSpeed);
        }
    }

    /// <summary>
    /// Toggles this GameObject and all children between "Default" and "Hands" layers.
    /// </summary>
    /// <param name="useHandsLayer">If true, sets to "Hands" layer. If false, sets to "Default" layer.</param>
    public void ToggleLayer(bool useHandsLayer)
    {
        string targetLayerName = useHandsLayer ? "Hands" : "Default";
        int targetLayer = LayerMask.NameToLayer(targetLayerName);
        
        if (targetLayer == -1)
        {
            Debug.LogError($"Layer '{targetLayerName}' does not exist!");
            return;
        }

        SetLayerRecursively(gameObject, targetLayer);
    }

    /// <summary>
    /// Recursively sets the layer for a GameObject and all its children.
    /// </summary>
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}