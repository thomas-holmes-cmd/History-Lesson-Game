using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectionUI : MonoBehaviour
{
    public GameObject optionPrefab;
    public Transform container;
    public StartButtonHandler startButtonHandler;

    private Transform[] allOptions;

    private Transform p1;
    private Transform p2;

    private Character p1Char;
    private Character p2Char;

    private Vector3 normalScale = Vector3.one;
    private Vector3 selectedScale = new Vector3(1.2f, 1.2f, 1.2f);

    private void Start()
    {
        if (GameManager.instance == null || GameManager.instance.characters == null)
            return;

        startButtonHandler.SetInteractable(false);

        allOptions = new Transform[GameManager.instance.characters.Length];

        int i = 0;

        foreach (Character c in GameManager.instance.characters)
        {
            GameObject obj = Instantiate(optionPrefab, container);

            Button btn = obj.GetComponent<Button>();

            TMP_Text text = obj.GetComponentInChildren<TMP_Text>();
            if (text != null) text.text = c.name;

            Image icon = obj.transform.Find("Icon").GetComponent<Image>();
            if (icon != null) icon.sprite = c.icon;

            allOptions[i] = obj.transform;
            i++;

            btn.onClick.AddListener(() => HandleClick(c, obj.transform));
        }
    }

    private void HandleClick(Character character, Transform option)
    {
        if (p1 == null)
        {
            SetP1(character, option);
            return;
        }

        if (p2 == null)
        {
            SetP2(character, option);
            return;
        }
    }

    void SetP1(Character c, Transform option)
    {
        p1 = option;
        p1Char = c;

        GameManager.instance.SetCharacter(1, c);

        UpdateVisuals();
        UpdateStart();
    }

    void SetP2(Character c, Transform option)
    {
        p2 = option;
        p2Char = c;

        GameManager.instance.SetCharacter(2, c);

        UpdateVisuals();
        UpdateStart();
    }

    void UpdateVisuals()
    {
        foreach (Transform option in allOptions)
        {
            if (option == null) continue;

            Transform icon = option.Find("Icon");
            Outline outline = icon.GetComponent<Outline>();

            if (outline == null) continue;

            bool isP1 = option == p1;
            bool isP2 = option == p2;

            if (!isP1 && !isP2)
            {
                outline.enabled = false;
            }
            else
            {
                outline.enabled = true;

                if (isP1 && isP2)
                {
                    outline.effectColor = Color.magenta;
                }
                else if (isP1)
                {
                    outline.effectColor = Color.blue;
                }
                else if (isP2)
                {
                    outline.effectColor = new Color(1f, 0.5f, 0f);
                }
            }
        }
    }

    private void Update()
    {
        if (allOptions == null) return;

        foreach (Transform option in allOptions)
        {
            if (option == null) continue;

            bool selected = option == p1 || option == p2;

            Vector3 target = selected ? selectedScale : normalScale;

            option.localScale = Vector3.Lerp(
                option.localScale,
                target,
                Time.deltaTime * 10f
            );
        }
    }

    void UpdateStart()
    {
        bool bothSelected = GameManager.instance.BothPlayersSelected();
        startButtonHandler.SetInteractable(bothSelected);
    }
}