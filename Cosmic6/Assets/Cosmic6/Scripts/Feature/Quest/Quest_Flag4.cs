using UnityEngine;

public class Quest_Flag4 : Quest
{
    private FlagState flagstate;

    public Quest_Flag4(string title, string description)
        : base(title, description, "Flag4", 1)
    {
    }

    public override void CheckProgress()
    {
        if (flagstate == null)
        {
            Debug.LogError("FlagState is null! Make sure it is set.");
            return;
        }

        
        if (flagstate.flag4_Registered)
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
    
    

