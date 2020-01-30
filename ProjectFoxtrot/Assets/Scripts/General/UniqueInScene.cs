using System.Linq;
using UnityEngine;

/// <summary> 
/// Self explanatory. Once a scene loads up,
/// this object makes sure he's the only one of his kind. 
/// </summary>
public class UniqueInScene : MonoBehaviour 
{
	// Use this for initialization
	void Awake () 
	{
		// Set the gameManager static, with only one instance available per scene and unkillable when scenes change 
		if (FindObjectsOfType(typeof(UniqueInScene)).Count(obj => obj.name == gameObject.name) > 1)
		{
			Destroy(gameObject);
			return;
		}
	}
}
