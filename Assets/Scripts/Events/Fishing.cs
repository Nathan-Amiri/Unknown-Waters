using UnityEngine;

public class Fishing : EventTrigger
{
    protected override void EventTriggered()
    {
        gameManager.TriggerEvent("Would you like to start fishing?", "Fishing");
    }
}