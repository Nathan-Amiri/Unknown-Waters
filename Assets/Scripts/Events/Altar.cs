using UnityEngine;

public class Altar : EventTrigger
{
    protected override void EventTriggered()
    {
        if (!gameManager.hasFish)
            gameManager.TriggerEvent("You have nothing to place here");
        else
            gameManager.TriggerEvent("Place fish on the stump?", "Altar");
    }
}