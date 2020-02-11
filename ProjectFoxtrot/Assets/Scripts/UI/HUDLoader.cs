using UnityEngine;
using UnityEngine.SceneManagement;

public class HUDLoader : MonoBehaviour
{
	private bool GameStarted = false;

	private void Start()
	{
		// Trying to see if the menu scene is already loaded or not.
		// We're also checking if right now we're in game or at the main menu.
		bool menuSceneLoaded = false;
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene scene = SceneManager.GetSceneAt(i);
			if (scene.name.Equals("Menu"))
			{
				menuSceneLoaded = true;
			}
			else if (scene.name.ToLower().Contains("level"))
			{
				GameStarted = true;
			}
		}

		if (!menuSceneLoaded)
		{
			SceneManager.LoadScene("Menu", LoadSceneMode.Additive);
		}
	}

	private void Update()
	{
		// Now, that we made sure the MenuScene is loaded, get the animators.
		GameObject auxObj;
		auxObj = GameObject.FindGameObjectWithTag("MainMenu");
		if (auxObj != null)
		{
			if (!GameStarted)
			{
				auxObj.GetComponent<Animator>().SetTrigger("Join_FromLeft");
			}
			Destroy(gameObject);
		}
	}
}
