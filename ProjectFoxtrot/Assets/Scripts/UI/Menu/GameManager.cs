using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour 
{
    public static GameManager instance = null;

    [Header("Pause menu")]
    [SerializeField] private Transform pauseMenu = null;



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

    private void Update()
    {
        if(Controls.GetActionDown(UserAction.Pause))
        {
            PauseGame(Time.timeScale == 1f ? true : false);
        }
    }

    public void LoadScene(string name)
    {
        SceneManager.LoadSceneAsync(name, LoadSceneMode.Single);
    }

    public void PauseGame(bool paused)
    {
        if(paused)
        {
            Time.timeScale = 0f;
            pauseMenu.GetComponent<Animator>().SetTrigger("Join_FromRight");
        }
        else
        {
            Time.timeScale = 1f;
            pauseMenu.GetComponent<Animator>().SetTrigger("Leave_ToRight");
        }

    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
