using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Settings")]
    public Transform cameraTransform; // assign your camera or camera parent
    public float intensity = 0.5f;    // how violently it shakes
    public bool isShaking = false;    // toggle shake on/off

    private Vector3 initialLocalPos;

    private void Awake()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        initialLocalPos = cameraTransform.localPosition;
    }

    private void LateUpdate()
    {
        if (isShaking)
        {
            // Shake in X and Y, keep Z stable
            Vector3 randomOffset = Random.insideUnitSphere * intensity;
            randomOffset.z = 0f;
            cameraTransform.localPosition = initialLocalPos + randomOffset;
        }
        else
        {
            // Reset position when not shaking
            cameraTransform.localPosition = initialLocalPos;
        }
    }
}
