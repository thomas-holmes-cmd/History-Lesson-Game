using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    public Character[] characters;

    public Character player1Character;
    public Character player2Character;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetCharacter(int player, Character character)
    {
        if (player == 1) player1Character = character;
        if (player == 2) player2Character = character;
    }

    public bool BothPlayersSelected()
    {
        return player1Character != null && player2Character != null;
    }
}