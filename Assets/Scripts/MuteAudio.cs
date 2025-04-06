using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuteAudio : MonoBehaviour
{
    public AudioSource musicSource;

    bool isMuted = false;

    public void MusicToggle()
    {
        if (isMuted)
        {
            PlayMusic();
        }
        else
        {
            PauseMusic();
        }
    }

    private void PauseMusic()
    {
        musicSource.Pause();
        isMuted = true;
    }

    private void PlayMusic()
    {
        musicSource.Play();
        isMuted = false;
    }
}
