using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    public static VolumeSettings Instance;
    [SerializeField] private AudioMixer myMixer;

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
        }
    }

    private void Start()
    {
        Debug.Log("[VolumeSettings] HasKey: " + PlayerPrefs.HasKey("masterVolume"));
        if (PlayerPrefs.HasKey("masterVolume"))
            SetMasterVolume(PlayerPrefs.GetFloat("masterVolume"));
        else
            SetMasterVolume(1f); // default
    }

    public void SetMasterVolume(float volume)
    {
        Debug.Log("[VolumeSettings] SetMasterVolume called with: " + volume);
        myMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20);
        PlayerPrefs.SetFloat("masterVolume", volume);
    }

    public float GetMasterVolume()
    {
        return PlayerPrefs.HasKey("masterVolume") ? PlayerPrefs.GetFloat("masterVolume") : 1f;
    }
}