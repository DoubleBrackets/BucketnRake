using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource musicSource;

    [SerializeField]
    private AudioClip musicClip;

    private void Awake()
    {
        musicSource.clip = musicClip;
        musicSource.Play();
    }

    public void SetMusicEnabled(bool val)
    {
        musicSource.mute = val;
    }
}