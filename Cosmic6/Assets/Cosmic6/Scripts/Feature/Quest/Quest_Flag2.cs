using UnityEngine;

public class Quest_Flag2 : Quest
{
    /*private FlagState flagstate;

    public Quest_Flag2(string title, string description)
        : base(title, description, "Flag2", 1)
    {
    }

    public override void CheckProgress()
    {
        if (flagstate == null)
        {
            Debug.LogError("FlagState is null! Make sure it is set.");
            return;
        }

        
        if (flagstate.flag2_Registered)
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

    public Quest_Flag2(string title, string description)
        : base(title, description, "Flag2", default, QuestType.Exploration, 1)
    {
    }

    public override void CheckProgress()
    {
        if (flagManager.isFlagRegistered[1])
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
    
    

