using UnityEngine;

public class Path : EventTrigger
{
    protected override void EventTriggered()
    {
        string message = string.Empty;

        if (gameManager.currentDay == 1)
            message = "You gaze down the path. You should take a walk sometime.";

        if (gameManager.currentDay == 2)
            message = "You sense something wonderful at the end of the path. Maybe tomorrow you'll explore.";

        if (gameManager.currentDay == 3)
            message = "Freedom lies down the path. But there's so much to do here. You can't go yet.";

        if (gameManager.currentDay == 4)
            message = "You gaze longily down the path. Your legs won't move. Escape is so close.";

        if (gameManager.currentDay == 5)
            message = "The path is a dead end.";

        gameManager.TriggerEvent(message);
    }
}