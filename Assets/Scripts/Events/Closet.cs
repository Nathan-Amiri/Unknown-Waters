using UnityEngine;

public class Closet : EventTrigger
{
    protected override void EventTriggered()
    {
        string message = string.Empty;

        if (gameManager.currentDay == 1)
            message = "You are 96% sure there are no skeletons in your closet.";

        if (gameManager.currentDay == 2)
            message = "You are 86% sure there are no skeletons in your closet.";

        if (gameManager.currentDay == 3)
            message = "You are 54% sure there are no skeletons in your closet.";

        if (gameManager.currentDay == 4)
            message = "You're 0% sure there are no skeletons in your closet.";

        if (gameManager.currentDay == 5)
            message = "You...you don't have a closet. You never had a closet.";

        gameManager.TriggerEvent(message);
    }
}