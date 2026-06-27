using System;
using UnityEngine;

namespace BerserkPixel.Tilemap_Generator.Utilities
{
    /// <summary>
    /// Use this property on a ScriptableObject type to allow the editors drawing the field to draw an expandable
    /// area that allows for changing the values on the object without having to change editor.
    /// </summary>
    public class ExpandableAttribute : PropertyAttribute
    {
        public BackgroundStyles BackgroundStyle;
        
        public ExpandableAttribute(BackgroundStyles backgroundStyle)
        {
            BackgroundStyle = backgroundStyle;
        }
        
        public ExpandableAttribute()
        {
            BackgroundStyle = BackgroundStyles.HelpBox;
        }
    }
    
    [Serializable]
    public enum BackgroundStyles
    {
        None,
        HelpBox,
        Darken,
        Lighten
    }
}