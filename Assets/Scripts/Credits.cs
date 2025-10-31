using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Credits : MonoBehaviour
{
    public static bool unknownEnding; // Set by GameManager

    public Fade fade;
    public TMP_Text creditsText;

    private void Start()
    {
        fade.StartFade(true);

        StartCoroutine(CreditsCoroutine());
    }

    private IEnumerator CreditsCoroutine()
    {
        creditsText.color = Color.red;
        string ending = unknownEnding ? "UNKNOWN ENDING" : "KNOWN ENDING";
        creditsText.text = "Beneath the Surface:\n\n" + ending;

        yield return new WaitForSeconds(4); // Keep in mind we're still fading in! This pause needs to be longer than the others. Idk how much longer though

        creditsText.color = Color.green;
        creditsText.text = "Dorian Kavadlo:\n\nCreative Director\nProgramming\nMusic & SFX";

        yield return new WaitForSeconds(4);

        creditsText.color = Color.yellow;
        creditsText.text = "Vannia \"navinau\" U:\n\nBackground Artist";

        yield return new WaitForSeconds(4);

        creditsText.color = Color.blue;
        creditsText.text = "Gavin Banes:\n\nCharacter Animations\nAdditional Art";

        yield return new WaitForSeconds(4);

        creditsText.color = Color.cyan;
        creditsText.text = "Taylor Poorman:\n\nAdditional Art";

        yield return new WaitForSeconds(4);

        creditsText.color = Color.magenta;
        creditsText.text = "Nathan Amiri (azeTrom):\n\nProgramming\nDesign\nGeneral Mayhem";

        yield return new WaitForSeconds(4);

        creditsText.color = Color.white;
        creditsText.text = "Thanks for playing!";

        yield return new WaitForSeconds(4);

        fade.StartFade(true);

        MusicManager.I?.HardStopAll();

        yield return new WaitForSeconds(.9f);

        SceneManager.LoadScene(0);
    }
}