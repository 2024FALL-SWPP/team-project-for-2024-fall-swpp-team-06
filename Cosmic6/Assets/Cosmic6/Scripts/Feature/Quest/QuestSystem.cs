using System.Collections;
using System.Collections.Generic;
using DevionGames.InventorySystem.ItemActions;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;

public class QuestSystem : MonoBehaviour
{

    public GameManager gameManager;
    public GameObject questPanel;
    public GameObject questItemPrefab;
    public GameObject clearedQuestItemPrefab;
    public GameObject tutorialPopupPrefab;
    public PlayerStatusController playerStatusController;

    private BaseManager baseManager;
    private Transform mainQuestContainer;
    private Transform explorationQuestContainer;
    private Transform clearedQuestContainer;

    private Queue<Quest> questQueue;
    private List<Quest> activeQuests;

    public GameObject questclearPrefab;
    public Transform questclearParent;
    public Transform tutorialParent;

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
                new Quest("지역1 탐사", "지도(M)에 표시된 탐사 포인트를 찾아가자.",
                    "Flag1", default, Quest.QuestType.Exploration, 1)
            }
        },
        {
            "Base2", new List<Quest>
            {
                new Quest("지역2 탐사 - 1", "지도(M)에 표시된 탐사 포인트를 찾아가자.",
                    "Flag2", "고용량 산소통", Quest.QuestType.Exploration, 1),
                new Quest("지역2 탐사 - 2", "지도(M)에 표시된 탐사 포인트를 찾아가자.",
                    "Flag3", "순간이동기", Quest.QuestType.Exploration, 1)
            }
        },
        {
            "Base3", new List<Quest>
            {
                new Quest("지역3 탐사 - 1", "지도(M)에 표시된 탐사 포인트를 찾아가자.",
                    "Flag4", "방열복", Quest.QuestType.Exploration, 1),
                new Quest("지역3 탐사 - 2", "지도(M)에 표시된 탐사 포인트를 찾아가자.",
                    "Flag5", "초고용량 산소통", Quest.QuestType.Exploration, 1)
            }
        },
        {
            "EscapeFlag", new List<Quest>
            {
                new Quest("외계행성 탈출", "지도(M)에 표시된 탈출 포인트를 찾아가자.",
                    "Escape Flag", default, Quest.QuestType.Main, 1)
            }
        }
    };
    
    private List<GameObject> tutorials = new ();

    private List<string> tutorialContents = new()
    {
        "작물을 수확(F)하여 열매를 섭취하자. 인벤토리(I)에서 우클릭으로 섭취할 수 있다.",
        "파손된 우주선을 거점으로 등록하자. 각 지역의 거점에서 지역의 정보를 얻을 수 있다.",
        "농사를 지어 작물을 수확해 보자. 삽으로 밭을 갈면 씨앗을 심을 수 있다. 아이템은 인벤토리(I)에서 우측 하단 슬롯으로 드래그한 후 좌클릭하여 사용할 수 있다."
    };

    public int curTutorialIdx { get; private set; } = -1;

    void Awake()
    {
        questPanel.SetActive(false);
        activeQuests = new List<Quest>();
        gameManager.OnGameStart += GameStart;
        
        mainQuestContainer = questPanel.transform.GetChild(0).GetChild(0);
        explorationQuestContainer = questPanel.transform.GetChild(1).GetChild(0);
        clearedQuestContainer = questPanel.transform.GetChild(2).GetChild(0);
    }

    void GameStart()
    {
        StartCoroutine(GameStartCoroutine());
    }

    IEnumerator GameStartCoroutine()
    {
        yield return new WaitForSeconds(gameManager.gameStartAnimationDuration);
        InitializeQuests();
        yield return new WaitForSeconds(gameManager.gameStartDuration - gameManager.gameStartAnimationDuration);

        for (int i = 0; i < 3; i++)
        {
            var tutorialPopup = Instantiate(tutorialPopupPrefab, tutorialParent);
            
            tutorialPopup.SetActive(false);

            if (i == 2)
            {
                var size = tutorialPopup.GetComponent<RectTransform>().sizeDelta;

                tutorialPopup.GetComponent<RectTransform>().sizeDelta = new(size.x, 250);
            }

            var contentText = tutorialPopup.transform.GetChild(0).GetComponent<TMP_Text>();
            contentText.text = tutorialContents[i];
            tutorials.Add(tutorialPopup);
        }

        StartCoroutine(GenerateTutorialPopup(0));
    }

    IEnumerator GenerateTutorialPopup(int id)
    {
        var tutorialPopup = tutorials[id];
        curTutorialIdx = id;
        tutorialPopup.SetActive(true);
        yield return FadePopup(tutorialPopup, false, true);
    }
    
    public void DestroyTutorialPopup(int id)
    {
        StartCoroutine(DestroyCoroutine(id));
    }

    IEnumerator DestroyCoroutine(int id)
    {
        var popupInstance = tutorials[id];
        if (id != curTutorialIdx)
        {
            Destroy(popupInstance);
            tutorials[id] = null;
            yield break;
        }

        yield return FadePopup(popupInstance, false, false);
        Destroy(popupInstance);

        for (int i = id + 1; i < 3; i++)
        {
            if (tutorials[i] != null)
            {
                StartCoroutine(GenerateTutorialPopup(i));
                yield break;
            }
        }
    }
    
    void Start()
    {
        playerStatusController.Tutorial1Complete += DestroyTutorialPopup;
        baseManager = gameManager.GetComponent<BaseManager>();
        baseManager.Tutorial2Complete += DestroyTutorialPopup;
        FarmingManager.Instance.Tutorial3Complete += DestroyTutorialPopup;
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

        questQueue.Enqueue(new Quest("통신장비 수집", "외계행성 탈출을 위해 지구에 연락을 취해야 한다. 탐지기를 활용해서 모든 통신장비를 찾자.",
            "Tele", "완성된 통신장비", Quest.QuestType.Main, 7));

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
        GameObject questItem = default;

        switch (quest.type)
        {
            case Quest.QuestType.Main:
                questItem = Instantiate(questItemPrefab, mainQuestContainer);
                break;
            case Quest.QuestType.Exploration:
                questItem = Instantiate(questItemPrefab, explorationQuestContainer);
                break;
        }

        TMP_Text titleText = questItem.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
        TMP_Text contentText = questItem.transform.GetChild(1).GetComponent<TMP_Text>();

        titleText.text = quest.title;
        contentText.text = quest.description;
        if (quest.targetValue != 1)
            contentText.text += $"\n(진행: {quest.currentValue}/{quest.targetValue})";

        questItem.name = $"Quest_{quest.title}";
    }

    private void AddClearedQuest(Quest quest)
    {
        GameObject clearedQuestItem = Instantiate(clearedQuestItemPrefab, clearedQuestContainer);
        
        TMP_Text titleText = clearedQuestItem.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
        TMP_Text contentText = clearedQuestItem.transform.GetChild(1).GetComponent<TMP_Text>();
        
        titleText.text = quest.title;
        var rewardText = "보상: ";
        rewardText += quest.reward == default ? "경험" : quest.reward;

        if (quest.reward != default)
        {
            switch (quest.reward[0])
            {
                case '고':
                    rewardText += "\n산소통 용량이 100 늘어났다.";
                    break;
                case '순':
                    rewardText += "\n이제 거점 사이를 순간이동(T) 할 수 있다.";
                    break;
                case '방':
                    rewardText += "\n이제 고온을 버틸 수 있다.";
                    break;
                case '초':
                    rewardText += "\n산소통 용량이 200 늘어났다.";
                    break;
                case '완':
                    rewardText += "\n지구에 구조요청을 보낼 수 있게 됐다.";
                    break;
            }
        }

        contentText.text = rewardText;
        
        clearedQuestItem.name = $"ClearedQuest_{quest.title}";
    }

    private void UpdateQuestDisplay()
    {
        foreach (Transform questItem in mainQuestContainer)
        {
            TMP_Text contentText = questItem.GetChild(1).GetComponent<TMP_Text>();
            string questTitle = questItem.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text;

            Quest quest = activeQuests.Find(q => q.title == questTitle);
            if (quest != null && quest.targetValue != 1)
            {
                contentText.text = quest.description + $"\n(진행: {quest.currentValue}/{quest.targetValue})";
            }
        }
        
        foreach (Transform questItem in explorationQuestContainer)
        {
            TMP_Text contentText = questItem.GetChild(1).GetComponent<TMP_Text>();
            string questTitle = questItem.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text;

            Quest quest = activeQuests.Find(q => q.title == questTitle);
            if (quest != null && quest.targetValue != 1)
            {
                contentText.text = quest.description + $"\n(진행: {quest.currentValue}/{quest.targetValue})";
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

                    switch (quest.type)
                    {
                        case Quest.QuestType.Main:
                            Destroy(mainQuestContainer.Find($"Quest_{quest.title}").gameObject);
                            break;
                        case Quest.QuestType.Exploration:
                            Destroy(explorationQuestContainer.Find($"Quest_{quest.title}").gameObject);
                            break;
                    }
                    
                    AddClearedQuest(quest);
                    
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
        
        StartCoroutine(UpdateQuestCoroutine(baseName));
    }

    IEnumerator UpdateQuestCoroutine(string baseName)
    {
        string popupMessage;
        if (baseName.StartsWith('B'))
        {
            var baseNumChar = baseName[baseName.Length - 1];
            popupMessage = "지역" + baseNumChar + " 정보 다운로드 중";
            
        }
        else
        {
            popupMessage = "지구에 구조요청 보내는 중";
        }

        var popupInstance = Instantiate(popupPrefab, popupParent);
        ShowPopup(popupInstance);
        popupInstance.name = "Popup_" + baseName;
        
        TMP_Text contentText = popupInstance.transform.GetChild(0).GetComponent<TMP_Text>();
        contentText.text = popupMessage;

        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(1.5f);
            contentText.text += '.';
        }
        
        yield return new WaitForSeconds(1.5f);

        contentText.text = baseName.StartsWith('B') ? "다운로드 완료!" : "구조요청 완료!";

        yield return new WaitForSeconds(2.5f);
        
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

            TMP_Text cleartitleText = clearInstance.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
            TMP_Text clearcontentText = clearInstance.transform.GetChild(1).GetComponent<TMP_Text>();

            cleartitleText.text = quest.title + " 완료!";
            clearcontentText.text = quest.reward == default
                ? "아쉽지만 아무것도 발견하지 못했다..."
                : quest.reward + (quest.reward.EndsWith("기") || quest.reward.EndsWith("비") ? "를 획득했다!" : "을 획득했다!");
            
            clearInstance.name = $"Popup_{quest.title}";

            activePopups.Add(clearInstance);
            //UpdatePopupPositions();
            StartCoroutine(RemovePopupAfterDelay(clearInstance, true));
    
        }
    }

    public void ShowPopup(GameObject popupInstance)
    {
        activePopups.Add(popupInstance);
        StartCoroutine(RemovePopupAfterDelay(popupInstance, false, 8f));
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
