using System;
using System.Linq;
using UnityEngine;

/// <summary> A UserAction along with its attached KeyCombo and its alternative. </summary>
[Serializable]
public class Keybind
{
    public UserAction action;
    public KeyCombo keyCombo;
    public KeyCombo keyComboAlt = KeyCombo.Null;

    [HideInInspector] public bool changeMain = false;


    public Keybind(UserAction action, KeyCombo keyCombo)
    {
        this.action = action;
        this.keyCombo = keyCombo;
    }
    public Keybind(UserAction action, KeyCombo keyCombo, KeyCombo keyComboAlt) : this(action, keyCombo)
    {
        this.keyComboAlt = keyComboAlt;
    }

    /// <summary> 
    /// Refreshes the two KeyCombo, making sure that if there is a KeyCombo to 
    /// be empty, that's to be the alternative. Returns the new main keyCombo. 
    /// </summary>
    public KeyCombo Refresh()
    {
        if (keyCombo == KeyCombo.Null && keyComboAlt != KeyCombo.Null)
        {
            keyCombo = keyComboAlt;
            keyComboAlt = KeyCombo.Null;
        }
        return keyCombo;
    }

    public override string ToString()
    {
        return action.ToString() + ": " + keyCombo.ToString() + (keyComboAlt == KeyCombo.Null ? "" : " and " + keyComboAlt.ToString());
    }
}
