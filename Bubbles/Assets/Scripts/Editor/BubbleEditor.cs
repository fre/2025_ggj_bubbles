#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Bubble))]
[CanEditMultipleObjects]
public class BubbleEditor : Editor
{
    private SerializedProperty _sizeProperty;
    private SerializedProperty _variantProperty;
    private SerializedProperty _coreSizeRatioProperty;
    private Tool _lastTool;

    private void OnEnable()
    {
        _sizeProperty = serializedObject.FindProperty("Size");
        _variantProperty = serializedObject.FindProperty("Variant");
        _coreSizeRatioProperty = serializedObject.FindProperty("CoreSizeRatio");
        _lastTool = Tools.current;
    }

    private void OnDisable()
    {
        Tools.current = _lastTool;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        Bubble bubble = (Bubble)target;
        Transform transform = bubble.transform;

        // Position handle
        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = Handles.PositionHandle(transform.position, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(transform, "Move Bubble");
            transform.position = newPosition;
        }

        // Scale handle
        EditorGUI.BeginChangeCheck();
        float handleSize = HandleUtility.GetHandleSize(transform.position) * 0.5f;
        Vector3 right = transform.position + Vector3.right * bubble.Size * 0.5f;
        float newSize = Handles.ScaleSlider(bubble.Size, transform.position, Vector3.right, Quaternion.identity, handleSize, 0.1f);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(bubble, "Scale Bubble");
            bubble.Size = Mathf.Max(0.1f, newSize);
            bubble.UpdateShape();
            EditorUtility.SetDirty(bubble);
        }

        // Draw variant selector
        Handles.BeginGUI();
        float radius = bubble.Size * 0.5f;
        Vector2 screenPos = HandleUtility.WorldToGUIPoint(transform.position + Vector3.up * (radius + 0.5f));
        float totalWidth = 100f;
        Rect rect = new Rect(screenPos.x - totalWidth * 0.5f, screenPos.y, totalWidth, 20);

        GUILayout.BeginArea(rect);
        EditorGUILayout.BeginHorizontal();
        
        // Previous variant button
        if (GUILayout.Button("<", GUILayout.Width(20)))
        {
            int newVariant = bubble.Variant <= 0 ? GameRules.Data.VariantCount - 1 : bubble.Variant - 1;
            ChangeVariant(bubble, newVariant);
        }

        // Current variant display
        GUI.backgroundColor = Color.HSVToRGB(bubble.Hue, 0.7f, 0.6f);
        GUILayout.Box(bubble.Variant.ToString(), GUILayout.ExpandWidth(true));
        GUI.backgroundColor = Color.white;

        // Next variant button
        if (GUILayout.Button(">", GUILayout.Width(20)))
        {
            int newVariant = bubble.Variant >= GameRules.Data.VariantCount - 1 ? 0 : bubble.Variant + 1;
            ChangeVariant(bubble, newVariant);
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
        Handles.EndGUI();

        // Draw variant color preview
        Color previewColor = Color.HSVToRGB(bubble.Hue, 0.7f, 0.6f);
        Handles.color = previewColor;
        Handles.DrawWireDisc(transform.position, Vector3.forward, radius);
        Handles.color = new Color(previewColor.r, previewColor.g, previewColor.b, 0.2f);
        Handles.DrawSolidDisc(transform.position, Vector3.forward, radius);
    }

    private void ChangeVariant(Bubble bubble, int newVariant)
    {
        Undo.RecordObject(bubble, "Change Bubble Variant");
        bubble.Variant = newVariant;
        bubble.CoreSizeRatio = GameRules.BubbleVariantData(newVariant).CoreSizeRatio;
        bubble.UpdateShape();
        EditorUtility.SetDirty(bubble);
    }
}
#endif