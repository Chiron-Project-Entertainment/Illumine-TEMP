using UnityEngine;

public class UserInterface : MonoBehaviour 
{
	public static UserInterface instance = null;

	public void Awake()
	{
		if (instance == null)
			instance = this;
		else
		{
			Destroy(gameObject);
			return;
		}
	}
}
