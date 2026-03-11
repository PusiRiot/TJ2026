using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer (opcional)")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string musicVolumeParameter = "MusicVolume";
    [SerializeField] private string sfxVolumeParameter = "SFXVolume";

    [Header("Audi" +
        "o Sources")]
    [SerializeField] private AudioSource musicSourceA;
    [SerializeField] private AudioSource musicSourceB;
    [SerializeField] private AudioSource sfx2DSourcePrefab;
    [SerializeField] private int initialPoolSize = 10;

    [Header("Default Volumes")]
    [Range(0f, 1f)][SerializeField] private float musicVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;

    [Header("Fade Settings")]
    [SerializeField] private float defaultMusicFadeTime = 1f;

    private AudioSource activeMusicSource;
    private AudioSource inactiveMusicSource;

    private readonly List<AudioSource> sfxPool = new List<AudioSource>();
    private Coroutine musicFadeCoroutine;

    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupMusicSources();
        BuildSFXPool();
        LoadSavedVolumes();
        ApplyVolumes();
    }

    private void SetupMusicSources()
    {
        if (musicSourceA == null)
        {
            GameObject goA = new GameObject("MusicSource_A");
            goA.transform.SetParent(transform);
            musicSourceA = goA.AddComponent<AudioSource>();
        }

        if (musicSourceB == null)
        {
            GameObject goB = new GameObject("MusicSource_B");
            goB.transform.SetParent(transform);
            musicSourceB = goB.AddComponent<AudioSource>();
        }

        ConfigureMusicSource(musicSourceA);
        ConfigureMusicSource(musicSourceB);

        activeMusicSource = musicSourceA;
        inactiveMusicSource = musicSourceB;
    }

    private void ConfigureMusicSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f; // 2D
        source.volume = 0f;
    }

    private void BuildSFXPool()
    {
        if (sfx2DSourcePrefab == null)
        {
            GameObject prefabGO = new GameObject("SFX2DSource_Prefab");
            sfx2DSourcePrefab = prefabGO.AddComponent<AudioSource>();
            sfx2DSourcePrefab.playOnAwake = false;
            sfx2DSourcePrefab.loop = false;
            sfx2DSourcePrefab.spatialBlend = 0f; // no diegético = 2D
            prefabGO.SetActive(false);
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateSFXSource();
        }
    }

    private AudioSource CreateSFXSource()
    {
        AudioSource source;

        if (sfx2DSourcePrefab.gameObject.scene.IsValid())
        {
            source = Instantiate(sfx2DSourcePrefab, transform);
        }
        else
        {
            GameObject go = new GameObject($"SFX2DSource_{sfxPool.Count}");
            go.transform.SetParent(transform);
            source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
        }

        source.gameObject.SetActive(true);
        sfxPool.Add(source);
        return source;
    }

    private AudioSource GetAvailableSFXSource()
    {
        for (int i = 0; i < sfxPool.Count; i++)
        {
            if (!sfxPool[i].isPlaying)
                return sfxPool[i];
        }

        return CreateSFXSource();
    }

    #region Music

    public void PlayMusic(AudioClip clip, bool loop = true, float fadeTime = -1f)
    {
        if (clip == null) return;

        if (fadeTime < 0f)
            fadeTime = defaultMusicFadeTime;

        if (activeMusicSource.clip == clip && activeMusicSource.isPlaying)
            return;

        if (musicFadeCoroutine != null)
            StopCoroutine(musicFadeCoroutine);

        inactiveMusicSource.clip = clip;
        inactiveMusicSource.loop = loop;
        inactiveMusicSource.volume = 0f;
        inactiveMusicSource.Play();

        musicFadeCoroutine = StartCoroutine(CrossFadeMusic(fadeTime));
    }

    public void StopMusic(float fadeTime = -1f)
    {
        if (fadeTime < 0f)
            fadeTime = defaultMusicFadeTime;

        if (musicFadeCoroutine != null)
            StopCoroutine(musicFadeCoroutine);

        musicFadeCoroutine = StartCoroutine(FadeOutAndStop(activeMusicSource, fadeTime));
    }

    public void PauseMusic()
    {
        activeMusicSource.Pause();
        inactiveMusicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (activeMusicSource.clip != null)
            activeMusicSource.UnPause();

        if (inactiveMusicSource.clip != null && inactiveMusicSource.time > 0f)
            inactiveMusicSource.UnPause();
    }

    private IEnumerator CrossFadeMusic(float duration)
    {
        AudioSource from = activeMusicSource;
        AudioSource to = inactiveMusicSource;

        float time = 0f;
        float fromStart = from.volume;
        float toTarget = musicVolume;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = duration <= 0f ? 1f : time / duration;

            from.volume = Mathf.Lerp(fromStart, 0f, t);
            to.volume = Mathf.Lerp(0f, toTarget, t);

            yield return null;
        }

        from.volume = 0f;
        from.Stop();

        to.volume = toTarget;

        SwapMusicSources();
        musicFadeCoroutine = null;
    }

    private IEnumerator FadeOutAndStop(AudioSource source, float duration)
    {
        float time = 0f;
        float startVolume = source.volume;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = duration <= 0f ? 1f : time / duration;
            source.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        source.volume = 0f;
        source.Stop();
        source.clip = null;

        musicFadeCoroutine = null;
    }

    private void SwapMusicSources()
    {
        AudioSource temp = activeMusicSource;
        activeMusicSource = inactiveMusicSource;
        inactiveMusicSource = temp;
    }

    #endregion

    #region NonDiegetic SFX

    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        AudioSource source = GetAvailableSFXSource();
        source.clip = clip;
        source.volume = Mathf.Clamp01(sfxVolume * volumeMultiplier);
        source.pitch = pitch;
        source.loop = false;
        source.spatialBlend = 0f;
        source.Play();
    }

    public void PlaySFXRandomPitch(AudioClip clip, float volumeMultiplier = 1f, float minPitch = 0.95f, float maxPitch = 1.05f)
    {
        if (clip == null) return;
        float pitch = Random.Range(minPitch, maxPitch);
        PlaySFX(clip, volumeMultiplier, pitch);
    }

    public void PlaySFXOneShot(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null) return;

        AudioSource source = GetAvailableSFXSource();
        source.pitch = 1f;
        source.spatialBlend = 0f;
        source.volume = Mathf.Clamp01(sfxVolume * volumeMultiplier);
        source.PlayOneShot(clip);
    }

    #endregion

    #region Volume Control

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        ApplyMusicVolume();
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        ApplySFXVolume();
        PlayerPrefs.SetFloat(SFXVolumeKey, sfxVolume);
        PlayerPrefs.Save();
    }

    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;

    private void LoadSavedVolumes()
    {
        musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, musicVolume);
        sfxVolume = PlayerPrefs.GetFloat(SFXVolumeKey, sfxVolume);
    }

    private void ApplyVolumes()
    {
        ApplyMusicVolume();
        ApplySFXVolume();
    }

    private void ApplyMusicVolume()
    {
        if (audioMixer != null && !string.IsNullOrWhiteSpace(musicVolumeParameter))
        {
            audioMixer.SetFloat(musicVolumeParameter, LinearToDb(musicVolume));
        }
        else
        {
            if (activeMusicSource != null && activeMusicSource.isPlaying)
                activeMusicSource.volume = musicVolume;
        }
    }

    private void ApplySFXVolume()
    {
        if (audioMixer != null && !string.IsNullOrWhiteSpace(sfxVolumeParameter))
        {
            audioMixer.SetFloat(sfxVolumeParameter, LinearToDb(sfxVolume));
        }
    }

    private float LinearToDb(float value)
    {
        if (value <= 0.0001f) return -80f;
        return Mathf.Log10(value) * 20f;
    }

    #endregion
}