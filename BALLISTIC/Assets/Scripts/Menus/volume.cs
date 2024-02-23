using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class volume : MonoBehaviour
{
    public AudioSource bgmAudioSource;

    public void AdjustVolume(float volume) {
        bgmAudioSource.volume = volume;
    }

}
