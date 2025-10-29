using UnityEngine;

public class EnterHouse : EventTrigger
{
    protected override void EventTriggered()
    {
        if (!gameManager.hasFishedToday)
        {
            gameManager.TriggerEvent("You don't feel sleepy yet.");
            return;
        }
        if (gameManager.hasFish)
        {
            gameManager.TriggerEvent("It hungers for the fish you carry. You must feed it before you can enter.");
            return;
        }

        string message = string.Empty;

        if (gameManager.currentDay == 1)
            message = "You feel tired.\n\nGo inside?";

        else if (gameManager.currentDay == 2)
            message = "You feel weary.\n\nGo inside?";

        else if (gameManager.currentDay == 3)
            message = "You feel nothing.\n\nGo inside?";

        else if (gameManager.currentDay == 4)
            message = "You feel DELICIOUS.\n\nGo inside?";

        gameManager.TriggerEvent(message, "EnterHouse");
    }
}