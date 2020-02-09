///////////////////////////////////////////////////////////////
///                                                         ///
///             Script coded by Hakohn (Robert).            ///
///                                                         ///
///////////////////////////////////////////////////////////////

using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary> Every main KeyCode, organized by key types. </summary>
public static class KeyCodes
{
    /// <summary> Array containing all the mouse button key codes. </summary>
    public static readonly KeyCode[] mouseButtons = new KeyCode[] {
        KeyCode.Mouse0, KeyCode.Mouse1, KeyCode.Mouse2, KeyCode.Mouse3,
        KeyCode.Mouse4, KeyCode.Mouse5, KeyCode.Mouse6
    };

    /// <summary> Array containing all the letter key codes. </summary>
    public static readonly KeyCode[] letterKeys = new KeyCode[] {
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G,
        KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N,
        KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T, KeyCode.U,
        KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y, KeyCode.Z, KeyCode.Space
    };

    /// <summary> Array containing all the number key codes. </summary>
    public static readonly KeyCode[] numberKeys = new KeyCode[] {
        KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
        KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9,
        KeyCode.Keypad0, KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4,
        KeyCode.Keypad5, KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9
    };

    /// <summary> Array containing all the arrow key  codes. </summary>
    public static readonly KeyCode[] arrowKeys = new KeyCode[] {
        KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow
    };

    /// <summary> Array containing all the modifier key codes. </summary>
    public static readonly KeyCode[] modifierKeys = new KeyCode[] {
        KeyCode.LeftAlt, KeyCode.LeftControl, KeyCode.LeftShift,
        KeyCode.RightAlt, KeyCode.RightControl, KeyCode.RightShift
    };

    /// <summary> Array containing all the function key codes. </summary>
    public static readonly KeyCode[] functionKeys = new KeyCode[] {
        KeyCode.F1, KeyCode.F2, KeyCode.F3, KeyCode.F4, KeyCode.F5,
        KeyCode.F6, KeyCode.F7, KeyCode.F8, KeyCode.F9, KeyCode.F10,
        KeyCode.F11, KeyCode.F12, KeyCode.F13, KeyCode.F14, KeyCode.F15
    };

    /// <summary> Array containing all the symbol key codes. </summary>
    public static readonly KeyCode[] symbolKeys = new KeyCode[] {
        KeyCode.Tab, KeyCode.BackQuote, KeyCode.Underscore, KeyCode.Equals,
        KeyCode.Exclaim, KeyCode.Question, KeyCode.Minus, KeyCode.Comma,
        KeyCode.Colon, KeyCode.Semicolon, KeyCode.LeftParen, KeyCode.RightParen,
        KeyCode.Plus, KeyCode.Period
    };

    /// <summary> Array containing all the existing key codes. </summary>
    public static KeyCode[] All
    {
        get
        {
            return (KeyCode[])Enum.GetValues(typeof(KeyCode));
        }
    }


    public static string ToShortString(KeyCode key)
    {
        string ans = key.ToString();
        Dictionary<string, string> abbreviations = new Dictionary<string, string>() {
            { "UpArrow", "Up" }, { "LeftArrow", "<" }, { "DownArrow", "Down" }, { "RightArrow", ">" },
            { "Left", "L" }, { "Right", "R" },
            { "Control", "Ctrl" },
            { "Alpha", "" }, { "Keypad", "Num" },
            { "Mouse", "M" }
        };
        
        foreach(var dictKey in abbreviations.Keys)
        {
            if(ans.Contains(dictKey))
            {
                ans = ans.Replace(dictKey, abbreviations[dictKey]);
            }
        }
        return ans;
    }
}
