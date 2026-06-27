using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Reflection;

namespace BerserkPixel.Tilemap_Generator.Utilities
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class InspectorButtonAttribute : PropertyAttribute
    {
        public readonly string MethodName;
        public readonly float verticalPadding = 4f;

        public InspectorButtonAttribute(string MethodName)
        {
            this.MethodName = MethodName;
            verticalPadding = 4f;
        }

        public InspectorButtonAttribute(string MethodName, float VerticalPadding)
        {
            this.MethodName = MethodName;
            verticalPadding = VerticalPadding;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InspectorButtonAttribute))]
    public class InspectorButtonPropertyDrawer : PropertyDrawer
    {
        private MethodInfo _eventMethodInfo;

        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            InspectorButtonAttribute inspectorButtonAttribute = (InspectorButtonAttribute)attribute;

            Rect buttonRect = new Rect(
                position.x,
                position.y,
                position.width,
                position.height + inspectorButtonAttribute.verticalPadding
            );
            GUI.skin.button.alignment = TextAnchor.MiddleLeft;
            
            if (GUI.Button(buttonRect, inspectorButtonAttribute.MethodName))
            {
                System.Type eventOwnerType = prop.serializedObject.targetObject.GetType();
                string eventName = inspectorButtonAttribute.MethodName;

                // we try and get the name of the method of the class using reflection
                if (_eventMethodInfo == null)
                {
                    _eventMethodInfo = eventOwnerType.GetMethod(eventName,
                        BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                }

                // if we got it, we trigger it
                if (_eventMethodInfo != null)
                {
                    _eventMethodInfo.Invoke(prop.serializedObject.targetObject, null);
                }
                else
                {
                    Debug.LogWarning($"InspectorButton: Unable to find method {eventName} in {eventOwnerType}");
                }
            }
        }
    }
#endif
}