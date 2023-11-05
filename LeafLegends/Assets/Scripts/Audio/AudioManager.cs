using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum SFX
{
    LeafCollected,
    Footstep,
    GroundSlideStartup,
    Jump,
    Land,
    Rake,
    WallSlideStartup,
    Lever,
    GrappleThrow,
    GrapplePull,
    DoorOpen,
    VictoryJingle
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField]
    private AudioSource musicSource;

    [SerializeField]
    private AudioClip musicClip;

    [SerializeField]
    private SFXEntry[] sfxClips;
    
    [System.Serializable]
    private struct SFXEntry
    {
        public SFX type;
        public AudioClip[] clip;
        [Range(0,1)] public float volume;
    }

    private void Awake()
    {
        Instance = this;
        musicSource.clip = musicClip;
        musicSource.Play();
    }

    private void OnValidate()
    {
        for (var i = 0; i < sfxClips.Length; i++)
        {
            sfxClips[i].type = (SFX)i;
        }
    }

    public void SetMusicEnabled(bool val)
    {
        musicSource.mute = !val;
    }

    public void PlaySFX(SFX sfxType, Vector2 pos)
    {
        var clips = sfxClips[(int)sfxType].clip;
        if (clips.Length == 0)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(clips[Random.Range(0, clips.Length)], pos, sfxClips[(int)sfxType].volume);
    }
}