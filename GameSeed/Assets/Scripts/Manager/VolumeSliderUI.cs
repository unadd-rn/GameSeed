using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderUI : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;

    private void Start()
    {
        Debug.Log("[VolumeSliderUI] Instance null? " + (VolumeSettings.Instance == null));
        Debug.Log("[VolumeSliderUI] GetMasterVolume: " + VolumeSettings.Instance.GetMasterVolume());
        masterSlider.value = VolumeSettings.Instance.GetMasterVolume();
        Debug.Log("[VolumeSliderUI] slider.value after set: " + masterSlider.value);
        masterSlider.onValueChanged.AddListener(VolumeSettings.Instance.SetMasterVolume);
    }
}
