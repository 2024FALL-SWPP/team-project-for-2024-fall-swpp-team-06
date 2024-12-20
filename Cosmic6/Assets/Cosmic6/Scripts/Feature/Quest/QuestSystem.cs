using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;

public class QuestSystem : MonoBehaviour
{

    public GameManager gameManager;
    public GameObject questPanel;
    public GameObject questItemPrefab; 
    public Transform questListContainer;

    private Queue<Quest> questQueue;
    private List<Quest> activeQuests;

    public GameObject questclearPrefab;
    public Transform questclearParent;

    public GameObject questPopupPrefab;
    public GameObject popupPrefab;
    
    public Transform popupParent;
    public float popupDuration = 5f;
    public float verticalSpacing = 60f;

    private List<GameObject> activePopups = new List<GameObject>();

    private Dictionary<string, List<Quest>> baseQuests = new Dictionary<string, List<Quest>>()
    {
        {
            "Base1", new List<Quest>
            {
                new Quest("지역1 탐사", "지도(M)에 표시된 탐사 포인트를 찾아가자.", "Flag1", default, 1)
            }
        },
        {
            "Base2", new List<Quest>
            {
                new Quest("지역2 탐사 - 1", "지도(M)에 표시된 탐사 포인트를 찾아가자.", "Flag2", "고용량 산소통", 1),
                new Quest("지역2 탐사 - 2", "지도(M)에 표시된 탐사 포인트를 찾아가자.", "Flag3", "순간이동기", 1)
            }
        },
        {
            "Base3", new List<Quest>
            {
                new Quest("지역3 탐사 - 1", "지도(M)에 표시된 탐사 포인트를 찾아가자.", "Flag4", "방열복", 1),
                new Quest("지역3 탐사 - 2", "지도(M)에 표시된 탐사 포인트를 찾아가자.", "Flag5", "초고용량 산소통", 1)
            }
        },
        {
            "EscapeFlag", new List<Quest>
            {
                new Quest("외계행성 탈출", "지도(M)에 표시된 탈출 포인트를 찾아가자.", "Escape Flag", default, 1)
            }
        }
    };

    void Awake()
    {
        questPanel.SetActive(false);
        activeQuests = new List<Quest>();
        gameManager.OnGameStart += GameStart;
    }

    void GameStart()
    {
        StartCoroutine(GameStartCoroutine());
    }

    IEnumerator GameStartCoroutine()
    {
        yield return new WaitForSeconds(gameManager.gameStartAnimationDuration);
        InitializeQuests();
    }
    
    void Start()
    {
        
    }

    public void ToggleActive()
    {
        questPanel.SetActive(!questPanel.activeSelf);
        if (questPanel.activeSelf)
        {
            UpdateQuestDisplay();
        }
    }

    void InitializeQuests()
    {
        questQueue = new Queue<Quest>();

        questQueue.Enqueue(new Quest("통신장비 수집", "외계행성 탈출을 위해 지구에 연락을 취해야 한다. 모든 통신장비를 찾자.", "Tele", default,  7));

        for (int i = 0; i < 1; i++)
        {
            if (questQueue.Count > 0)
            {
                Quest quest = questQueue.Dequeue();
                AddQuest(quest);
                ShowQuestPopup(quest);
            }
        }
    }



    private void AddQuest(Quest quest)
    {
        activeQuests.Add(quest);

        GameObject questItem = Instantiate(questItemPrefab, questListContainer);
        RectTransform rectTransform = questItem.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(700, 50);

        TMP_Text titleText = questItem.transform.Find("Title").Find("Text").GetComponent<TMP_Text>();
        TMP_Text contentText = questItem.transform.Find("Content").Find("Text").GetComponent<TMP_Text>();

        titleText.text = quest.title;
        contentText.text = $"{quest.description} (진행: {quest.currentValue}/{quest.targetValue})";

        questItem.name = $"Quest_{quest.title}";
    }

    private void UpdateQuestDisplay()
    {
        foreach (Transform questItem in questListContainer)
        {
            TMP_Text contentText = questItem.Find("Content").Find("Text").GetComponent<TMP_Text>();
            string questTitle = questItem.Find("Title").Find("Text").GetComponent<TMP_Text>().text;

            Quest quest = activeQuests.Find(q => q.title == questTitle);
            if (quest != null)
            {
                if (quest.targetValue != 1)
                    contentText.text = $"{quest.description} (진행: {quest.currentValue}/{quest.targetValue})";
            }
        }
    }

    public void UpdateQuestProgress(string variable, int value)
    {
        for (int i = activeQuests.Count - 1; i >= 0; i--)
        {
            Quest quest = activeQuests[i];

            if (quest.conditionVariable == variable)
            {
                quest.UpdateProgress(value);

                if (quest.IsComplete())
                {
                    Debug.Log($"{quest.title} completed!");
                    clearPopup(quest);
                    activeQuests.RemoveAt(i);
                    Destroy(questListContainer.Find($"Quest_{quest.title}").gameObject);

                    if (questQueue.Count > 0)
                    {
                        AddQuest(questQueue.Dequeue());
                    }
                }
            }
        }
        UpdateQuestDisplay();
    }

    public void UpdateQuest(string baseName, int progressValue)
    {
        if (!baseQuests.ContainsKey(baseName))
        {
            Debug.LogWarning($"No quests found for {baseName}");
            return;
        }

        var baseIndex = baseName[baseName.Length - 1] - '0';
        
        foreach (var quest in baseQuests[baseName])
        {
            if (!activeQuests.Exists(q => q.title == quest.title))
            {
                AddQuest(quest);
                ShowQuestPopup(quest);
            }
        }

        Debug.Log($"Quests updated for {baseName}");
        UpdateQuestDisplay();
    }

    public void clearPopup(Quest quest)
    {
        if (questclearPrefab != null)
        {
            GameObject clearInstance;
            if (questclearParent != null)
            {
                clearInstance = Instantiate(questclearPrefab, questclearParent);
            }
            else
            {
                clearInstance = Instantiate(questclearPrefab);
            }

            TMP_Text cleartitleText = clearInstance.transform.Find("Title").GetComponent<TMP_Text>();
            TMP_Text clearcontentText = clearInstance.transform.Find("Content").GetComponent<TMP_Text>();

            cleartitleText.text = "CLEAR!";
            clearcontentText.text = quest.title;

            

            clearInstance.name = $"Popup_{quest.title}";

            Destroy(clearInstance, 5f);
    
        }
    }

    public void ShowPopup(string text)
    {
        GameObject popupInstance = Instantiate(popupPrefab, popupParent);
        
        
    }

    public void ShowQuestPopup(Quest quest)
    {
        if (questPopupPrefab != null)
        {
            GameObject popupInstance;
            if (popupParent != null)
            {
                popupInstance = Instantiate(questPopupPrefab, popupParent);
            }
            else
            {
                popupInstance = Instantiate(questPopupPrefab);
            }

            TMP_Text titleText = popupInstance.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
            TMP_Text contentText = popupInstance.transform.GetChild(1).GetComponent<TMP_Text>();

            titleText.text = quest.title;
            contentText.text = quest.description;

            

            popupInstance.name = $"Popup_{quest.title}";

            activePopups.Add(popupInstance);
            //UpdatePopupPositions();
            StartCoroutine(RemovePopupAfterDelay(popupInstance, true));
    
        }
    }

    private void UpdatePopupPositions()
    {
        for (int i = 0; i < activePopups.Count; i++)
        {
            RectTransform rectTransform = activePopups[i].GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, -i * verticalSpacing);
        }
    }

    private IEnumerator RemovePopupAfterDelay(GameObject popupInstance, bool isQuest, float duration=5f)
    {
        yield return FadePopup(popupInstance, isQuest, true);
        yield return new WaitForSeconds(duration - 1f);
        yield return FadePopup(popupInstance, isQuest, false);

        if (activePopups.Contains(popupInstance))
        {
            activePopups.Remove(popupInstance);
            Destroy(popupInstance);

            //UpdatePopupPositions();
        }
    }

    private IEnumerator FadePopup(GameObject popupInstance, bool isQuest, bool isOn, float duration = 0.5f)
    {
        float timer = 0;
        Image background = popupInstance.GetComponent<Image>();
        
        Color backgroundColor = background.color;
        Image title = isQuest ? popupInstance.transform.GetChild(0).GetComponent<Image>() : default;
        Color titleColor = isQuest ? title.color : default;

        TMP_Text titleText =
            isQuest ? popupInstance.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>() : default;
        TMP_Text contentText = isQuest ? popupInstance.transform.GetChild(1).GetComponent<TMP_Text>() :
            popupInstance.transform.GetChild(0).GetComponent<TMP_Text>();
        
        while (timer < duration)
        {
            timer += 0.02f;
            var textColor = Color.Lerp(isOn ? Color.clear : Color.white, isOn ? Color.white : Color.clear,
                timer / duration);
            
            
            if (isQuest)
            {
                title.color = new(titleColor.r, titleColor.g, titleColor.b,
                    Mathf.Lerp(isOn ? 0 : titleColor.a, isOn ? titleColor.a : 0, timer / duration));
                titleText.color = textColor;
            }
            
            background.color = new(backgroundColor.r, backgroundColor.g, backgroundColor.b,
                Mathf.Lerp(isOn ? 0 : backgroundColor.a, isOn ? backgroundColor.a : 0, timer / duration));
            contentText.color = textColor;
            
            yield return new WaitForSeconds(0.02f);
        }
    }
}
