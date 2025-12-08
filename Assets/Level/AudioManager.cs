using UnityEngine;
using System;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public Sound[] sounds;

    AudioSource _musicTrack;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        s.source.PlayOneShot(s.clip);
    }

    public void StopPlaying(string sound)
    {
        Sound s = Array.Find(sounds, item => item.name == sound);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Stop();
    }

    public void SetMusic(string musicName)
    {
        if (_musicTrack != null)
            _musicTrack.Stop();
        Sound s = Array.Find(sounds, sound => sound.name == musicName);
        s.source.loop = true;
        _musicTrack = s.source;
        s.source.Play();
    }

    public void StopMusic()
    {
        if (_musicTrack != null)
            _musicTrack.Stop();
    }
}
