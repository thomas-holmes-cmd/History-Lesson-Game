using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartButtonHandler : MonoBehaviour
{

    public Button startButton;

    private void Awake()
    {
        if (startButton == null)
            startButton = GetComponent<Button>();

        startButton.interactable = false;
        startButton.onClick.AddListener(StartGame);
    }

    public void SetInteractable(bool state)
    {
        startButton.interactable = state;
    }

    private void StartGame()
    {

        if (!GameManager.instance.BothPlayersSelected()) return;

        SceneManager.LoadScene("Map_Scene");
    }
}