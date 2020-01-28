using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A script with no use yet once the game has started. Its only use is
/// for ease when dealing with the menus while in the editor.
/// </summary>
public class ButtonRack : MonoBehaviour
{
    private Vector2 startingPosition = new Vector2(-1865, -1495);

    [Header("The offset of the whole button rack, should you want it placed elsewhere")]
    [SerializeField] private Vector2 offsetOfButtonRack = new Vector2(0, 0);

    [Header("Distances (in pixels)")]
    [SerializeField] private Vector2 distanceBetweenButtons = new Vector2(155, 155);
    [SerializeField] private Vector2 distanceTillMargins = new Vector2(1020 + 155, 1020 + 155);
    [SerializeField] private float xOffsetOfButtons = 100;

    [Header("Bar prefabs")]
    [SerializeField] private GameObject topBottomBarPrefab = null;
    [SerializeField] private GameObject interButtonBarPrefab = null;

    [Header("Bar transforms, if already existing in scene, ordered from top to bottom")]
    [SerializeField] private Transform topBar = null;
    [SerializeField] private Transform bottomBar = null;
    [SerializeField] private List<Transform> interButtonBars = null;

    [Header("Transform of the menu the buttons belong to.")]
    [SerializeField] private Transform menuTransform = null;
    [SerializeField] private Transform nonInteractibleParent = null;

    [Header("Button transforms already in scene to be placed here, ordered from top to bottom")]
    [SerializeField] private Transform[] buttonArray = null;


    private Vector2 SpawnOrMoveTo(GameObject prefab, Vector2 position, ref List<Transform> inWhichToSave, int index)
    {
        if (index >= inWhichToSave.Count)
        {
            inWhichToSave.Add((Instantiate(prefab) as GameObject).transform);
            return SpawnOrMoveTo(prefab, position, ref inWhichToSave, index);
        }
        else if (inWhichToSave[index] == null)
        {
            inWhichToSave.RemoveAt(index);
            return SpawnOrMoveTo(prefab, position, ref inWhichToSave, index);
        }
        return MoveTo(inWhichToSave[index], position);
    }
    private Vector2 SpawnOrMoveTo(GameObject prefab, Vector2 position, ref Transform whereToSave)
    {
        if (whereToSave == null)
        {
            whereToSave = (Instantiate(prefab) as GameObject).transform;
        }
        return MoveTo(whereToSave, position);
    }
    private Vector2 MoveTo(Transform objectTransform, Vector2 position, Transform parent = null)
    {
        if (parent == null) parent = transform;
        objectTransform.SetParent(parent, false);
        objectTransform.SetAsFirstSibling();
        objectTransform.GetComponent<RectTransform>().anchoredPosition = position;
        return position;
    }

    public void ResetButtonRack()
    {
        // First, we create / reposition the bottom bar.
        Vector2 lastPosition = SpawnOrMoveTo(topBottomBarPrefab, startingPosition + offsetOfButtonRack, ref bottomBar) + distanceTillMargins - distanceBetweenButtons;
        for (int i = buttonArray.Length - 1; i >= 0; i--)
        {
            // Now, we create each button, followed by another inter-button bar (besides the last button).
            lastPosition = MoveTo(buttonArray[i], lastPosition + distanceBetweenButtons / 2 + xOffsetOfButtons * Vector2.right, menuTransform)
                - xOffsetOfButtons * Vector2.right;

            // Creating / repositioning the inter-button bars.
            if (i != 0)
            {
                lastPosition = SpawnOrMoveTo(interButtonBarPrefab, lastPosition + distanceBetweenButtons / 2, ref interButtonBars, i - 1);
            }
        }
        // Lastly, we create / reposition the top bar.
        SpawnOrMoveTo(topBottomBarPrefab, lastPosition - distanceBetweenButtons / 2 + distanceTillMargins, ref topBar);

        // Find the parent of this object, to make sure that the button rack and other non-interactible object are
        // set as the first siblings, and that they wouldn't sit as a layer on top of buttons (which would
        // make the buttons unpressable).
        if (nonInteractibleParent == null)
            nonInteractibleParent = transform.GetComponentsInParent<Transform>().First(parent => parent.name.ToLower().Contains("no") && parent.name.ToLower().Contains("interact"));
        nonInteractibleParent.SetAsFirstSibling();
    }
}
