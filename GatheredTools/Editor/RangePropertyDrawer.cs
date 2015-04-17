using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine.UI;

[CustomPropertyDrawer(typeof(Range))]
public class RangeDrawer : PropertyDrawer
{
    private const float FieldWidth = 30.0f;
    private const float SliderPadding = 4.0f;

    public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(pos, label, property);
        {
            // Label
            pos = EditorGUI.PrefixLabel(pos, label);

            // Child objects shouldn't be indented
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Create rects
            Rect minRect = new Rect(pos.x, pos.y, FieldWidth, pos.height);
            Rect sliderRect = new Rect(minRect.xMax + SliderPadding * 0.5f, pos.y, Mathf.Max(0.0f, pos.width - FieldWidth * 2.0f - SliderPadding), pos.height);
            Rect maxRect = new Rect(sliderRect.xMax, pos.y, FieldWidth, pos.height);

            SerializedProperty spMin = property.FindPropertyRelative("minimum");
            SerializedProperty spMax = property.FindPropertyRelative("maximum");
            SerializedProperty spValue = property.FindPropertyRelative("value");

            // Minimum
            spMin.floatValue = EditorGUI.FloatField(minRect, spMin.floatValue);
            spMin.floatValue = Mathf.Min(spMin.floatValue, spMax.floatValue);

            // Maximum
            spMax.floatValue = EditorGUI.FloatField(maxRect, spMax.floatValue);
            spMax.floatValue = Mathf.Max(spMax.floatValue, spMin.floatValue);

            // Value slider            
            spValue.floatValue = EditorGUI.Slider(sliderRect, spValue.floatValue, spMin.floatValue, spMax.floatValue);

            // Reset indenting
            EditorGUI.indentLevel = indent;
        }
        EditorGUI.EndProperty();
    }
}