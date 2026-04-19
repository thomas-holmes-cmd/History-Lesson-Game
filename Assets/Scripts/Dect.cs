using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectDebugger : MonoBehaviour
{

    [Header("Assign these to check system")]
    public GameManager gameManager;
    public CharacterSelectionUI ui;
    public StartButtonHandler startButtonHandler;
    public GameObject optionPrefab;
    public Transform container;

    private void Start()
    {
        Debug.Log("===== CHARACTER SELECT DEBUG START =====");

        CheckGameManager();
        CheckCharacters();
        CheckUI();
        CheckPrefab();
        CheckStartButton();

        Debug.Log("===== DEBUG COMPLETE =====");
    }

    void CheckGameManager()
    {

        if (gameManager == null)
        {
            Debug.LogError("❌ GameManager NOT assigned in Inspector!");
            return;
        }

        if (GameManager.instance == null)
        {
            Debug.LogError("❌ GameManager.instance is NULL (GameManager not in scene or not Awake yet)");
            return;
        }

        Debug.Log("✔ GameManager exists");
    }

    void CheckCharacters()
    {

        if (GameManager.instance == null) return;

        if (GameManager.instance.characters == null)
        {
            Debug.LogError("❌ Characters array is NULL");
            return;
        }

        if (GameManager.instance.characters.Length == 0)
        {
            Debug.LogError("❌ Characters array is EMPTY (size = 0)");
            return;
        }

        Debug.Log("✔ Characters found: " + GameManager.instance.characters.Length);
    }

    void CheckUI()
    {

        if (ui == null)
        {
            Debug.LogError("❌ CharacterSelectionUI not assigned");
            return;
        }

        Debug.Log("✔ UI script exists");
    }

    void CheckPrefab()
    {

        if (optionPrefab == null)
        {
            Debug.LogError("❌ optionPrefab NOT assigned");
            return;
        }

        Button b = optionPrefab.GetComponent<Button>();

        if (b == null)
        {
            Debug.LogError("❌ optionPrefab has NO Button component");
        }
        else
        {
            Debug.Log("✔ Prefab has Button");
        }

        Debug.Log("✔ Prefab exists");
    }

    void CheckStartButton()
    {

        if (startButtonHandler == null)
        {
            Debug.LogError("❌ StartButtonHandler not assigned");
            return;
        }

        Button b = startButtonHandler.GetComponent<Button>();

        if (b == null)
        {
            Debug.LogError("❌ StartButtonHandler object has NO Button component");
        }
        else
        {
            Debug.Log("✔ Start Button exists");
        }
    }
}
