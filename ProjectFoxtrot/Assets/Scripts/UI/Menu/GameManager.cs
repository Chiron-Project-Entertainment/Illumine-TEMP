using UnityEngine;

public class GameManager : MonoBehaviour 
{
    public static GameManager instance = null;

    private void Awake()
    {
        // Set the gameManager static, with only one instance available per scene and unkillable when scenes change 
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
