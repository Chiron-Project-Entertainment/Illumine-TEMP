using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ButtonRack))]
public class ButtonRackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Create / reset button rack"))
        {
            (target as ButtonRack).ResetButtonRack();
        }
    }
}
