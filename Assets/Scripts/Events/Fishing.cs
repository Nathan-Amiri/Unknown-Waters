using UnityEngine;

public class Fishing : EventTrigger
{
    protected override void EventTriggered()
    {
        if (gameManager.hasFishedToday)
        {
            gameManager.TriggerEvent("You already fished today.");
            return;
        }

        gameManager.TriggerEvent("Would you like to start fishing?", "Fishing");
    }
}