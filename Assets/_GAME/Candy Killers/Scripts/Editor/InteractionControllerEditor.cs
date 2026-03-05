#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InteractionController))]
public class InteractionControllerEditor : Editor
{
    private InteractionController controller;
    private SerializedProperty interactionBoxSize;
    private SerializedProperty interactionBoxOffset;
    private SerializedProperty showDebugGizmos;

    private void OnEnable()
    {
        controller = (InteractionController)target;
        interactionBoxSize = serializedObject.FindProperty("interactionBoxSize");
        interactionBoxOffset = serializedObject.FindProperty("interactionBoxOffset");
        showDebugGizmos = serializedObject.FindProperty("showDebugGizmos");
    }

    private void OnSceneGUI()
    {
        if (!showDebugGizmos.boolValue)
            return;

        serializedObject.Update();

        Transform transform = controller.transform;
        Vector3 boxCenter = transform.position + transform.TransformDirection(interactionBoxOffset.vector3Value);
        
        // Draw the box
        Handles.color = new Color(0f, 1f, 0f, 0.2f);
        Handles.matrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);
        
        // Draw wire cube
        Handles.color = Color.green;
        DrawWireCube(Vector3.zero, interactionBoxSize.vector3Value);
        
        Handles.matrix = Matrix4x4.identity;
        
        // Center position handle
        Handles.color = Color.cyan;
        EditorGUI.BeginChangeCheck();
        Vector3 newCenter = Handles.PositionHandle(boxCenter, transform.rotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(controller, "Changed Interaction Box Offset");
            interactionBoxOffset.vector3Value = transform.InverseTransformDirection(newCenter - transform.position);
            serializedObject.ApplyModifiedProperties();
        }
        
        // Face handles for resizing
        Vector3[] directions = new Vector3[]
        {
            transform.right,      // Right
            -transform.right,     // Left
            transform.up,         // Up
            -transform.up,        // Down
            transform.forward,    // Forward
            -transform.forward    // Back
        };
        
        int[] sizeIndices = new int[] { 0, 0, 1, 1, 2, 2 };
        
        for (int i = 0; i < directions.Length; i++)
        {
            Vector3 currentSize = interactionBoxSize.vector3Value;
            Vector3 handlePos = boxCenter + directions[i] * currentSize[sizeIndices[i]] * 0.5f;
            float handleSize = HandleUtility.GetHandleSize(handlePos) * 0.1f;
            
            Handles.color = Color.yellow;
            
            EditorGUI.BeginChangeCheck();
            Vector3 newHandlePos = Handles.Slider(handlePos, directions[i], handleSize, Handles.SphereHandleCap, 0.01f);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(controller, "Changed Interaction Box Size");
                
                float delta = Vector3.Dot(newHandlePos - handlePos, directions[i]);
                Vector3 newSize = currentSize;
                newSize[sizeIndices[i]] += delta * 2f;
                newSize[sizeIndices[i]] = Mathf.Max(0.1f, newSize[sizeIndices[i]]);
                
                interactionBoxSize.vector3Value = newSize;
                interactionBoxOffset.vector3Value += transform.InverseTransformDirection(directions[i] * delta);
                
                serializedObject.ApplyModifiedProperties();
            }
        }
    }

    private void DrawWireCube(Vector3 center, Vector3 size)
    {
        Vector3 half = size * 0.5f;
        
        // Bottom face
        Handles.DrawLine(center + new Vector3(-half.x, -half.y, -half.z), center + new Vector3(half.x, -half.y, -half.z));
        Handles.DrawLine(center + new Vector3(half.x, -half.y, -half.z), center + new Vector3(half.x, -half.y, half.z));
        Handles.DrawLine(center + new Vector3(half.x, -half.y, half.z), center + new Vector3(-half.x, -half.y, half.z));
        Handles.DrawLine(center + new Vector3(-half.x, -half.y, half.z), center + new Vector3(-half.x, -half.y, -half.z));
        
        // Top face
        Handles.DrawLine(center + new Vector3(-half.x, half.y, -half.z), center + new Vector3(half.x, half.y, -half.z));
        Handles.DrawLine(center + new Vector3(half.x, half.y, -half.z), center + new Vector3(half.x, half.y, half.z));
        Handles.DrawLine(center + new Vector3(half.x, half.y, half.z), center + new Vector3(-half.x, half.y, half.z));
        Handles.DrawLine(center + new Vector3(-half.x, half.y, half.z), center + new Vector3(-half.x, half.y, -half.z));
        
        // Vertical edges
        Handles.DrawLine(center + new Vector3(-half.x, -half.y, -half.z), center + new Vector3(-half.x, half.y, -half.z));
        Handles.DrawLine(center + new Vector3(half.x, -half.y, -half.z), center + new Vector3(half.x, half.y, -half.z));
        Handles.DrawLine(center + new Vector3(half.x, -half.y, half.z), center + new Vector3(half.x, half.y, half.z));
        Handles.DrawLine(center + new Vector3(-half.x, -half.y, half.z), center + new Vector3(-half.x, half.y, half.z));
    }
}
#endif
