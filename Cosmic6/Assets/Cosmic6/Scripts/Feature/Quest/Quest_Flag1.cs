using UnityEngine;

public class Quest_Flag1 : Quest
{
    /*private FlagState flagstate;

    public Quest_Flag1(string title, string description)
        : base(title, description, "Flag1", 1)
    {
    }

    public override void CheckProgress()
    {
        if (flagstate == null)
        {
            Debug.LogError("FlagState is null! Make sure it is set.");
            return;
        }

        
        if (flagstate.flag1_Registered)
        {
            currentValue = 1; 
            isComplete = true; 
            Debug.Log($"Quest '{title}' is complete!");
        }
        else
        {
            currentValue = 0;
            isComplete = false;
            Debug.Log($"Quest '{title}' is not complete.");
        }
    }*/

    private FlagManager flagManager;

    public Quest_Flag1(string title, string description)
        : base(title, description, "Flag1", default, QuestType.Exploration, 1)
    {
    }

    public override void CheckProgress()
    {
        if (flagManager.isFlagRegistered[0])
        {
            currentValue = 1;
            isComplete = true;
            Debug.Log($"Quest '{title}' is complete!");
        }
        else
        {
            currentValue = 0;
            isComplete = false;
            Debug.Log($"Quest '{title}' is not complete.");
        }
    }
}
    
    

