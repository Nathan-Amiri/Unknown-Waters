using UnityEngine;

public class Lantern : EventTrigger
{
    protected override void EventTriggered()
    {
        string message = gameManager.hasLantern ? "Put down lantern?" : "Pick up lantern?";
        gameManager.TriggerEvent(message, "Lantern");
    }
}