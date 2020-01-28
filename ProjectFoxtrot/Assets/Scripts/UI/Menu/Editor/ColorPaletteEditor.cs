using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ColorPalette))]
public class ColorPaletteEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Apply Changes"))
        {
            (target as ColorPalette).ApplyChanges();
        }
    }
}
