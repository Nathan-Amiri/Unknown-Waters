using UnityEngine;

public class MenuMusicBoot : MonoBehaviour
{
    void Start()
    {
        if (MusicManager.I == null)
        {
            var go = new GameObject("MusicManager");
            go.AddComponent<MusicManager>();
        }
        MusicManager.I?.PlayMainMenu(1.2f);
    }
}
