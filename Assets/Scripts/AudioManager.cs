using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundLevel
{
    Off,
    Low,
    Mid,
    High
}

public static class Extension
{
    public static float Level(this SoundLevel soundLevel)
    {
        switch (soundLevel)
        {
            case SoundLevel.Off:
                return 0;
            case SoundLevel.Low:
                return 0.33f;
            case SoundLevel.Mid:
                return 0.66f;
            case SoundLevel.High:
                return 1;
            default:
                return 0;
        }
    }
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public List<Sound> sounds = new List<Sound>();
    public bool audioEnabled;
    private string _currentlyPlaying;
    private SoundLevel _soundLevel = SoundLevel.Mid;

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
            s.source.volume = SoundLevel.Level();
        }
        
        DontDestroyOnLoad(gameObject);
    }

    public Sound Play(string name)
    {
        if (audioEnabled)
        {
            Sound s = sounds.Find(sound => sound.name == name);
            s.source.volume = s.volume;
            if (s != null) s.source.Play();
            return s;
        }
        return null;
    }

    public SoundLevel SoundLevel
    {
        get
        {
            return _soundLevel;
        }
        set
        {
            _soundLevel = value;
            AdjustVolume();
        }
    }

    public void PlayWithRandomPitch(string name)
    {
        PlayWithPitch(name, (Random.Range(0.6f, .9f)));
    }

    public void PlayConnect(int streakCount)
    {
        int randomConnect = Random.Range(1, 5);
        Play("connect" + randomConnect);
        
        // Testing without random pitch
        
        // if (audioEnabled)
        // {
        //     int randomConnectSound = UnityEngine.Random.Range(1, 5);
        //     Sound s = sounds.Find(sound => sound.name == "connect" + randomConnectSound);
        //     if (s != null)
        //     {
        //         float pitch = Mathf.Min(0.6f + (0.1f * streakCount), 0.9f);
        //         Debug.Log(string.Format("Pitch {0}", pitch));
        //         s.source.pitch = pitch;
        //         s.source.Play();
        //     }
        // }

        switch (streakCount)
        {
            case 1:
                Play("streak0");
                break;
            case 2:
                Play("streak1");
                break;
            case 3:
                Play("streak2");
                break;
            case 4:
                Play("streak3");
                break;
            case 5:
                Play("streak4");
                break;
            default:
                break;
        }
    }

    public void PlayWithPitch(string name, float pitch)
    {
        if (audioEnabled)
        {
            Sound s = sounds.Find(sound => sound.name == name);
            if (s != null)
            {
                s.source.pitch = pitch;
                s.source.volume = SoundLevel.Level();
                s.source.Play();
            }
        }
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
        if (_currentlyPlaying != null)
        {
            Stop(_currentlyPlaying);
        }
        if (audioEnabled)
        {
            Sound s = Play(name);
            s.source.loop = true;
            _currentlyPlaying = name;
        }
    }

    public void AdjustVolume()
    {
        foreach(Sound sound in sounds)
        {
            sound.source.volume = SoundLevel.Level();
        }
    }
}