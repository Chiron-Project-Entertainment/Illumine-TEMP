///////////////////////////////////////////////////////////////
///                                                         ///
///             Script coded by Hakohn (Robert).            ///
///                                                         ///
///////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour 
{
    public static GameManager instance = null;

    [Header("General")]
    [SerializeField] private Animator mainMenuAnimator = null;

    [Header("Pause menu")]
    [SerializeField] private Animator pauseMenuAnimator = null;

    public bool GameIsPaused { get { return Time.timeScale == 0f ? true : false; } set { PauseGame(value); } }
    public bool GameStarted { get; set; }

    private void Awake()
    {
        // Set the gameManager static, with only one instance available per scene and not killable when scenes change 
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
        SceneManager.LoadScene(name, LoadSceneMode.Single);
    }

    public void PauseGame(bool pause)
    {
        if(pause)
        {
            Time.timeScale = 0f;
            pauseMenuAnimator.GetComponent<Animator>().SetTrigger("Join_FromLeft");
        }
        else
        {
            Time.timeScale = 1f;
            pauseMenuAnimator.GetComponent<Animator>().SetTrigger("Leave_ToLeft");
        }
    }

    public void BackSettingsMenu()
    {
        if(GameIsPaused)
        {
            pauseMenuAnimator.SetTrigger("Join_FromLeft");
        }
        else
        {
            mainMenuAnimator.SetTrigger("Join_FromLeft");
        }
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
