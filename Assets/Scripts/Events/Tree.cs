using UnityEngine;

public class Tree : EventTrigger
{
    protected override void EventTriggered()
    {
        string message = string.Empty;

        if (gameManager.currentDay == 1)
            message = "It's your favorite tree.";

        if (gameManager.currentDay == 2)
            message = "The tree doesn't look happy to see you.";

        if (gameManager.currentDay == 3)
            message = "The tree's wood is dead and rotting.";

        if (gameManager.currentDay == 4)
            message = "There are words carved in the bark...\n\n\"KNOW YOUR ENEMY\".";

        if (gameManager.currentDay == 5)
        {
            if (gameManager.obedience > 0)
                message = "It is unknown.";
            else
                message = "It will be known.";
        }

        gameManager.TriggerEvent(message);
    }
}