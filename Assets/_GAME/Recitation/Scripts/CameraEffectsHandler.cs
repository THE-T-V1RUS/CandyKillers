using UnityEngine;
using System.Collections;

public class CameraEffectsHandler : MonoBehaviour
{
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine = null;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    public void TriggerScreenShake(float duration, float intensity)
    {
        // Stop current shake if one is running
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            transform.localPosition = originalPosition;
        }
        
        // Start new shake
        shakeCoroutine = StartCoroutine(ScreenShake(duration, intensity));
    }

    IEnumerator ScreenShake(float duration, float intensity)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            // Calculate normalized time (0 to 1)
            float t = elapsed / duration;
            
            // Smooth falloff using ease-out curve
            float falloff = 1f - (t * t); // Quadratic ease-out
            float strength = intensity * falloff;
            
            // Random offset
            float offsetX = Random.Range(-1f, 1f) * strength;
            float offsetY = Random.Range(-1f, 1f) * strength;
            
            transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);
            
            yield return null;
        }
        
        // Reset to original position
        transform.localPosition = originalPosition;
        shakeCoroutine = null;
    }
}
