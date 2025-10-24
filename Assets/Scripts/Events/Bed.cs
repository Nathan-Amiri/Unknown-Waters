using UnityEngine;

public class Bed : EventTrigger
{
    protected override void EventTriggered()
    {
        if (!gameManager.hasFishedToday)
        {
            gameManager.TriggerEvent("You aren't sleepy yet.");
            return;
        }
        if (gameManager.hasLantern)
        {
            gameManager.TriggerEvent("Better put your lantern down first.");
            return;
        }

        gameManager.TriggerEvent("Go to sleep?", "Bed");
    }
}