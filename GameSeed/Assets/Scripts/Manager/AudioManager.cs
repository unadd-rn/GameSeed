using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource SFXSource;
    [SerializeField] private AudioSource loopingSFXSource;

    [System.Serializable]
    public struct MusicTrack
    {
        public string trackName;
        public AudioClip clip;
    }

    [System.Serializable]
    public struct SoundEffect
    {
        public string groupID;
        public AudioClip[] clips;
    }

    [Header("Audio Collections")]
    public MusicTrack[] tracks;
    public SoundEffect[] soundEffects;
    
    private void Awake()
    {
        // Singleton pattern yang lebih rapi
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // kl mw manggil -> AudioManager.Instance.PlayMusic("MenuBGM");
    public void PlayMusic(string trackName)
    {
        AudioClip clip = GetMusicFromName(trackName);
        
        if (clip == null) 
        {
            Debug.LogWarning($"Music track '{trackName}' tidak ditemukan!");
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    // kl mw manggil -> AudioManager.Instance.PlaySFX("Jump");
    public void PlaySFX(string groupID)
    {
        AudioClip clip = GetSoundEffectFromName(groupID);
        
        if (clip != null)
        {
            SFXSource.PlayOneShot(clip);
        }
        else 
        {
            Debug.LogWarning($"SFX group '{groupID}' kosong atau tidak ditemukan!");
        }
    }

    public void PlayLoopingSFX(string groupID)
    {
        AudioClip clip = GetSoundEffectFromName(groupID);
        
        if (clip != null)
        {
            loopingSFXSource.clip = clip;
            loopingSFXSource.loop = true; // Set loop jadi true
            if (!loopingSFXSource.isPlaying)
            {
                loopingSFXSource.Play();
            }
        }
        else 
        {
            Debug.LogWarning($"SFX group '{groupID}' kosong atau tidak ditemukan!");
        }
    }

    // ---> Fungsi Baru Buat Berhentiin SFX Looping
    public void StopLoopingSFX()
    {
        if (loopingSFXSource.isPlaying)
        {
            loopingSFXSource.Stop();
        }
    }

    private AudioClip GetMusicFromName(string trackName)
    {
        foreach (var track in tracks)
        {
            if (track.trackName == trackName)
            {
                return track.clip;
            }
        }
        return null;
    }

    private AudioClip GetSoundEffectFromName(string name)
    {
        foreach (var soundEffect in soundEffects)
        {
            if (soundEffect.groupID == name)
            {
                if (soundEffect.clips.Length > 0)
                {
                    return soundEffect.clips[Random.Range(0, soundEffect.clips.Length)];
                }
            }
        }
        return null;
    }
}




// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class AudioManager : MonoBehaviour
// {
//     public static AudioManager Instance;
//     [SerializeField] AudioSource musicSource;
//     [SerializeField] AudioSource SFXSource;

//     [Header("Music")]
//     public AudioClip BGMMenu;

//     [Header("SFX")]
//     public AudioClip BrokenStick;

//     [System.Serializable]
//     public struct MusicTrack
//     {
//         public string trackName;
//         public AudioClip clip;
//     }

//     [System.Serializable]
//     public struct SoundEffect
//     {
//         public string groupID;
//         public AudioClip[] clips;
//     }
    
//     private void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }

//         Instance = this;

//         DontDestroyOnLoad(gameObject);
//     }
 
//     public AudioClip GetClipFromName(string trackName)
//     {
//         foreach (var track in tracks)
//         {
//             if (track.trackName == trackName)
//             {
//                 return track.clip;
//             }
//         }
//         return null;
//     }

//     public void PlayMusic(AudioClip clip)
//     {
//         if(musicSource.clip == clip && musicSource.isPlaying) return;

//         musicSource.clip = clip;
//         musicSource.loop = true;
//         musicSource.Play();
//     }

//     public void PlaySFX(AudioClip clip)
//     {
//         SFXSource.PlayOneShot(clip);
//     }
// }
