using UnityEngine.Audio; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public List<Sound> sounds = new List<Sound>();
    public bool audioEnabled;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
        }
    }

    public Sound Play(string name)
    {
        if (audioEnabled)
        {
            Sound s = sounds.Find(sound => sound.name == name);
            if (s != null) s.source.Play();
            return s;
        }
        return null;
    }

    public void Stop(string name)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        if (s != null) s.source.Stop();
    }

    public void StopAll()
    {
        for (int i = 0; i < sounds.Count; i++)
        {
            Sound s = sounds[i];
            s.source.Stop();
        }
    }

    public void PlayAndLoop(string name)
    {
        if (audioEnabled)
        {
            Sound s = Play(name);
            s.source.loop = true;
        }
    }
}