using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    private bool isPaused = false;

    [Header("Old UI References (Optional)")]
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject otherUI;
    [SerializeField] private GameObject enemySlider;
    [SerializeField] private GameObject playerSlider;

    [Header("Animation Settings")]
    [SerializeField] private PortraitAnimator portraitAnimator;
    [Tooltip("Nama event untuk panel Pause di Portrait Animator")]
    [SerializeField] private string pauseEventName = "Pause";
    [Tooltip("Nama event untuk UI utama/gameplay jika ingin ikut dianimasikan keluar saat pause")]
    [SerializeField] private string otherUIEventName = "OtherUI";

    public void Pause()
    {
        isPaused = !isPaused;

        Debug.Log("masuk");

        // Jika lupa memasang PortraitAnimator di Inspector, script akan otomatis memakai sistem SetActive lamamu (Fallback)
        if (portraitAnimator == null)
        {
            Time.timeScale = isPaused ? 0 : 1;
            if (pauseUI != null) pauseUI.SetActive(isPaused);
            if (otherUI != null) otherUI.SetActive(!isPaused);
            if (enemySlider != null) enemySlider.SetActive(!isPaused);
            if (playerSlider != null) playerSlider.SetActive(!isPaused);
            return;
        }

        // --- SISTEM ANIMASI BARU ---
        if (isPaused)
        {
            // 1. Munculkan panel Pause lewat animasi
            portraitAnimator.PlayEventIn(pauseEventName);
            
            // 2. Sembunyikan UI utama lewat animasi (jika kamu mendaftarkannya juga di PortraitAnimator)
            portraitAnimator.PlayEventOut(otherUIEventName);

            // Opsional: Untuk slider, jika tidak dianimasikan, kamu bisa langsung hilangkan/munculkan secara instan
            if (enemySlider != null) enemySlider.SetActive(false);
            if (playerSlider != null) playerSlider.SetActive(false);

            // 3. Tunggu animasi masuk selesai (~0.5 detik), baru hentikan waktu game
            CancelInvoke("FreezeTime");
            Invoke("FreezeTime", 0.5f);
        }
        else
        {
            // 1. Kembalikan waktu ke normal SEGERA agar animasi tombol keluar bisa berjalan
            Time.timeScale = 1f;
            CancelInvoke("FreezeTime");

            // 2. Sembunyikan panel Pause lewat animasi
            portraitAnimator.PlayEventOut(pauseEventName);
            
            // 3. Munculkan kembali UI utama lewat animasi
            portraitAnimator.PlayEventIn(otherUIEventName);

            // Kembalikan slider ke posisi aktif
            if (enemySlider != null) enemySlider.SetActive(true);
            if (playerSlider != null) playerSlider.SetActive(true);
        }
    }

    private void FreezeTime()
    {
        if (isPaused)
        {
            Time.timeScale = 0f;
        }
    }
}

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class PauseManager : MonoBehaviour
// {
//     private bool isPaused = false;
//     [SerializeField] private GameObject pauseUI;
//     [SerializeField] private GameObject otherUI;
//     [SerializeField] private GameObject enemySlider;
//     [SerializeField] private GameObject playerSlider;

//     public void Pause()
//     {
//         isPaused = !isPaused;
//         Time.timeScale = isPaused ? 0 : 1;
//         if (pauseUI != null) pauseUI.SetActive(isPaused);
//         if (pauseUI != null) otherUI.SetActive(!isPaused);
//     }
// }
