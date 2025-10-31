// MusicManager.cs
// Persistent music + ambience manager with per-clip volume control.
// Uses two music sources for crossfades and a third for ambience.

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager I;

    [Header("Core Themes")]
    public AudioClip mainMenu;
    public AudioClip overworldTheme;
    public AudioClip cabinAmbience;

    [Header("Fishing Variants")]
    public AudioClip fishing12;      // days 1–2
    public AudioClip fishing35;      // days 3–5

    [Header("Special Cues")]
    public AudioClip entityReelUp;
    public AudioClip entityEmerge;

    [Header("Endings")]
    public AudioClip unknownEnding;  // obedient ending

    public AudioClip beached;

    public AudioClip knownEndingSingle;

    public AudioClip knownEnding_A;  // 17s part in overworld
    public AudioClip knownEnding_B;  // continuation in credits

    [Header("Global Settings")]
    [Range(0f, 2f)] public float musicVolume = 0.9f;     // base default for music
    [Range(0.02f, 5f)] public float defaultFade = 1.25f; // default crossfade time

    // internal sources
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

        var sources = GetComponents<AudioSource>();
        if (sources.Length < 3)
        {
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
        // Intentionally empty. Drive music from gameplay code.
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

    // Public API ---------------------------------------------------------------

    public void PlayMainMenu(float fade = -1f)
    {
        SetAmbience(null, defaultFade);
        CrossfadeTo(mainMenu, fade, true);
    }

    // inCabin: true when indoors to enable ambience
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

        // Schedules Known Ending Part A -> Part B with per-clip volumes
        public void PlayKnownEndingTwoPart(double partALengthSeconds, float preDelay = 0.05f)
        {
            if (!knownEnding_A || !knownEnding_B) return;

            if (fadeCR != null) StopCoroutine(fadeCR);
            a.Stop(); b.Stop(); amb.Stop();
            a.volume = b.volume = amb.volume = 0f;

            var srcA = a;
            var srcB = b;

            double now = AudioSettings.dspTime;
            double startA = now + Mathf.Max(0f, preDelay);
            double endA = startA + partALengthSeconds;
            double startB = endA;

            float volA = GetVolumeForClip(knownEnding_A);
            float volB = GetVolumeForClip(knownEnding_B);

            srcA.clip = knownEnding_A;
            srcA.loop = false;
            srcA.volume = volA;
            srcA.PlayScheduled(startA);
            srcA.SetScheduledEndTime(endA);

            srcB.clip = knownEnding_B;
            srcB.loop = true;
            srcB.volume = volB;
            srcB.PlayScheduled(startB);

            activeMusic = srcB;
        }

    public void PlayKnownEndingSingle(float fade = -1f, bool loop = false)
    {
        SetAmbience(null, defaultFade);
        CrossfadeTo(knownEndingSingle, fade, loop);
    }

    // Ambience uses per-clip volume too
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
        StartCoroutine(FadeAudio(amb, GetVolumeForClip(clip), fade, stopAtZero: false));
    }

    public void HardStopAll()
    {
        if (fadeCR != null) StopCoroutine(fadeCR);
        a.Stop(); b.Stop(); amb.Stop();
        a.volume = b.volume = amb.volume = 0f;
        a.clip = b.clip = amb.clip = null;
    }

    // Internals ----------------------------------------------------------------

    void CrossfadeTo(AudioClip next, float fade, bool loop)
    {
        if (!next) return;
        if (fade < 0f) fade = defaultFade;

        float targetVol = GetVolumeForClip(next);

        if (activeMusic != null && activeMusic.clip == next)
        {
            activeMusic.loop = loop;
            StartFade(activeMusic, targetVol, fade);
            return;
        }

        AudioSource from = activeMusic == a ? a : b;
        AudioSource to = activeMusic == a ? b : a;

        to.clip = next;
        to.loop = loop;
        to.volume = 0f;
        to.Play();

        if (fadeCR != null) StopCoroutine(fadeCR);
        fadeCR = StartCoroutine(CrossfadeRoutine(from, to, targetVol, fade));
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

    // Per-clip volume map (0–1)
    float GetVolumeForClip(AudioClip clip)
    {
        if (!clip) return musicVolume;

        if (clip == mainMenu) return 0.70f;
        if (clip == overworldTheme) return 0.90f;

        if (clip == cabinAmbience) return 0.20f; // set your ambience level here

        if (clip == fishing12) return 0.80f;
        if (clip == fishing35) return 1.00f;

        if (clip == entityReelUp) return 0.70f;
        if (clip == entityEmerge) return 0.50f;

        if (clip == beached) return 0.75f;
        if (clip == unknownEnding) return 0.80f;
        if (clip == knownEnding_A) return 0.85f;
        if (clip == knownEnding_B) return 0.85f;

        return musicVolume;
    }

    public void PlayEntityEmergence_CleanLoop(float fadeIn = -1f)
    {
        if (fadeIn < 0f) fadeIn = defaultFade;
        if (!entityEmerge) return;

        // pick the inactive music source for crossfade safety
        AudioSource next = (activeMusic == a) ? b : a;

        // set up the clip to loop natively (no loop-window coroutine)
        next.Stop();
        next.clip = entityEmerge;
        next.loop = true;
        next.time = 0f;
        next.volume = 0f;
        next.Play();

        // fade out the old source, fade in the new
        if (fadeCR != null) StopCoroutine(fadeCR);
        fadeCR = StartCoroutine(CrossfadeRoutine(activeMusic, next, GetVolumeForClip(entityEmerge), fadeIn));
        activeMusic = next;
    }

    public void PlayBeached(float fade = -1f, bool loop = true)
    {
        SetAmbience(null, defaultFade);
        CrossfadeTo(beached, fade, loop);
    }

}
