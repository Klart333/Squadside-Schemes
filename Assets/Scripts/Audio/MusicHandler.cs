/*using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

public class MusicHandler : MonoBehaviour
{
    [SerializeField]
    private AudioSource firstSource;

    [SerializeField]
    private float firstLoopTime = 20;

    [SerializeField]
    private AudioSource secondSource;

    [SerializeField]
    private float secondLoopTime = 10;

    [SerializeField]
    private AudioSource weirdSource;

    [SerializeField]
    private AudioSource mainSource;

    [SerializeField]
    private AudioMixer musicMixer;

    private AudioSource currentSource;

    private bool playing = false;

    private void Start()
    {
        GameManager.Instance.OnGameStart += Instance_OnGameStart;

        //LevelUIManager.Instance.OnOptionsSpawned += StopPlaying;
        //LevelUIManager.Instance.OnOptionsCleared += Play;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnGameStart -= Instance_OnGameStart;

        //LevelUIManager.Instance.OnOptionsSpawned -= StopPlaying;
        //LevelUIManager.Instance.OnOptionsCleared -= Play;
    }

    private void Instance_OnGameStart()
    {
        PlayingLoops();
        Play();
    }

    public async void Play()
    {
        playing = true;

        float t = 0;

        musicMixer.GetFloat("Volume", out float target);
        float start = 0;

        while (t <= 1.0f)
        {
            t += Time.deltaTime;

            musicMixer.SetFloat("Volume", Mathf.Lerp(start, target, Mathf.SmoothStep(0.0f, 1.0f, t)));

            await Task.Yield();
        }

        musicMixer.SetFloat("Volume", target);
    }

    public void StopPlaying()
    {
        playing = false;
    }

    private void Update()
    {
        if (currentSource == null)
        {
            return;
        }

        if (playing)
        {
            if (!currentSource.isPlaying)
            {
                currentSource.Play();
            }
        }
        else
        {
            if (currentSource.isPlaying)
            {
                currentSource.Stop();
            }
        }
    }

    private async void PlayingLoops()
    {
        ChangeSource(firstSource);
        float t = 0;

        while (t <= firstLoopTime)
        {
            if (playing)
            {
                t += Time.deltaTime;
            }

            await Task.Yield();
        }

        ChangeSource(secondSource);

        t = 0;
        while (t <= secondLoopTime)
        {
            if (playing)
            {
                t += Time.deltaTime;
            }

            await Task.Yield();
        }

        ChangeSource(weirdSource);
        t = 0;
        while (t <= weirdSource.clip.length)
        {
            if (playing)
            {
                t += Time.deltaTime;
            }

            await Task.Yield();
        }

        ChangeSource(mainSource);
    }

    private void ChangeSource(AudioSource newSource)
    {
        if (currentSource != null)
        {
            currentSource.Stop();
        }

        currentSource = newSource;
    }
}
*/