using UnityEngine;

public class Quest_EscapeFlag : Quest
{
    private FlagManager flagManager;
    private GameManager gameManager;

    public Quest_EscapeFlag(string title, string description)
        : base(title, description, "Escape Flag", default,1)
    {
    }

    public override void CheckProgress()
    {
        if (flagManager.isEscapeFlagRegistered)
        {
            currentValue = 1;
            isComplete = true;
            Debug.Log($"Quest '{title}' is complete!");
            gameManager.GameClear();
        }
        else
        {
            currentValue = 0;
            isComplete = false;
            Debug.Log($"Quest '{title}' is not complete.");
        }
    }
}