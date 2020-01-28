using UnityEngine;

/// <summary> 
/// Self explanatory. Once a scene loads up,
/// this object will remain alive. 
/// </summary>
public class DontDestroyOnLoad : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
