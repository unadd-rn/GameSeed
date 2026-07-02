using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

public class BenerinTimeline : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;

    void Awake()
    {
        // director = GetComponent<PlayableDirector>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Plays the Timeline when any scene finishes loading
        director.Play();
    }

}
