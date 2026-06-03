using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private const string MusicVolumeKey = "audio_music_volume";
    private const string SfxVolumeKey = "audio_sfx_volume";
    private const string MusicPath = "Music/";
    private const float MusicGain = 1.15f;
    private const float SfxGain = 1.25f;

    private static AudioManager instance;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioClip menuMusic;
    private AudioClip fightMusic;
    private AudioClip clickClip;
    private AudioClip boostClip;
    private AudioClip jumpClip;
    private AudioClip lightAttackClip;
    private AudioClip heavyAttackClip;
    private AudioClip hitClip;
    private AudioClip overheatClip;
    private AudioClip roundWinClip;
    private AudioClip matchWinClip;

    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(MusicVolumeKey, 0.8f);
        set
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(value));
            PlayerPrefs.Save();
            if (instance != null)
                instance.ApplyVolumes();
        }
    }

    public static float SfxVolume
    {
        get => PlayerPrefs.GetFloat(SfxVolumeKey, 0.8f);
        set
        {
            PlayerPrefs.SetFloat(SfxVolumeKey, Mathf.Clamp01(value));
            PlayerPrefs.Save();
            if (instance != null)
                instance.ApplyVolumes();
        }
    }

    public static AudioManager Ensure()
    {
        if (instance != null)
            return instance;

        GameObject audioObject = new GameObject("AudioManager");
        AudioManager manager = audioObject.AddComponent<AudioManager>();
        DontDestroyOnLoad(audioObject);
        return manager;
    }

    public static void PlayMenuMusic()
    {
        Ensure().PlayMusic(Ensure().menuMusic);
    }

    public static void PlayFightMusic()
    {
        Ensure().PlayMusic(Ensure().fightMusic);
    }

    public static void PauseMusic()
    {
        if (instance != null && instance.musicSource.isPlaying)
            instance.musicSource.Pause();
    }

    public static void ResumeMusic()
    {
        if (instance != null && instance.musicSource.clip != null && !instance.musicSource.isPlaying)
            instance.musicSource.UnPause();
    }

    public static void StopMusic()
    {
        if (instance != null)
            instance.musicSource.Stop();
    }

    public static void PlayClick()
    {
        Ensure().PlaySfx(Ensure().clickClip);
    }

    public static void PlayBoost()
    {
        Ensure().PlaySfx(Ensure().boostClip);
    }

    public static void PlayJump()
    {
        Ensure().PlaySfx(Ensure().jumpClip, 0.25f);
    }

    public static void PlayLightAttack()
    {
        Ensure().PlaySfx(Ensure().lightAttackClip);
    }

    public static void PlayHeavyAttack()
    {
        Ensure().PlaySfx(Ensure().heavyAttackClip);
    }

    public static void PlayHit()
    {
        Ensure().PlaySfx(Ensure().hitClip != null ? Ensure().hitClip : Ensure().lightAttackClip, 0.72f);
    }

    public static void PlayOverheat()
    {
        Ensure().PlaySfx(Ensure().overheatClip != null ? Ensure().overheatClip : Ensure().boostClip, 0.95f);
    }

    public static void PlayOverheatWarning()
    {
        Ensure().PlaySfx(Ensure().overheatClip != null ? Ensure().overheatClip : Ensure().boostClip, 0.45f);
    }

    public static void PlayRoundWin()
    {
        Ensure().PlaySfx(Ensure().roundWinClip != null ? Ensure().roundWinClip : Ensure().boostClip, 0.88f);
    }

    public static void PlayMatchWin()
    {
        Ensure().PlaySfx(Ensure().matchWinClip != null ? Ensure().matchWinClip : Ensure().boostClip, 1f);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;

        LoadClips();
        ApplyVolumes();
    }

    private void LoadClips()
    {
        menuMusic = Resources.Load<AudioClip>(MusicPath + "Menu");
        fightMusic = Resources.Load<AudioClip>(MusicPath + "fight");
        clickClip = Resources.Load<AudioClip>(MusicPath + "click2");
        boostClip = Resources.Load<AudioClip>(MusicPath + "boost");
        jumpClip = Resources.Load<AudioClip>(MusicPath + "SFX_Jump_05");
        lightAttackClip = Resources.Load<AudioClip>(MusicPath + "Socapex - Swordsmall_2");
        heavyAttackClip = Resources.Load<AudioClip>(MusicPath + "Socapex - Swordsmall_3");
        hitClip = Resources.Load<AudioClip>(MusicPath + "hit");
        overheatClip = Resources.Load<AudioClip>(MusicPath + "overheat");
        roundWinClip = Resources.Load<AudioClip>(MusicPath + "round_win");
        matchWinClip = Resources.Load<AudioClip>(MusicPath + "match_win");
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: music clip is missing.");
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = GetEffectiveMusicVolume();
        musicSource.Play();
    }

    private void PlaySfx(AudioClip clip)
    {
        PlaySfx(clip, 1f);
    }

    private void PlaySfx(AudioClip clip, float volumeScale)
    {
        if (clip == null)
            return;

        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
    }

    private void ApplyVolumes()
    {
        if (musicSource != null)
            musicSource.volume = GetEffectiveMusicVolume();

        if (sfxSource != null)
            sfxSource.volume = GetEffectiveSfxVolume();
    }

    private float GetEffectiveMusicVolume()
    {
        float value = Mathf.Clamp01(MusicVolume);
        return Mathf.Clamp01(value * value * MusicGain);
    }

    private float GetEffectiveSfxVolume()
    {
        float value = Mathf.Clamp01(SfxVolume);
        return Mathf.Clamp01(value * value * SfxGain);
    }
}
