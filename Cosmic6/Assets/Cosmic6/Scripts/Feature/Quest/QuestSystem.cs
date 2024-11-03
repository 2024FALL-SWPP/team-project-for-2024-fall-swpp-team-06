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
        questQueue.Enqueue(new Quest("Collect Gold", "Gather at least 100 gold coins.", "Gold", 100));
        questQueue.Enqueue(new Quest("Defeat Enemies", "Eliminate 5 enemies in the field.", "Enemies", 5));
        questQueue.Enqueue(new Quest("Gather Herbs", "Collect 5 healing herbs from the meadow.", "Herbs", 5));
        questQueue.Enqueue(new Quest("Deliver Message", "Take the letter to the town.", "Deliveries", 1));

        for (int i = 0; i < 4; i++)
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
        }
    }

    public void GenerateQuest(Quest quest)
    {
        AddQuest(quest);
        ShowPopup(quest);
    }
}
