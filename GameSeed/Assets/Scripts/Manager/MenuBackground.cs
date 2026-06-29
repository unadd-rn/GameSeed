using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuBackground : MonoBehaviour
{
    public static MenuBackground Instance;

    [SerializeField] Image frameImage;
    [SerializeField] Sprite[] frames;
    [SerializeField] float fps = 12f;

    Coroutine _anim;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartAnim()
    {
        if (_anim != null) StopCoroutine(_anim);
        
        // Ensure the Image component is turned on visually
        if (frameImage != null) 
        {
            frameImage.enabled = true; 
            frameImage.gameObject.SetActive(true);
        }
        
        _anim = StartCoroutine(Loop());
    }

    public void StopAnim()
    {
        if (_anim != null) StopCoroutine(_anim);
        _anim = null;
        
        // Turn off ONLY the image component so it stops rendering, 
        // leaving the game object and transition routines intact!
        if (frameImage != null) frameImage.enabled = false; 
    }

    IEnumerator Loop()
    {
        if (frames == null || frames.Length == 0) yield break;
        
        float interval = 1f / fps;
        int i = 0;
        
        while (true)
        {
            if (frameImage == null) yield break;
            
            frameImage.sprite = frames[i % frames.Length];
            i++;
            yield return new WaitForSecondsRealtime(interval);
        }
    }
}