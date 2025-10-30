// MusicManager.cs
// Drop on a GameObject named "MusicManager". It will DontDestroyOnLoad.
// Add no components manually if you don’t want to — it will create 3 AudioSources on Awake:
//   Music A, Music B (for crossfading), and Ambience (for cabin loop).

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager I;

    [Header("Core Themes")]
    public AudioClip mainMenu;
    public AudioClip overworldTheme;
    public AudioClip cabinAmbience;   // looping ambience used when inside the cabin

    [Header("Fishing Variants")]
    public AudioClip fishing12;       // days 1–2
    public AudioClip fishing35;       // days 3–5

    [Header("Special Cues")]
    public AudioClip entityReelUp;    // when reeling the entity at the very end
    public AudioClip entityEmerge;    // when the entity surfaces in overworld

    [Header("Endings")]
    public AudioClip unknownEnding;   // obedient ending (can be non-looping)
    public AudioClip knownEnding_A;   // 17s piece that plays in overworld before cut
    public AudioClip knownEnding_B;   // continues in credits scene (can loop)

    [Header("Mixer/Volumes/Fades")]
    [Range(0f, 2f)] public float musicVolume = 0.9f;
    [Range(0f, 2f)] public float ambienceVolume = 0.6f;
    [Range(0.02f, 5f)] public float defaultFade = 1.25f;

    // internal audio sources
    AudioSource a;   // music A
    AudioSource b;   // music B
    AudioSource amb; // ambience
    AudioSource activeMusic;

    Coroutine fadeCR;

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        // find or add three AudioSources on this same GameObject
        var sources = GetComponents<AudioSource>();
        if (sources.Length < 3)
        {
            // clear any existing for a clean setup
            foreach (var s in sources) Destroy(s);

            a = gameObject.AddComponent<AudioSource>();
            b = gameObject.AddComponent<AudioSource>();
            amb = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            a = sources[0];
            b = sources[1];
            amb = sources[2];
        }

        NameSource(a, "Music A");
        NameSource(b, "Music B");
        NameSource(amb, "Ambience");

        InitSource(a, loop: true, vol: 0f);
        InitSource(b, loop: true, vol: 0f);
        InitSource(amb, loop: true, vol: 0f);

        activeMusic = a;

        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void OnDestroy()
    {
        if (I == this)
            SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    void OnSceneChanged(Scene oldS, Scene newS)
    {
        // no automatic switching — you’ll drive it from your gameplay code.
        // left here in case you want scene-name based defaults later.
    }

    static void NameSource(AudioSource s, string label)
    {
#if UNITY_EDITOR
        s.hideFlags = HideFlags.DontSave;
        s.name = label;
#endif
    }

    static void InitSource(AudioSource s, bool loop, float vol)
    {
        s.playOnAwake = false;
        s.loop = loop;
        s.spatialBlend = 0f;
        s.volume = vol;
        s.pitch = 1f;
    }

    // public API ---------------------------------------------------------------

    public void PlayMainMenu(float fade = -1f)
    {
        SetAmbience(null, defaultFade);
        CrossfadeTo(mainMenu, fade, true);
    }

    // overworld: pass inCabin = true when you are actually inside the cabin
    public void PlayOverworld(bool inCabin, float fade = -1f)
    {
        CrossfadeTo(overworldTheme, fade, true);
        SetAmbience(inCabin ? cabinAmbience : null, defaultFade);
    }

    public void PlayFishingForDay(int day, float fade = -1f)
    {
        var clip = (day <= 2) ? fishing12 : fishing35;
        SetAmbience(null, defaultFade);
        CrossfadeTo(clip, fade, true);
    }

    // short stingers/cues — these replace the current music with a cue
    public void PlayEntityReelUp(float fade = -1f)
    {
        SetAmbience(null, defaultFade);
        CrossfadeTo(entityReelUp, fade, true);
    }

    public void PlayEntityEmergence(float fade = -1f)
    {
        SetAmbience(null, defaultFade);
        CrossfadeTo(entityEmerge, fade, true);
    }

    public void PlayUnknownEnding(float fade = -1f, bool loop = false)
    {
        SetAmbience(null, defaultFade);
        CrossfadeTo(unknownEnding, fade, loop);
    }

    // schedules a seamless A -> B handoff that survives scene loads
    // partALengthSeconds should match the exact rendered length of knownEnding_A (eg 17.0)
    public void PlayKnownEndingTwoPart(double partALengthSeconds, float targetVol = -1f, double preDelay = 0.05)
    {
        if (!knownEnding_A || !knownEnding_B) return;
        if (targetVol < 0f) targetVol = musicVolume;

        // stop any active fades and reset
        if (fadeCR != null) StopCoroutine(fadeCR);
        a.Stop(); b.Stop(); amb.Stop();
        a.volume = b.volume = amb.volume = 0f;

        var srcA = a;
        var srcB = b;

        double now = AudioSettings.dspTime;
        double startA = now + Mathf.Max(0.0f, (float)preDelay);
        double endA = startA + partALengthSeconds;
        double startB = endA; // butt-join — no gap

        // schedule A (non-loop)
        srcA.clip = knownEnding_A;
        srcA.loop = false;
        srcA.volume = targetVol;  // set gain before scheduling
        srcA.PlayScheduled(startA);
        srcA.SetScheduledEndTime(endA);

        // schedule B (looping in credits)
        srcB.clip = knownEnding_B;
        srcB.loop = true;
        srcB.volume = targetVol;
        srcB.PlayScheduled(startB);

        activeMusic = srcB;
    }

    // fades ambience on/off (null turns it off)
    public void SetAmbience(AudioClip clip, float fade = -1f)
    {
        if (fade < 0f) fade = defaultFade;

        if (clip == null)
        {
            StartCoroutine(FadeAudio(amb, 0f, fade, stopAtZero: true));
            return;
        }

        if (amb.clip != clip) amb.clip = clip;
        if (!amb.isPlaying) amb.Play();
        StartCoroutine(FadeAudio(amb, ambienceVolume, fade, stopAtZero: false));
    }

    public void HardStopAll()
    {
        if (fadeCR != null) StopCoroutine(fadeCR);
        a.Stop(); b.Stop(); amb.Stop();
        a.volume = b.volume = amb.volume = 0f;
        a.clip = b.clip = amb.clip = null;
    }

    // helpers -----------------------------------------------------------------

    void CrossfadeTo(AudioClip next, float fade, bool loop)
    {
        if (!next) return;
        if (fade < 0f) fade = defaultFade;

        // if the active source already has this clip, just ensure volumes/loop are correct
        if (activeMusic != null && activeMusic.clip == next)
        {
            activeMusic.loop = loop;
            StartFade(activeMusic, musicVolume, fade);
            return;
        }

        AudioSource from = activeMusic == a ? a : b;
        AudioSource to = activeMusic == a ? b : a;

        to.clip = next;
        to.loop = loop;
        to.volume = 0f;
        to.Play();

        if (fadeCR != null) StopCoroutine(fadeCR);
        fadeCR = StartCoroutine(CrossfadeRoutine(from, to, musicVolume, fade));
        activeMusic = to;
    }

    void StartFade(AudioSource src, float target, float duration)
    {
        if (fadeCR != null) StopCoroutine(fadeCR);
        fadeCR = StartCoroutine(FadeAudio(src, target, duration, stopAtZero: false));
    }

    IEnumerator CrossfadeRoutine(AudioSource from, AudioSource to, float targetTo, float duration)
    {
        float t = 0f;
        float startFrom = from ? from.volume : 0f;
        float startTo = to.volume;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = t / duration;

            if (from && from.clip) from.volume = Mathf.Lerp(startFrom, 0f, k);
            to.volume = Mathf.Lerp(startTo, targetTo, k);

            yield return null;
        }

        if (from && from.clip)
        {
            from.Stop();
            from.volume = 0f;
        }
        to.volume = targetTo;
    }

    IEnumerator FadeAudio(AudioSource src, float target, float duration, bool stopAtZero)
    {
        if (!src) yield break;

        float t = 0f;
        float start = src.volume;
        if (target > 0f && !src.isPlaying) src.Play();

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }

        src.volume = target;
        if (stopAtZero && Mathf.Approximately(target, 0f))
            src.Stop();
    }
}
