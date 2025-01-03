// Quest.cs
using System;

[Serializable]
public class Quest
{
    public enum QuestType
    {
        Main, Exploration
    }
    
    public string title;
    public string description;
    public string reward;
    public QuestType type;
    
    public string conditionVariable;
    public int targetValue;
    public int currentValue;

    public bool isComplete { get; protected set; }

    public Quest(string title, string description, string conditionVariable, string reward, QuestType type,
        int targetValue)
    {
        this.title = title;
        this.description = description;
        this.reward = reward;
        this.type = type;
        this.conditionVariable = conditionVariable;
        this.targetValue = targetValue;
        this.currentValue = 0;
        this.isComplete = false;
    }

    public bool IsComplete()
    {
        return isComplete;
    }

    public virtual void CheckProgress()
    {
        if (currentValue >= targetValue)
        {
            isComplete = true;
        }
    }

    public void UpdateProgress(int value)
    {
        currentValue += value;
        CheckProgress();
    }

}
