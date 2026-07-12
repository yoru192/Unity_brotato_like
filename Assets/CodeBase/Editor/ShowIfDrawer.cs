using CodeBase.StaticData;
using UnityEditor;
using UnityEngine;

namespace CodeBase.Editor
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var showIf = (ShowIfAttribute)attribute;
            var conditionProperty = property.serializedObject.FindProperty(showIf.ConditionField);

            bool show = conditionProperty.enumValueIndex == (int)showIf.CompareValue;
            if (show)
                EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var showIf = (ShowIfAttribute)attribute;
            var conditionProperty = property.serializedObject.FindProperty(showIf.ConditionField);
            bool show = conditionProperty.enumValueIndex == (int)showIf.CompareValue;
            return show ? EditorGUI.GetPropertyHeight(property, label) : -EditorGUIUtility.standardVerticalSpacing;
        }
    }
}