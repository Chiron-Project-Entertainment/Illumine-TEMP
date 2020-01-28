using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Keybinder : Button
{
    /// <summary> The text mesh that will be updated. </summary>
    public TextMeshProUGUI actionLabel = null;
    public TextMeshProUGUI mainLabel = null;
    public TextMeshProUGUI altLabel = null;

    public UserAction userAction;

    public bool acceptsMouseButtons = false;
    public bool acceptsLetterKeys = true;
    public bool acceptsNumberKeys = true;
    public bool acceptsArrowKeys = true;
    public bool acceptsFunctionKeys = true;
    public bool acceptsSymbolKeys = true;
    public bool acceptsModifierKeys = true;
    public bool acceptsModifierKeysOnly = true;

    private bool waitingForInput = false;
    private KeyCode lastKeyPressed = KeyCode.None;
    private HashSet<KeyCode> modifiersPressed = new HashSet<KeyCode>();

    protected override void Awake()
    {
        base.Awake();
        onClick.AddListener(RequestKeybind);
    }

    protected override void Start()
    {
        base.Start();
        UpdateLabels();
    }

    private void Update()
    {
        if(waitingForInput)
        {
            if(Controls.AnyKeyPressed)
            {
                foreach(KeyCode key in KeyCodes.All)
                {
                    if(Controls.GetKey(key))
                    {
                        // Check if it's eligible as a main key.
                        if(IsEligibleMainKey(key))
                        {
                            lastKeyPressed = key;
                        }
                        // Check if it's eligible as a modifier key.
                        else if(acceptsModifierKeys && KeyCodes.modifierKeys.Contains(key))
                        {
                            if(KeyCodes.modifierKeys.Contains(lastKeyPressed) == false)
                            {
                                modifiersPressed.Add(key);
                            }
                            if(acceptsModifierKeysOnly && lastKeyPressed == KeyCode.None)
                            {
                                lastKeyPressed = key;
                            }
                        }
                    }
                }
            }
            // Once all the keys have been released, check if there has been any
            // correct key pressed out of these.
            else if(lastKeyPressed != KeyCode.None)
            {
                KeyCombo keyCombo = new KeyCombo(lastKeyPressed, modifiersPressed.ToArray());

                // Update the label text if the keybind was indeed changed.
                Controls.AssignKeycombo(userAction, keyCombo);

                UpdateLabels();
                ResetValues();
            }
        }
    }

    /// <summary> Check if it's eligible as a main key.  </summary>
    private bool IsEligibleMainKey(KeyCode key)
    {
        if (
            (acceptsLetterKeys && KeyCodes.letterKeys.Contains(key)) ||
            (acceptsNumberKeys && KeyCodes.numberKeys.Contains(key)) ||
            (acceptsArrowKeys && KeyCodes.arrowKeys.Contains(key)) ||
            (acceptsFunctionKeys && KeyCodes.functionKeys.Contains(key)) ||
            (acceptsSymbolKeys && KeyCodes.symbolKeys.Contains(key)) ||
            (acceptsMouseButtons && KeyCodes.mouseButtons.Contains(key)))
            return true;
        return false;
    }

    private void UpdateLabels()
    {
        actionLabel.text = userAction.ToString();
        KeyCombo mainCombo = Controls.instance.GetKeyCombos(userAction)[0];
        mainLabel.text = mainCombo.key != KeyCode.None ? mainCombo.ToString() : " ";
        KeyCombo altCombo = Controls.instance.GetKeyCombos(userAction)[1];
        altLabel.text = altCombo.key != KeyCode.None ? altCombo.ToString() : " ";
    }

    private void ResetValues()
    {
        waitingForInput = false;
        lastKeyPressed = KeyCode.None;
        modifiersPressed = new HashSet<KeyCode>();
    }

    public void RequestKeybind()
    {
        waitingForInput = true;
        mainLabel.text = "?";
        altLabel.text = "?";
    }
}
