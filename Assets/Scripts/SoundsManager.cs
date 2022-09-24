using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Enum Structures

public enum EnumBeeps
{
    damage,
    credits
}

public enum EnumMusics
{
    Waltz,
    Synapse,
    Fantasy,
    Sunny
}

#endregion

[RequireComponent(typeof(AudioSource))]
public class SoundsManager : GlobalBehaviour<SoundsManager>
{
    #region Serializable Fields

    [SerializeField]
    private List<AudioClip> beepClips;

    [SerializeField]
    private List<AudioClip> musicClips;

    #endregion

    #region Global Variables

    private AudioSource audioSource;

    #endregion

    #region Unity Lifecycle

    IEnumerator Start()
    {
        yield return new WaitUntil(() => TryGetComponent(out audioSource)); 
    }

    #endregion

    #region Functions / Methods

    public void PlayBeep(EnumBeeps enumBeep)
    {
        if (beepClips != null && beepClips.Count > 0)
        {
            var beepClip = beepClips.Find(beep =>
                Enum.GetName(typeof(EnumBeeps), enumBeep) == beep.name);

            if (beepClip)
            {
                audioSource.Pause();
                audioSource.loop = false;
                audioSource.PlayOneShot(beepClip);
                audioSource.loop = true;
                audioSource.UnPause();
            }                
        }
    }

    public IEnumerator PlayMusicCoroutine(EnumMusics enumMusic)
    {
        if (musicClips != null && musicClips.Count > 0)
        {
            var musicClip = musicClips.Find(music =>
                Enum.GetName(typeof(EnumMusics), enumMusic) == music.name);

            if (musicClip)
            {
                yield return new WaitUntil(() => audioSource);

                if (audioSource.isPlaying)
                    audioSource.Stop();

                audioSource.clip = musicClip;
                audioSource.Play();

                if (!audioSource.loop)
                    audioSource.loop = true;
            }
        }
    }

    #endregion
}
