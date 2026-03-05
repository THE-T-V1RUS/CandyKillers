using UnityEngine;
using UnityEngine.EventSystems;

public class CropHandle : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public enum HandleType
    {
        Left, Right, Top, Bottom,
        TopLeft, TopRight, BottomLeft, BottomRight,
        Center
    }

    public float baseCropWidth = 1800;
    public float baseCropHeight = 1200;

    private RectTransform handleRect;
    public float handleBaseSize = 20f;
    public float minHandleSize = 10f;

    public HandleType handle;

    private RectTransform cropBox;
    private RectTransform imageRect;
    public RectTransform canvasRect;

    private Vector2 startMousePos;
    private Vector2 startSize;
    private Vector2 startPos;

    public float minSize = 20f;

    public Rect customBoundsWorld;  // The bounds (world space) of the parent image
    private Rect imageLocalBounds;

    private void Awake()
    {
        cropBox = transform.parent.GetComponent<RectTransform>();
        imageRect = cropBox.parent.GetComponent<RectTransform>();

        imageLocalBounds = imageRect.rect; // This is stable local rect of the image
    }

    private void Start()
    {
        handleRect = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, eventData.position, eventData.pressEventCamera, out startMousePos);

        startSize = cropBox.sizeDelta;
        startPos = cropBox.anchoredPosition;

        // Refresh bounds every drag start in case parent image changed
        customBoundsWorld = GetWorldRect(imageRect);
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, eventData.position, eventData.pressEventCamera, out Vector2 currentMousePos);

        Vector2 delta = currentMousePos - startMousePos;

        Vector2 newSize = startSize;
        Vector2 newPos = startPos;

        Vector2 parentSize = imageRect.rect.size;

        if (handle == HandleType.Center)
        {
            // Move crop box by delta, clamped inside parent bounds

            // Calculate new position candidate
            Vector2 candidatePos = startPos + delta;

            // Clamp so crop box stays fully inside parent
            float halfWidth = startSize.x / 2f;
            float halfHeight = startSize.y / 2f;

            float minX = -parentSize.x / 2f + halfWidth;
            float maxX = parentSize.x / 2f - halfWidth;
            float minY = -parentSize.y / 2f + halfHeight;
            float maxY = parentSize.y / 2f - halfHeight;

            newPos.x = Mathf.Clamp(candidatePos.x, minX, maxX);
            newPos.y = Mathf.Clamp(candidatePos.y, minY, maxY);
        }
        else
        {
            // Your existing resize logic for other handles

            // Horizontal resize
            if (IsLeftHandle())
            {
                float maxWidth = (startPos.x + startSize.x / 2f) + (parentSize.x / 2f);
                newSize.x = Mathf.Clamp(startSize.x - delta.x, minSize, maxWidth);
                newPos.x = startPos.x + (startSize.x - newSize.x) / 2f;
            }
            else if (IsRightHandle())
            {
                float maxWidth = (parentSize.x / 2f) - (startPos.x - startSize.x / 2f);
                newSize.x = Mathf.Clamp(startSize.x + delta.x, minSize, maxWidth);
                newPos.x = startPos.x + (newSize.x - startSize.x) / 2f;
            }

            // Vertical resize
            if (IsTopHandle())
            {
                float maxHeight = (parentSize.y / 2f) - (startPos.y - startSize.y / 2f);
                newSize.y = Mathf.Clamp(startSize.y + delta.y, minSize, maxHeight);
                newPos.y = startPos.y + (newSize.y - startSize.y) / 2f;
            }
            else if (IsBottomHandle())
            {
                float maxHeight = (startPos.y + startSize.y / 2f) + (parentSize.y / 2f);
                newSize.y = Mathf.Clamp(startSize.y - delta.y, minSize, maxHeight);
                newPos.y = startPos.y + (startSize.y - newSize.y) / 2f;
            }
        }

        cropBox.sizeDelta = newSize;
        cropBox.anchoredPosition = newPos;
    }

    private Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        return new Rect(corners[0], corners[2] - corners[0]);
    }

    private void LateUpdate()
    {
        if (cropBox == null || handleRect == null)
            return;

        Vector2 cropSize = cropBox.rect.size;

        float scaleX = cropSize.x / baseCropWidth;
        float scaleY = cropSize.y / baseCropHeight;

        scaleX = Mathf.Clamp(scaleX, 0.1f, 10f);
        scaleY = Mathf.Clamp(scaleY, 0.1f, 10f);

        handleRect.localScale = new Vector3(scaleX, scaleY, 1f);

        PositionHandle();
    }

    private void PositionHandle()
    {
        Vector2 pos = Vector2.zero;
        Vector2 size = cropBox.sizeDelta;

        switch (handle)
        {
            case HandleType.Left: pos = new Vector2(-size.x / 2, 0); break;
            case HandleType.Right: pos = new Vector2(size.x / 2, 0); break;
            case HandleType.Top: pos = new Vector2(0, size.y / 2); break;
            case HandleType.Bottom: pos = new Vector2(0, -size.y / 2); break;
            case HandleType.TopLeft: pos = new Vector2(-size.x / 2, size.y / 2); break;
            case HandleType.TopRight: pos = new Vector2(size.x / 2, size.y / 2); break;
            case HandleType.BottomLeft: pos = new Vector2(-size.x / 2, -size.y / 2); break;
            case HandleType.BottomRight: pos = new Vector2(size.x / 2, -size.y / 2); break;
            case HandleType.Center: pos = Vector2.zero; break;  // center handle in middle
        }

        handleRect.anchoredPosition = pos;
    }

    private bool IsLeftHandle()
    {
        return handle == HandleType.Left || handle == HandleType.TopLeft || handle == HandleType.BottomLeft;
    }

    private bool IsRightHandle()
    {
        return handle == HandleType.Right || handle == HandleType.TopRight || handle == HandleType.BottomRight;
    }

    private bool IsTopHandle()
    {
        return handle == HandleType.Top || handle == HandleType.TopLeft || handle == HandleType.TopRight;
    }

    private bool IsBottomHandle()
    {
        return handle == HandleType.Bottom || handle == HandleType.BottomLeft || handle == HandleType.BottomRight;
    }
}
