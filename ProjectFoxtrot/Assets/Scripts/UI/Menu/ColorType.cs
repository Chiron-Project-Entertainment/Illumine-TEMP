///////////////////////////////////////////////////////////////
///                                                         ///
///             Script coded by Hakohn (Robert).            ///
///                                                         ///
///////////////////////////////////////////////////////////////

using UnityEngine;

public enum ColorCategory
{
    Primary, PrimayLight, PrimaryDark,
    Secondary, SecondaryLight, SecondaryDark,
    TextMain, TextAlternative, TextDropdown
}

public class ColorType : MonoBehaviour
{
    public ColorCategory category;
    [HideInInspector] public Color color;
}
