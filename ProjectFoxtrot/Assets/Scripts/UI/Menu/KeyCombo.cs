using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary> A KeyCode along with its modifier keys (if any). </summary>
[Serializable]
public struct KeyCombo
{
    public KeyCode key;
    public KeyCode[] modifiers;

    /// <param name="key"> The key to be pressed. </param>
    /// <param name="modifiers"> The modifier keys to be pressed. </param>
    public KeyCombo(KeyCode key, params KeyCode[] modifiers)
    {
        // Assigning the main key.
        this.key = key;

        // Making sure that the keys set as modifiers are indeed modifier keys and are distinct.
        this.modifiers = modifiers
            .Where(modif => KeyCodes.modifierKeys.Contains(modif) && modif != key)
            .Distinct()
            .OrderBy(modif => modif.ToString())
            .Reverse()
            .ToArray();
    }

    public override string ToString()
    {
        string ans = KeyCodes.ToShortString(key);
        foreach (var mod in modifiers)
            ans = KeyCodes.ToShortString(mod) + "+" + ans;
        return ans;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is KeyCombo))
            return false;

        var combo = (KeyCombo)obj;
        return this == combo;
    }
    public override int GetHashCode()
    {
        var hashCode = 1342178661;
        hashCode = hashCode * -1521134295 + key.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<KeyCode[]>.Default.GetHashCode(modifiers);
        return hashCode;
    }

    public static  bool operator==(KeyCombo one, KeyCombo other)
    {
        if (one.key == other.key && one.modifiers.Length == other.modifiers.Length)
        {
            foreach (var e in one.modifiers)
                if (other.modifiers.Contains(e) == false)
                    return false;
            return true;
        }
        return false;
    }
    public static  bool operator!=(KeyCombo one, KeyCombo other)
    {
        return !(one == other);
    }

    public static KeyCombo Null = new KeyCombo(KeyCode.None);
}
