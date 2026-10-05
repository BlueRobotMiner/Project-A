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
    private readonly Dictionary<Sfx, SfxEntry> lookup = new Dictionary<Sfx, SfxEntry>();
    private readonly Dictionary<Sfx, float> lastPlayed = new Dictionary<Sfx, float>();
    private AudioClip[] sceneTracks;
    private bool shuffle;
    private int trackIndex = -1;
    private bool bossPlaying;
    private float fadeVolume = 1f;
    private Coroutine fadeRoutine;

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

    void Update()
    {
        musicSource.volume = GameSettings.MusicVolume * fadeVolume;
        if (!bossPlaying && fadeRoutine == null && !musicSource.isPlaying && sceneTracks != null && sceneTracks.Length > 0) NextTrack();
    }

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

    public static void Play(Sfx id)
    {
        if (Instance != null) Instance.PlaySfx(id, false);
    }

    public static void PlayUI(Sfx id)
    {
        if (Instance != null) Instance.PlaySfx(id, true);
    }

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

    void PlaySfx(Sfx id, bool ui)
    {
        SfxEntry e;
        if (!lookup.TryGetValue(id, out e) || e.clips == null || e.clips.Length == 0) return;
        float last;
        if (lastPlayed.TryGetValue(id, out last) && Time.unscaledTime - last < e.minInterval) return;
        lastPlayed[id] = Time.unscaledTime;

        AudioClip clip = e.clips[Random.Range(0, e.clips.Length)];
        if (clip == null) return;
        float volume = e.volume * GameSettings.SfxVolume;
        if (ui)
        {
            uiSource.PlayOneShot(clip, volume);
            return;
        }
        int index = PickVoice(id, Mathf.Max(1, e.maxSimultaneous));
        voiceIds[index] = id;
        voiceStart[index] = Time.unscaledTime;
        AudioSource src = voices[index];
        src.Stop();
        src.clip = clip;
        src.volume = volume;
        src.pitch = 1f + Random.Range(-e.pitchVariance, e.pitchVariance);
        src.Play();
    }
}
