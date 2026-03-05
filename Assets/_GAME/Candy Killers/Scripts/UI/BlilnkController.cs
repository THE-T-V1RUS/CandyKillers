using UnityEngine;

[ExecuteAlways] // Ensures it runs in editor mode
public class BlinkController : MonoBehaviour
{
    [SerializeField] RectTransform topBlink, bottomBlink;
    [Range(0.0f, 1.0f)] public float blinkAmount;

    [SerializeField] float minBlink = 0f;
    [SerializeField] float maxBlink = 1200f;

    private void OnValidate()
    {
        ApplyBlink();
    }

    private void Update()
    {
        // Only update during runtime
        if (Application.isPlaying)
            ApplyBlink();
    }

    private void ApplyBlink()
    {
        float height = Mathf.Lerp(minBlink, maxBlink, blinkAmount);

        if (topBlink != null)
            topBlink.sizeDelta = new Vector2(topBlink.sizeDelta.x, height);

        if (bottomBlink != null)
            bottomBlink.sizeDelta = new Vector2(bottomBlink.sizeDelta.x, height);
    }
}
