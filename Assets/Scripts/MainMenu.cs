using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        // Optional: stop or fade out menu music here
        MusicManager.I?.HardStopAll();
        SceneManager.LoadScene(1);
    }
}