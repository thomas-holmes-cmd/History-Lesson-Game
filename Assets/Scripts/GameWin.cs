using UnityEngine;

public class GameWin : MonoBehaviour
{
    public void LoadCurrentScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        Time.timeScale = 1;
    }
    public void LoadCharacterSelectScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Character_Scene");
        Time.timeScale = 1;
    }
}
