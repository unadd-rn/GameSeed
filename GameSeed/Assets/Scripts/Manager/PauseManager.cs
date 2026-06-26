using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    private bool isPaused = false;
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject otherUI;
    [SerializeField] private GameObject enemySlider;
    [SerializeField] private GameObject playerSlider;

    public void Pause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
        if (pauseUI != null) pauseUI.SetActive(isPaused);
        if (pauseUI != null) otherUI.SetActive(!isPaused);
    }
}
