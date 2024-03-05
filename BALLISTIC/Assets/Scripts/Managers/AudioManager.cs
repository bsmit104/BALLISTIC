using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public Sound[] sounds, music;
    private Dictionary<string, Sound> soundDict = new Dictionary<string, Sound>();
    private Dictionary<string, Sound> musicDict = new Dictionary<string, Sound>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (Sound s in sounds)
        {
            soundDict.Add(s.name, s);

        }

        foreach (Sound m in music)
        {
            musicDict.Add(m.name, m);
        }

        PlayMusic("MenuMusic");
    }

    // usage example: AudioManager.Instance.PlaySound("soundName");
    public void PlaySound(string name, GameObject caller)
    {
        Sound s = soundDict[name];
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        // Check if audio source already exists
        var audioSources = caller.GetComponents<AudioSource>();
        if (audioSources != null)
        {
            foreach (AudioSource a in audioSources)
            {
                if (a.clip == soundDict[name].clip)
                {
                    a.Play();
                    return;
                }
            }
        }
        s.source = caller.AddComponent<AudioSource>();
        s.source.clip = s.clip;
        s.source.volume = s.volume;
        s.source.pitch = s.pitch;
        s.source.spatialBlend = 1;
        s.source.loop = s.loop;
        s.source.Play();
    }

    // usage example: AudioManager.Instance.StopSound("soundName");
    public void StopSound(string name, GameObject caller)
    {
        var audioSources = caller.GetComponents<AudioSource>();
        foreach (AudioSource a in audioSources)
        {
            if (a.clip == soundDict[name].clip)
            {
                a.Stop();
            }
        }
    }

    public void PlayMusic(string name)
    {
        Sound m = musicDict[name];
        if (m == null)
        {
            Debug.LogWarning("Music: " + name + " not found!");
            return;
        }
        m.source = gameObject.AddComponent<AudioSource>();
        m.source.clip = m.clip;
        m.source.volume = m.volume;
        m.source.pitch = m.pitch;
        m.source.loop = m.loop;
        m.source.Play();
    }

    public void StopMusic(string name)
    {
        Sound m = musicDict[name];
        if (m == null)
        {
            Debug.LogWarning("Music: " + name + " not found!");
            return;
        }
        m.source.Stop();
    }
}