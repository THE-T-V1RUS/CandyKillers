using UnityEngine;

public class ClampYRotation : MonoBehaviour
{
    [Header("Clamp Settings")]
    public float minY = -80f; // left limit
    public float maxY = 80f;  // right limit

    private float startY;

    void Start()
    {
        // Store starting local Y rotation
        startY = transform.localEulerAngles.y;
    }

    void LateUpdate()
    {
        Vector3 localEuler = transform.localEulerAngles;

        // Convert to -180..180 range so clamping works properly
        float currentY = localEuler.y;
        if (currentY > 180f) currentY -= 360f;

        // Clamp around the starting Y rotation
        float clampedY = Mathf.Clamp(currentY, startY + minY, startY + maxY);

        // Apply clamped value
        localEuler.y = clampedY;
        transform.localEulerAngles = localEuler;
    }
}
