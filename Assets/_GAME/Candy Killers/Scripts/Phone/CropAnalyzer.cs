#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class CropAnalyzer : MonoBehaviour
{
    public RectTransform cropBox;
    [SerializeField] private RectTransform keepImage;
    [SerializeField] private RectTransform deleteImage;
    [SerializeField] Camera cam;

    [Range(0, 1f)]
    public float keepPercent;
    [Range(0, 1f)]
    public float deletePercent;

    // This makes the function appear as a button in the inspector
    [ContextMenu("Calculate Percentages")]
    public void CalculatePercentages()
    {
        if (cropBox == null || keepImage == null || deleteImage == null)
        {
            Debug.LogWarning("Please assign all RectTransforms in the inspector.");
            return;
        }

        Rect cropRect = GetScreenRect(cropBox, cam);
        Rect keepRect = GetScreenRect(keepImage, cam);
        Rect deleteRect = GetScreenRect(deleteImage, cam);

        float keepImageArea = keepRect.width * keepRect.height;
        float deleteImageArea = deleteRect.width * deleteRect.height;

        float keepArea = GetOverlapArea(cropRect, keepRect);
        float deleteArea = GetOverlapArea(cropRect, deleteRect);

        keepPercent = keepImageArea > 0 ? Mathf.Clamp01(keepArea / keepImageArea) : 0f;
        deletePercent = deleteImageArea > 0 ? Mathf.Clamp01(deleteArea / deleteImageArea) : 0f;

        Debug.Log($"Keep: {keepPercent:P2}, Delete: {deletePercent:P2}");
    }

    private Rect GetScreenRect(RectTransform rt, Camera cam)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        Vector2 screenMin = cam.WorldToScreenPoint(corners[0]);
        Vector2 screenMax = screenMin;

        for (int i = 1; i < 4; i++)
        {
            Vector2 screenPoint = cam.WorldToScreenPoint(corners[i]);
            screenMin = Vector2.Min(screenMin, screenPoint);
            screenMax = Vector2.Max(screenMax, screenPoint);
        }

        return Rect.MinMaxRect(screenMin.x, screenMin.y, screenMax.x, screenMax.y);
    }

    private float GetOverlapArea(Rect a, Rect b)
    {
        if (!a.Overlaps(b))
            return 0f;

        float xMin = Mathf.Max(a.xMin, b.xMin);
        float xMax = Mathf.Min(a.xMax, b.xMax);
        float yMin = Mathf.Max(a.yMin, b.yMin);
        float yMax = Mathf.Min(a.yMax, b.yMax);

        float width = Mathf.Max(0, xMax - xMin);
        float height = Mathf.Max(0, yMax - yMin);

        return width * height;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CropAnalyzer))]
public class CropAnalyzerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CropAnalyzer analyzer = (CropAnalyzer)target;
        if (GUILayout.Button("Calculate Percentages"))
        {
            analyzer.CalculatePercentages();
        }
    }
}
#endif