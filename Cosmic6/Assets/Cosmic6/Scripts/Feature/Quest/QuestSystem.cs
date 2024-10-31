using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestSystem : MonoBehaviour
{
    public GameObject questPanel;

    public Text title1Text;
    public Text content1Text;
    public Text title2Text;
    public Text content2Text;

    private Queue<Quest> questQueue;
    private Quest[] activeQuests;

    // Start is called before the first frame update
    void Start()
    {
        questPanel.SetActive(false);
        activeQuests = new Quest[2];
        InitializeQuests();
    }

    public void ToggleQuestPanel()
    {
        if (!questPanel.activeSelf)
        {
            questPanel.SetActive(true);
            UpdateQuestDisplay();
        }
        else
        {
            questPanel.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitializeQuests()
    {
        questQueue = new Queue<Quest>();
        questQueue.Enqueue(new Quest("Collect Gold", "Gather at least 100 gold coins.", "Gold", 100));
        questQueue.Enqueue(new Quest("Defeat Enemies", "Eliminate 5 enemies in the field.", "Enemies", 5));
        questQueue.Enqueue(new Quest("Gather Herbs", "Collect 5 healing herbs from the meadow.", "Herbs", 5));
        questQueue.Enqueue(new Quest("Deliver Message", "Take the letter to the town.", "Deliveries", 1));

        for (int i = 0; i < activeQuests.Length; i++)
        {
            if (questQueue.Count > 0)
            {
                activeQuests[i] = questQueue.Dequeue();
            }
        }
    }

    void UpdateQuestDisplay()
    {
        title1Text.text = activeQuests[0] != null ? activeQuests[0].title : "";
        content1Text.text = activeQuests[0] != null ? $"{activeQuests[0].description} (Progress: {activeQuests[0].currentValue}/{activeQuests[0].targetValue})" : "";

        title2Text.text = activeQuests[1] != null ? activeQuests[1].title : "";
        content2Text.text = activeQuests[1] != null ? $"{activeQuests[1].description} (Progress: {activeQuests[1].currentValue}/{activeQuests[1].targetValue})" : "";

        Debug.Log("Displayed: " + activeQuests[0].title + " / " + activeQuests[1].title);
    }

    public void UpdateQuestProgress(string variable, int value)
    {
        for (int i = 0; i < activeQuests.Length; i++)
        {
            Quest quest = activeQuests[i];

            if (quest != null && quest.conditionVariable == variable)
            {
                quest.UpdateProgress(value);

                if (quest.IsComplete())
                {
                    Debug.Log($"{quest.title} completed!");
                    if (questQueue.Count > 0)
                    {
                        activeQuests[i] = questQueue.Dequeue();
                    }
                    else
                    {
                        activeQuests[i] = null;
                    }
                }
            }
        }
        UpdateQuestDisplay();
    }

}
