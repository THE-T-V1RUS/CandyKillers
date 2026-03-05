using UnityEngine;

public class Credits : MonoBehaviour
{
    public RectTransform contentTransform;

    private void OnEnable()
    {
        contentTransform.anchoredPosition = Vector2.zero;
    }
}
