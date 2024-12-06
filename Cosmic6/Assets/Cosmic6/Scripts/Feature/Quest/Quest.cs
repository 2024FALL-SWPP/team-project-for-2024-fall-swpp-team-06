// Quest.cs
using System;

[Serializable]
public class Quest
{
    public string title;
    public string description;

    public string conditionVariable;
    public int targetValue;
    public int currentValue;

    public bool isComplete { get; protected set; }

    public Quest(string title, string description, string conditionVariable, int targetValue)
    {
        this.title = title;
        this.description = description;
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
