using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using System;

public class AudioManager : Singleton<AudioManager>
{
    [Title("Audio")]
    [SerializeField]
    private SimpleAudioEvent[] buyUnitSFXs;

    [SerializeField]
    private SimpleAudioEvent goldSFX;

    [SerializeField]
    private SimpleAudioEvent starUpgrade;

    [SerializeField]
    private SimpleAudioEvent uiDeepClick;

    [SerializeField]
    private SimpleAudioEvent whoosh;

    [SerializeField]
    private SimpleAudioEvent splash;

    [SerializeField]
    private SimpleAudioEvent crunch;

    [SerializeField]
    private SimpleAudioEvent extinguishFire;

    [Title("Settings")]
    public bool useSoundEffects = true;
    public bool useMusic = true;

    private Queue<AudioSource> audioSources = new Queue<AudioSource>();

    private AudioSource musicSource;

    private float starPitch = 1.0f;

    public SimpleAudioEvent[] BuyUnitSFXs => buyUnitSFXs;
    public SimpleAudioEvent GoldSFX => goldSFX;
    public SimpleAudioEvent UIDeepClick => uiDeepClick;
    public SimpleAudioEvent Whoosh => whoosh;
    public SimpleAudioEvent Splash => splash;
    public SimpleAudioEvent Crunch => crunch;
    public SimpleAudioEvent ExtinguishFire => extinguishFire;

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        musicSource = GetComponent<AudioSource>();

        useSoundEffects = PlayerPrefs.GetInt("SoundEffects") == 0;
        useMusic = PlayerPrefs.GetInt("Music") == 0;

        if (!useMusic)
        {
            musicSource.volume = 0;
        }
    }

    public void PlaySoundEffect(SimpleAudioEvent audio, float pitchMult = 1)
    {
        if (!useSoundEffects)
        {
            return;
        }

        if (audioSources.Count == 0)
        {
            audioSources.Enqueue(gameObject.AddComponent<AudioSource>());
        }
        var source = audioSources.Dequeue();
        int index = audio.Play(source, pitchMult);

        StartCoroutine(ReturnToQueue(source, audio.Clips[index].length));
    }

    private IEnumerator ReturnToQueue(AudioSource source, float length)
    {
        yield return new WaitForSeconds(length);

        audioSources.Enqueue(source);
    }

    public void ToggleSoundEffects(bool isOn)
    {
        useSoundEffects = isOn;
        PlayerPrefs.SetInt("SoundEffects", useSoundEffects ? 0 : -1);
    }

    public void ToggleMusic(bool isOn)
    {
        useMusic = isOn;
        PlayerPrefs.SetInt("Music", useMusic ? 0 : -1);
    }

    public async void PlayerStarSFX()
    {
        PlaySoundEffect(starUpgrade, starPitch);

        starPitch += 0.1f;
        float cachedPitch = starPitch;

        await UniTask.Delay(TimeSpan.FromSeconds(1));

        if (starPitch - cachedPitch <= Mathf.Epsilon)
        {
            starPitch = 1.0f;
        }
    }
}
