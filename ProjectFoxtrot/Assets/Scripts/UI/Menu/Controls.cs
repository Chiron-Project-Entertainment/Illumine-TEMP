using System;
using System.Linq;
using UnityEngine;

/// <summary> The actions a player can make through input. </summary>
public enum UserAction
{
    None,
    Pause, Interact,
    Forward, Backward, Left, Right,
    Jump, Crouch, Sprint,
}
/// <summary> Instead of string axis, why not enum axis? </summary>
public enum InputAxis
{
    Vertical, Horizontal
}
/// <summary> Instead of using integers for buttons, why not enums? </summary>
public enum MouseButton
{
    Left, Right, Middle
}

/// <summary> The script to be used instead of the old Unity Input system. </summary>
public class Controls : MonoBehaviour
{
    /// <summary> The everlasting incarnation of the Controls script. </summary>
    public static Controls instance = null;

    /// <summary>  </summary>
    [Serializable] private struct Keybinds
    {
        [SerializeField] private Keybind[] elements;

        public UserAction this[KeyCombo keyCombo]
        {
            get
            {
                Keybind keybind;
                try
                {
                    keybind = elements.First(e => e.keyCombo == keyCombo || e.keyComboAlt == keyCombo);
                    return keybind.action;
                }
                catch (InvalidOperationException)
                {
                    //Debug.Log("There is no action with the " + key.ToString() + " key assigned!");
                    return UserAction.None;
                }
            }
        }

        public KeyCombo this[UserAction action]
        {
            get
            {
                Keybind keybind;
                try
                {
                    keybind = elements.First(e => e.action == action);
                    return keybind.keyCombo;
                }
                catch (InvalidOperationException)
                {
                    Debug.Log("The action " + action.ToString() + " has no keybind assigned!");
                    return KeyCombo.Null;
                }
            }
        }

        public KeyCombo[] this[UserAction action, bool includeAlt]
        {
            get
            {
                Keybind keybind;
                try
                {
                    keybind = elements.First(e => e.action == action);
                    if(includeAlt)
                        return new KeyCombo[2] { keybind.keyCombo, keybind.keyComboAlt };
                    else
                        return new KeyCombo[2] { keybind.keyCombo, KeyCombo.Null };
                }
                catch (InvalidOperationException)
                {
                    Debug.Log("The action " + action.ToString() + " has no keybind assigned!");
                    return new KeyCombo[2] { KeyCombo.Null, KeyCombo.Null };
                }
            }
        }

        public int Assign(UserAction action, KeyCombo keyCombo)
        {
            for(int i = 0; i < elements.Length; i++)
                if(elements[i].action == action)
                {
                    int val = 0;
                    if(elements[i].changeMain)
                    {
                        elements[i].keyCombo = keyCombo;
                        val = 1;
                    }
                    else
                    {
                        elements[i].keyComboAlt = keyCombo;
                        val = 2;
                    }

                    elements[i].changeMain = !elements[i].changeMain;
                    return val;
                }
            Debug.LogError("The " + action.ToString() + " action has not been implemented!");
            return 0;
        }
    }
    [SerializeField] private Keybinds keybinds;

    private void Awake()
    {
        // Set the Controls static, with only one instance available per scene.
        if (instance == null)
        {
            instance = this; 
        }
        else
        {
            Destroy(this);
            return;
        }
    }

    /// <summary> 
    /// Assign a keybind to a certain action. Returns 0 if the
    /// keybind could not be added or that the key was
    /// already in use; returns 1 if it has been assigned
    /// to the main KeyCombo; returns 2 if it has been assigned
    /// to the KeyCombo alternative.
    /// </summary>
    public static int AssignKeycombo(UserAction action, KeyCombo keyCombo)
    {
        // Check if there is no action with this keybind already assigned.
        if(instance.keybinds[keyCombo] == UserAction.None)
            return instance.keybinds.Assign(action, keyCombo);

        Debug.Log("The " + keyCombo.ToString() + " is already assigned to " + instance.keybinds[keyCombo] + "!");
        return 0;
    }

    /// <summary> Get the two KeyCombo assigned to the action. </summary>
    public KeyCombo[] GetKeyCombos(UserAction action)
    {
        return keybinds[action, true];
    }

    /// <summary> Returns true during the frame the user starts pressing down the key. </summary>
    public static bool GetKeyDown(KeyCode key) { return Input.GetKeyDown(key); }
    /// <summary> Returns true while the user holds down the key. </summary>
    public static bool GetKey(KeyCode key) { return Input.GetKey(key); }
    /// <summary> Returns true during the frame the user releases the key. </summary>
    public static bool GetKeyUp(KeyCode key) { return Input.GetKeyUp(key); }

    /// <summary> Returns true during the frame the user starts pressing down the mouse button. </summary>
    public static bool GetMouseButtonDown(MouseButton button) { return GetMouseButtonDown((int)button); }
    public static bool GetMouseButtonDown(int button) { return Input.GetMouseButtonDown(button); }
    /// <summary> Returns true while the user holds down the mouse button. </summary>
    public static bool GetMouseButton(MouseButton button) { return GetMouseButton((int)button); }
    public static bool GetMouseButton(int button) { return Input.GetMouseButton(button); }
    /// <summary> Returns true during the frame the user releases the mouse button. </summary>
    public static bool GetMouseButtonUp(MouseButton button) { return GetMouseButtonUp((int)button); }
    public static bool GetMouseButtonUp(int button) { return Input.GetMouseButtonUp(button); }

    /// <summary> Returns true during the frame the user starts pressing down the key combination. </summary>
    public static bool GetComboDown(KeyCombo keyCombo) { return GetKeyDown(keyCombo.key) && keyCombo.modifiers.All(mod => GetKey(mod)); }
    /// <summary> Returns true while the user holds down the key combination. </summary>
    public static bool GetCombo(KeyCombo keyCombo) { return GetKey(keyCombo.key) && keyCombo.modifiers.All(mod => GetKey(mod)); }
    /// <summary> Returns true during the frame the user releases the key combination. </summary>
    public static bool GetComboUp(KeyCombo keyCombo) { return GetKeyUp(keyCombo.key) && keyCombo.modifiers.All(mod => !GetKey(mod)); }

    /// <summary> Returns true during the frame the user starts pressing down the keybind/s of the action. </summary>
    public static bool GetActionDown(UserAction action) { return instance.keybinds[action, true].Any(combo => GetComboDown(combo)); }
    /// <summary> Returns true while the user holds down the keybind/s of the action. </summary>
    public static bool GetAction(UserAction action) { return instance.keybinds[action, true].Any(combo => GetCombo(combo)); }
    /// <summary> Returns true during the frame the user releases the keybind/s of the action. </summary>
    public static bool GetActionUp(UserAction action) { return instance.keybinds[action, true].Any(combo => GetComboUp(combo)); }

    /// <summary> Returns the value of the virtual axis with no smoothing filtering applied. </summary>
    public static float GetAxisRaw(InputAxis axis)
    {
        if (axis == InputAxis.Horizontal) return GetAction(UserAction.Left) ? -1f : GetAction(UserAction.Right) ? 1f : 0f;
        else if (axis == InputAxis.Vertical) return GetAction(UserAction.Backward) ? -1f : GetAction(UserAction.Forward) ? 1f : 0f;
        else return 0f;
    }

    /// <summary> Returns true if there is any key currently pressed. </summary>
    public static bool AnyKeyPressed { get { return KeyCodes.All.Any(key => GetKey(key)); } }
    /// <summary> Returns true if there is any keyboard key currently pressed. </summary>
    public static bool AnyKeyboardKeyPressed { get { return KeyCodes.All.Any(key => !KeyCodes.mouseButtons.Contains(key) && GetKey(key)); } }
    /// <summary> Returns true if there is any mouse button currently pressed. </summary>
    public static bool AnyMouseButtonPressed { get { return KeyCodes.mouseButtons.Any(button => GetKey(button)); } }
}
