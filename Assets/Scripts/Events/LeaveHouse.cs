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
            message = "There's a note under the door.[p]It reads, \"Beneath the stillness, something turns. Three <color=#FFE36E>yellow tang</color> will soothe its sleep.[p]Offer them to the altar once the light fades. Their <color=#B22222>blood</color> must touch the stone before you rest.[p]Fisherman,[n]<color=#B22222><i>Keep your lantern lit.</i></color>\"[p]Go outside?";

        else if (gameManager.currentDay == 2)
            message = "There's a note under the door.[p]It reads, \"The tide has softened. Catch only two or three... a <color=#FFE36E>Yellow Tang</color>, a <color=#5CB3FF>Little Tunny</color>, or a <color=#E4A0FF>Queen Angelfish</color>. Leave the rest beneath the waves.[p]Do not meet the eyes of the <color=#89A9FF>Lanternfish</color> tonight.\"[p]<color=#B22222><i>Be still, and be unseen.</i></color>[p]Go outside?";

        else if (gameManager.currentDay == 3 && gameManager.obedience > -5)
            message = "There's a note under the door.[p]It reads, \"The current drags slower now. Catch no more than three... a <color=#5CB3FF>Tunny</color>, a <color=#89A9FF>Lanternfish</color>, perhaps one other. But not the <color=#3A8E3A>green Tang</color>... it remembers too much.\"[p]<color=#B22222><i>Do not listen when the sea begins to hum.</i></color>[p]Go outside?";

        else if (gameManager.currentDay == 3 && gameManager.obedience <= -5)
            message = "There's a note under the door.[p]It reads, \"You caught the green one. It followed you home.[p]Catch two... any two... maybe it will stop whispering. Keep your back to the shore.\"[p]<color=#B22222><i>Something knocks beneath the floorboards.</i></color>[p]Go outside?";

        else if (gameManager.currentDay == 4 && gameManager.obedience > 0)
            message = "There's a note under the door.[p]It reads, \"Do not seek purity. The water runs red. Catch two or three of the corrupted... <color=#FFE36E>Tang</color>, <color=#5CB3FF>Tunny</color>, <color=#89A9FF>Lanternfish</color>. Their shapes are wrong but familiar.[p]Burn what’s left when you return.\"[p]<color=#B22222><i>The smoke smells like salt and teeth.</i></color>[p]Go outside?";

        else if (gameManager.currentDay == 4 && gameManager.obedience <= 0)
            message = "There's a note under the door.[p]It reads, \"You caught what was forbidden again. Only their corrupted eyes remain. Catch two if you must... each one watches as you reel them in.[p]The fire will not save you now.\"[p]<color=#B22222><i>They wait where the shoreline ends.</i></color>[p]Go outside?";

        else if (gameManager.currentDay == 5 && gameManager.obedience > 0)
            message = "There's a note under the door.[p]It reads, \"You have done well, Fisherman. The sea remembers. Catch nothing today... it will rise to meet you.\"[p]The ink ripples as though it breathes.[p]<color=#B22222><i>Go and collect your reward.</i></color>[p]Something waits outside.";

        else if (gameManager.currentDay == 5 && gameManager.obedience <= 0)
            message = "There's a note under the door.[p]It reads, \"7^..beneathbeneathbeneath..whywhywhy... two three fish no no stop STOP the light’s gone the water’s loud...\"[p]<color=#B22222><i>flesh.fish.file.corrupt</i></color>[p]<color=#FF0000>E R R O R . . .</color>[p]####### ?";


        gameManager.TriggerEvent(message, "LeaveHouse");
    }
}