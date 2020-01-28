using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary> 
/// A color palatte from which the children on this gameObject
/// which contain the ColorType component will choose from.
/// </summary>
public class ColorPalette : MonoBehaviour
{
    /// <summary> ColorPair array holder, with multiple methods attached to it. </summary>
    [Serializable] private struct ColorPairs
    {
        /// <summary> Holder of a ColorCategory/Color pair. </summary>
        [Serializable] public struct ColorPair
        {
            public ColorCategory category;
            public Color color;
        }
        /// <summary> Array made out of all the existing color pairs. </summary>
        [SerializeField] private ColorPair[] elements;
        /// <summary> Returns the Color attached to the given category. </summary>
        public Color this[ColorCategory category]
        {
            get
            {
                return elements.First(e => e.category == category).color;
            }
        }
    }
    [SerializeField] private ColorPairs colorPairs;
    [SerializeField] private TMP_FontAsset fontMain = null;
    [SerializeField] private TMP_FontAsset fontAlternative = null;
    [SerializeField] private TMP_FontAsset fontDropdown = null;

    private void Awake()
    {
        ApplyChanges();
    }

    /// <summary> 
    /// Applies all the changes done from the inspector to the
    /// UI elements having the ColorType script attached. 
    /// </summary>
    public void ApplyChanges()
    {
        foreach(ColorType childColorType in GetComponentsInChildren<ColorType>())
        {
            ColorCategory category = childColorType.category;
            switch(category)
            {
                // Changes to be done only to the text gameObjects.
                case ColorCategory.TextMain: case ColorCategory.TextAlternative:
                    TextMeshProUGUI textMesh = childColorType.GetComponent<TextMeshProUGUI>();
                    if(textMesh != null)
                    {
                        textMesh.font = category == ColorCategory.TextMain ? fontMain : category == ColorCategory.TextAlternative ? fontAlternative : fontDropdown;
                        textMesh.color = colorPairs[category];
                    }
                    break;
                
                // Changes to be done to gameObjects with an Image component attached.
                case ColorCategory.PrimayLight: case ColorCategory.Primary:
                case ColorCategory.PrimaryDark: case ColorCategory.SecondaryLight:
                case ColorCategory.Secondary: case ColorCategory.SecondaryDark:
                    Image childImage = childColorType.GetComponent<Image>();
                    if (childImage != null)
                    {
                        childImage.color = colorPairs[category];
                    }
                    break;
            }
        }
    }
}
