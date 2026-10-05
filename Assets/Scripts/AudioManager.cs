// AudioManager
// Singleton that carries music and sound effects between scenes (DontDestroyOnLoad).
// Music: per-scene SceneMusic tracks, or the built-in level rotation, with boss music
// overriding during boss fights. SFX: a voice pool with per-sound volume, random clip
// variation, anti-spam limits, and optional max duration with a quick fade out.
// Every button and toggle in each scene is hooked up to a UI click automatically.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public enum Sfx
    {
        UIClick, PlayerLaser, EnemyLaser, Torpedo, ProjectileHit, AsteroidExplode, ShipExplode,
        StardustPickup, CratePickup, ShieldPickup, RepairPickup, ShieldHit, HullHit, HullHitLaser, Portal
    }

    [System.Serializable]
    public class SfxEntry
    {
        public Sfx id;
        public AudioClip[] clips;
        [Range(0f, 1f)] public float volume = 1f;
        public float pitchVariance = 0.05f;
        public float minInterval = 0.05f;
        public int maxSimultaneous = 3;
        public float maxDuration = 0f;
        public float fadeOutTime = 0.1f;
    }

    public static AudioManager Instance;

    public SfxEntry[] sfx;
    public AudioClip[] levelMusic;
    public bool shuffleLevelMusic = true;
    public AudioClip bossMusic;
    public float musicFadeTime = 1f;
    public int sfxVoices = 16;

    private AudioSource musicSource;
    private AudioSource uiSource;
    private AudioSource[] voices;
    private Sfx[] voiceIds;
    private float[] voiceStart;
    private int[] voiceTokens;
    private float[] voiceBaseVolume;
    private float[] voiceFadeStart;
    private float[] voiceFadeTime;
    private int tokenCounter;
    private readonly Dictionary<Sfx, SfxEntry> lookup = new Dictionary<Sfx, SfxEntry>();
    private readonly Dictionary<Sfx, float> lastPlayed = new Dictionary<Sfx, float>();
    private AudioClip[] sceneTracks;
    private bool shuffle;
    private int trackIndex = -1;
    private bool bossPlaying;
    private float fadeVolume = 1f;
    private Coroutine fadeRoutine;

    // First instance keeps itself alive across scenes; later copies (from the scene's
    // prefab copy) remove themselves. Sets up music/UI/voice sources and the sound lookup.
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.ignoreListenerPause = true;
        uiSource = gameObject.AddComponent<AudioSource>();
        uiSource.playOnAwake = false;
        uiSource.ignoreListenerPause = true;
        voices = new AudioSource[Mathf.Max(1, sfxVoices)];
        voiceIds = new Sfx[voices.Length];
        voiceStart = new float[voices.Length];
        voiceTokens = new int[voices.Length];
        voiceBaseVolume = new float[voices.Length];
        voiceFadeStart = new float[voices.Length];
        voiceFadeTime = new float[voices.Length];
        for (int i = 0; i < voices.Length; i++)
        {
            voices[i] = gameObject.AddComponent<AudioSource>();
            voices[i].playOnAwake = false;
        }
        foreach (SfxEntry e in sfx) lookup[e.id] = e;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance != this) return;
        Instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Per scene: wires click sounds onto every button/toggle, then decides whether to
    // start this scene's music. The level rotation continues across level changes and
    // restarts unless boss music is playing or the tracks actually change.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (Button b in FindObjectsOfType<Button>(true)) b.onClick.AddListener(() => PlayUI(Sfx.UIClick));
        foreach (Toggle t in FindObjectsOfType<Toggle>(true)) t.onValueChanged.AddListener(v => PlayUI(Sfx.UIClick));

        SceneMusic music = FindObjectOfType<SceneMusic>();
        AudioClip[] tracks = music != null ? music.tracks : levelMusic;
        bool shuffleTracks = music != null ? music.shuffle : shuffleLevelMusic;
        bool sameTracks = tracks != null && sceneTracks != null && tracks.Length > 0 && sceneTracks.Length > 0 && tracks[0] == sceneTracks[0];
        if (bossPlaying || !sameTracks)
        {
            bossPlaying = false;
            sceneTracks = tracks;
            shuffle = shuffleTracks;
            trackIndex = -1;
            if (sceneTracks == null || sceneTracks.Length == 0) SwitchMusic(null, false);
            else NextTrack();
        }
    }

    // Applies the music volume slider every frame, auto-advances the level rotation
    // when a track ends, and processes any sound fades.
    void Update()
    {
        musicSource.volume = GameSettings.MusicVolume * fadeVolume;
        if (!bossPlaying && fadeRoutine == null && !musicSource.isPlaying && sceneTracks != null && sceneTracks.Length > 0) NextTrack();
        UpdateVoiceFades();
    }

    // Applies running fades to voices that hit their max duration.
    void UpdateVoiceFades()
    {
        float now = Time.unscaledTime;
        for (int i = 0; i < voices.Length; i++)
        {
            if (!voices[i].isPlaying || now < voiceFadeStart[i]) continue;
            float k = voiceFadeTime[i] > 0f ? 1f - (now - voiceFadeStart[i]) / voiceFadeTime[i] : 0f;
            if (k <= 0f)
            {
                voices[i].Stop();
                voiceFadeStart[i] = float.MaxValue;
            }
            else voices[i].volume = voiceBaseVolume[i] * k;
        }
    }

    // Fades a playing sound out over 'time' seconds. Handle comes from Play().
    public static void FadeOut(int handle, float time = 0.1f)
    {
        if (Instance == null || handle <= 0) return;
        for (int i = 0; i < Instance.voices.Length; i++)
        {
            if (Instance.voiceTokens[i] != handle || !Instance.voices[i].isPlaying) continue;
            if (Time.unscaledTime < Instance.voiceFadeStart[i])
            {
                Instance.voiceFadeStart[i] = Time.unscaledTime;
                Instance.voiceFadeTime[i] = time;
            }
            return;
        }
    }

    // Picks the next rotation track: random but never the same track twice in a row.
    void NextTrack()
    {
        if (shuffle && trackIndex < 0) trackIndex = Random.Range(0, sceneTracks.Length);
        else if (shuffle && sceneTracks.Length > 1)
        {
            int pick = Random.Range(0, sceneTracks.Length - 1);
            trackIndex = pick >= trackIndex ? pick + 1 : pick;
        }
        else trackIndex = (trackIndex + 1) % sceneTracks.Length;
        SwitchMusic(sceneTracks[trackIndex], false);
    }

    void SwitchMusic(AudioClip clip, bool loop)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeTo(clip, loop));
    }

    // Fades the current music out, swaps the clip, and fades back in.
    IEnumerator FadeTo(AudioClip clip, bool loop)
    {
        if (musicSource.isPlaying)
        {
            for (float t = 0f; t < musicFadeTime; t += Time.unscaledDeltaTime)
            {
                fadeVolume = 1f - t / musicFadeTime;
                yield return null;
            }
        }
        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = loop;
        if (clip != null) musicSource.Play();
        for (float t = 0f; t < musicFadeTime; t += Time.unscaledDeltaTime)
        {
            fadeVolume = t / musicFadeTime;
            yield return null;
        }
        fadeVolume = 1f;
        fadeRoutine = null;
    }

    // Boss music hooks: the fight starts it looping, and the boss's death returns
    // to the level rotation.
    public static void PlayBossMusic()
    {
        if (Instance == null || Instance.bossMusic == null) return;
        Instance.bossPlaying = true;
        Instance.SwitchMusic(Instance.bossMusic, true);
    }

    public static void StopBossMusic()
    {
        if (Instance == null || !Instance.bossPlaying) return;
        Instance.bossPlaying = false;
        if (Instance.sceneTracks != null && Instance.sceneTracks.Length > 0) Instance.NextTrack();
        else Instance.SwitchMusic(null, false);
    }

    public static int Play(Sfx id)
    {
        return Instance != null ? Instance.PlaySfx(id, false) : -1;
    }

    public static void PlayUI(Sfx id)
    {
        if (Instance != null) Instance.PlaySfx(id, true);
    }

    // Chooses a free voice, or the oldest of this sound's plays if the per-sound
    // simultaneous cap is hit, or the oldest voice overall as a last resort. Keeps
    // spamming input from stacking sounds or cutting newer ones off.
    int PickVoice(Sfx id, int maxSame)
    {
        int sameCount = 0;
        int oldestSame = -1;
        int oldestAny = 0;
        int free = -1;
        for (int i = 0; i < voices.Length; i++)
        {
            if (!voices[i].isPlaying)
            {
                if (free < 0) free = i;
                continue;
            }
            if (voiceStart[i] < voiceStart[oldestAny]) oldestAny = i;
            if (voiceIds[i] != id) continue;
            sameCount++;
            if (oldestSame < 0 || voiceStart[i] < voiceStart[oldestSame]) oldestSame = i;
        }
        if (sameCount >= maxSame) return oldestSame;
        return free >= 0 ? free : oldestAny;
    }

    // Plays a sound if it passes the anti-spam interval. Returns a handle for FadeOut,
    // or -1 when nothing played. UI sounds go through a dedicated always-audible source.
    int PlaySfx(Sfx id, bool ui)
    {
        SfxEntry e;
        if (!lookup.TryGetValue(id, out e) || e.clips == null || e.clips.Length == 0) return -1;
        float last;
        if (lastPlayed.TryGetValue(id, out last) && Time.unscaledTime - last < e.minInterval) return -1;
        lastPlayed[id] = Time.unscaledTime;

        AudioClip clip = e.clips[Random.Range(0, e.clips.Length)];
        if (clip == null) return -1;
        float volume = e.volume * GameSettings.SfxVolume;
        if (ui)
        {
            uiSource.PlayOneShot(clip, volume);
            return -1;
        }
        int index = PickVoice(id, Mathf.Max(1, e.maxSimultaneous));
        voiceIds[index] = id;
        voiceStart[index] = Time.unscaledTime;
        voiceTokens[index] = ++tokenCounter;
        voiceBaseVolume[index] = volume;
        voiceFadeStart[index] = e.maxDuration > 0f ? Time.unscaledTime + e.maxDuration : float.MaxValue;
        voiceFadeTime[index] = e.fadeOutTime;
        AudioSource src = voices[index];
        src.Stop();
        src.clip = clip;
        src.volume = volume;
        src.pitch = 1f + Random.Range(-e.pitchVariance, e.pitchVariance);
        src.Play();
        return voiceTokens[index];
    }
}
