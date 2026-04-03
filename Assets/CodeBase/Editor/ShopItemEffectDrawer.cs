using CodeBase.StaticData;
using UnityEditor;
using UnityEngine;

namespace CodeBase.Editor
{
    [CustomPropertyDrawer(typeof(ShopItemEffect))]
    public class ShopItemEffectDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(pos, label, prop);

            float h = EditorGUIUtility.singleLineHeight;
            float pad = 2f;
            var r = new Rect(pos.x, pos.y, pos.width, h);

            var categoryProp = prop.FindPropertyRelative("category");
            var valueProp    = prop.FindPropertyRelative("value");

            EditorGUI.PropertyField(r, categoryProp); r.y += h + pad;
            EditorGUI.PropertyField(r, valueProp);    r.y += h + pad;
            
            var category = (ShopItemCategory)categoryProp.enumValueIndex;
            switch (category)
            {
                case ShopItemCategory.Buff:
                    EditorGUI.PropertyField(r, prop.FindPropertyRelative("buffType"));
                    break;
                case ShopItemCategory.Upgrade:
                    EditorGUI.PropertyField(r, prop.FindPropertyRelative("upgradeType"));
                    break;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            float h = EditorGUIUtility.singleLineHeight;
            return h * 3 + 6f;
        }
    }
}