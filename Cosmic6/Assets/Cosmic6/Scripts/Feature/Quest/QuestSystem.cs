using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestSystem : MonoBehaviour
{
    public GameObject questPanel;
    public GameObject questItemPrefab; 
    public Transform questListContainer; 

    private Queue<Quest> questQueue;
    private List<Quest> activeQuests;

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
                new Quest("Flag Exploration 1", "Find the first flag", "Flag1", 1)
            }
        },
        {
            "Base2", new List<Quest>
            {
                new Quest("Flag Exploration 2", "Find the second flag", "Flag2", 1),
                new Quest("Flag Exploration 3", "Find the third flag", "Flag3", 1)
            }
        },
        {
            "Base3", new List<Quest>
            {
                new Quest("Flag Exploration 4", "Find the fourth flag", "Flag4", 1),
                new Quest("Flag Exploration 5", "Find the fifth flag", "Flag5", 1)
            }
        },
        {
            "EscapeFlag", new List<Quest>
            {
                new Quest("Escape spot activated", "Find the escape flag", "Escape Flag", 1)
            }
        }
    };


    void Start()
    {
        questPanel.SetActive(false);
        activeQuests = new List<Quest>();
        InitializeQuests();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            questPanel.SetActive(!questPanel.activeSelf);
            if (questPanel.activeSelf)
            {
                UpdateQuestDisplay();
            }
        }
    }

    void InitializeQuests()
    {
        questQueue = new Queue<Quest>();

        //questQueue.Enqueue(new Quest("Flag Exploration", "Find the first flag", "Flag1", 1));

        for (int i = 0; i < 1; i++)
        {
            if (questQueue.Count > 0)
            {
                Quest quest = questQueue.Dequeue();
                AddQuest(quest);
                ShowPopup(quest);
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
        contentText.text = $"{quest.description} (Progress: {quest.currentValue}/{quest.targetValue})";

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
                contentText.text = $"{quest.description} (Progress: {quest.currentValue}/{quest.targetValue})";
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

        foreach (var quest in baseQuests[baseName])
        {

            if (!activeQuests.Exists(q => q.title == quest.title))
            {
                AddQuest(quest);
                ShowPopup(quest);
            }
        }

        Debug.Log($"Quests updated for {baseName}");
        UpdateQuestDisplay();
    }

    /*public void UpdateQuest(string variable)
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

                    activeQuests.RemoveAt(i);
                    Destroy(questListContainer.Find($"Quest_{quest.title}").gameObject);

                    if (questQueue.Count > 0)
                    {
                        AddQuest(questQueue.Dequeue());
                    }
                }
            }
        }
    }*/

    public void ShowPopup(Quest quest)
    {
        if (popupPrefab != null)
        {
            GameObject popupInstance;
            if (popupParent != null)
            {
                popupInstance = Instantiate(popupPrefab, popupParent);
            }
            else
            {
                popupInstance = Instantiate(popupPrefab);
            }

            TMP_Text titleText = popupInstance.transform.Find("Title").GetComponent<TMP_Text>();
            TMP_Text contentText = popupInstance.transform.Find("Content").GetComponent<TMP_Text>();

            titleText.text = quest.title;
            contentText.text = quest.description;

            

            popupInstance.name = $"Popup_{quest.title}";

            activePopups.Add(popupInstance);
            UpdatePopupPositions();
            StartCoroutine(RemovePopupAfterDelay(popupInstance));
    
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

    private IEnumerator RemovePopupAfterDelay(GameObject popupInstance)
    {
        yield return new WaitForSeconds(popupDuration);

        if (activePopups.Contains(popupInstance))
        {
            activePopups.Remove(popupInstance);
            Destroy(popupInstance);

            UpdatePopupPositions();
        }
    }
}
