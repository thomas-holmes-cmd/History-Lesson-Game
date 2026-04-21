using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CharacterSelectionUI : MonoBehaviour
{

    public GameObject optionPrefab;
    public Transform container;
    public StartButtonHandler startButtonHandler;

    private Transform[] allOptions;

    private Transform p1;
    private Transform p2;

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


            Transform iconTransform = obj.transform.Find("Icon");
            Outline outline = iconTransform.GetComponent<Outline>();
            if (outline != null)
                outline.enabled = false;

            allOptions[i] = obj.transform;
            i++;

            btn.onClick.AddListener(() => HandleClick(c, obj.transform));
        }
    }


    private void HandleClick(Character character, Transform option)
    {

        // Check if clicking on already selected P1 option
        if (p1 == option)
        {
            ClearP1();
            return;
        }

        // Check if clicking on already selected P2 option
        if (p2 == option)
        {
            ClearP2();
            return;
        }

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

        GameManager.instance.SetCharacter(1, c);

        p1 = option;

        SetOutline(option, Color.blue);

        UpdateStart();
    }

    void ClearP1()
    {

        if (p1 != null)
            SetOutline(p1, false);

        GameManager.instance.SetCharacter(1, null);
        p1 = null;

        UpdateStart();
    }


    void SetP2(Character c, Transform option)
    {

        GameManager.instance.SetCharacter(2, c);

        p2 = option;

        SetOutline(option, new Color(1f, 0.5f, 0f));

        UpdateStart();
    }

    void ClearP2()
    {

        if (p2 != null)
            SetOutline(p2, false);

        GameManager.instance.SetCharacter(2, null);
        p2 = null;

        UpdateStart();
    }


    void SetOutline(Transform option, Color color)
    {

        Transform icon = option.Find("Icon");
        if (icon == null) return;

        Outline o = icon.GetComponent<Outline>();
        if (o == null) return;

        o.enabled = true;
        o.effectColor = color;
        o.effectDistance = new Vector2(4f, -4f);
    }

    void SetOutline(Transform option, bool state)
    {

        Transform icon = option.Find("Icon");
        if (icon == null) return;

        Outline o = icon.GetComponent<Outline>();
        if (o != null)
            o.enabled = state;
    }


    private void Update()
    {

        if (allOptions == null) return;

        for (int i = 0; i < allOptions.Length; i++)
        {

            if (allOptions[i] == null) continue;

            bool selected =
                allOptions[i] == p1 ||
                allOptions[i] == p2;

            Vector3 target = selected ? selectedScale : normalScale;

            allOptions[i].localScale = Vector3.Lerp(
                allOptions[i].localScale,
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