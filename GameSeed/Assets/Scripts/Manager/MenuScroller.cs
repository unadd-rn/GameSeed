using System.Collections;
using UnityEngine;

public class MenuScroller : MonoBehaviour
{
    [Header("Setup UI")]
    public RectTransform menuContainer; // Masukkan MenuContainer ke sini
    public float targetYPosition;     // Titik akhir scroll 
    public float scrollDuration = 1.2f;
    private bool hasScrolled = false;

    void Start()
    {
        if (PlayerPrefs.GetInt("HasScrolled", 0) == 1)
        {
            hasScrolled = true; 
            menuContainer.anchoredPosition = new Vector2(menuContainer.anchoredPosition.x, targetYPosition);
            
        }
    }

    void Update()
    {
        if (!hasScrolled && Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mulai UI Scroll!");
            StartCoroutine(DoSmoothScroll());
        }
    }

    IEnumerator DoSmoothScroll()
    {
        hasScrolled = true;
        
        // Catat posisi Y awal dari container
        Vector2 startPosition = menuContainer.anchoredPosition;
        Vector2 targetPosition = new Vector2(startPosition.x, targetYPosition);
        
        float elapsedTime = 0f;

        while (elapsedTime < scrollDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / scrollDuration;

            // Efek Ease-In Ease-Out
            t = Mathf.SmoothStep(0f, 1f, t);

            // Geser UI menggunakan anchoredPosition
            menuContainer.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        // Pastikan posisi mendarat dengan pas
        menuContainer.anchoredPosition = targetPosition;

        PlayerPrefs.SetInt("HasScrolled", 1);
        PlayerPrefs.Save();
    }
}