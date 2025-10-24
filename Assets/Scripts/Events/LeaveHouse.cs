using UnityEngine;

public class LeaveHouse : EventTrigger
{
    protected override void EventTriggered()
    {
        if (!gameManager.hasLantern)
        {
            gameManager.TriggerEvent("Better take your lantern, just in case.");
            return;
        }

        string message = string.Empty;

        if (gameManager.hasFishedToday)
            message = "Go outside?";
        else if (gameManager.currentDay == 1)
            message = "There's a note under the door. It reads, \"I'm a bit hungry. Would you mind leaving some fish for me?\"\n\nGo outside?";

        else if (gameManager.currentDay == 2)
            message = "There's a note under the door. It reads, \"You are forbidden from looking at the tree today. Do not defy me.\"\n\nGo outside?";

        else if (gameManager.currentDay == 3 && gameManager.obedience > 0)
            message = "There's a note under the door. It reads, \"Do not catch green fish. Do not disappoint me.\"\n\nGo outside?";
        else if (gameManager.currentDay == 3 && gameManager.obedience <= 0)
            message = "There's a note under the door. It reads, \"DO NOT COLLECT GREEN FISH. YOU HAVE BEEN WARNED.\"\n\nGo outside?";

        else if (gameManager.currentDay == 4 && gameManager.obedience > 0)
            message = "There's a note under the door. It reads, \"Look at the campfire today. Also, no red fish.\"\n\nGo outside?";
        else if (gameManager.currentDay == 4 && gameManager.obedience <= 0)
            message = "There's a note under the door. It reads, \"Insolent wretch. Look at the campfire today. No red fish. This is your final warning, human.\"\n\nGo outside?";

        else if (gameManager.currentDay == 5 && gameManager.obedience > 0)
            message = "There's a note under the door. It reads, \"You have done well. Come and collect your reward.\"\n\nThere's something out there. Go and meet it?";
        else if (gameManager.currentDay == 5 && gameManager.obedience <= 0)
            message = "There's a note under the door. It reads, \"DIEdie7^ why wjy DiE!!&%N@#(F*SDNOdepth$ *&WFS E.RRoR..\"\n\n## #######?";

        gameManager.TriggerEvent(message, "LeaveHouse");
    }
}