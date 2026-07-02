using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;
    [SerializeField] Animator transitionAnim;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }

    // Now accepts a specific transition name!
    public void goToSceneName(string name, string transitionName = "Default")
    {
        if (transitionName == "")
        {
            SceneManager.LoadScene(name);
            //StartCoroutine(LoadLevel(name));
        }else{
            TransitionManager.Instance.GoToScene(name, transitionName);
        }
    }

    IEnumerator LoadLevel(string name)
    {
        yield return new WaitForSecondsRealtime(1.3f);
        SceneManager.LoadScene(name);
    }

    #region Main Menu
    public void Play()
    {
        Debug.Log("Play");
        if (!PlayerPrefs.HasKey("SavedLevel"))
        {
            Debug.Log("[MainMenu] no save so new game");
            // Example: Using a transition named "Circle"
            goToSceneName("Cet - matchLobby", "Gelap");
            AudioManager.Instance.PlayMusic("MainMenu");
            return;
        }
    }

    public void Reset()
    {
        Debug.Log("Reset Game");
        PlayerPrefs.DeleteAll();
    }

    public void Lobby()
    {
        Debug.Log("balik ke matchlobby");
        // Example: Using a transition named "Square"
        goToSceneName("Cet - matchLobby", "");
        AudioManager.Instance.PlayMusic("MainMenu");
    }

    public void Rematch()
    {
        Debug.Log("balik ke Unad");
        // Example: Using a transition named "Square"
        goToSceneName("Unad", "Gelap");
    }

    public void Match()
    {
        Debug.Log("balik ke Match");
        // Example: Using a transition named "Square"
        goToSceneName("Match", "Gelap");
        AudioManager.Instance.PlayMusic("InGame");
    }

    public void Lobby1()
    {
        Debug.Log("balik ke lobby1");
        // Example: Using a transition named "Square"
        goToSceneName("Cet - lobby1", "");
        AudioManager.Instance.PlayMusic("MainMenu");
    }

    public void Garage()
    {
        Debug.Log("ke garage");
        // Example: Using a transition named "GarageDoor"
        goToSceneName("Rae - Garage 2", "Gelap");
        AudioManager.Instance.PlayMusic("MainMenu");
    }
    #endregion //mainmenu End

    #region Match lobby
    public void StartMatch(string type)
    {
        Debug.Log("ke tempat tarung");

        PlayerPrefs.SetString("MatchStatus", "match mulai");
        PlayerPrefs.SetString("MatchType", type);
        PlayerPrefs.Save();

        // Example: Using a transition named "BattleWipe"
        goToSceneName("Match", "Terang");
        AudioManager.Instance.PlayMusic("InGame");
    }

    public void StartBoss(string type)
    {
        Debug.Log("ke tempat tarung");

        PlayerPrefs.SetString("MatchStatus", "match mulai");
        PlayerPrefs.SetString("MatchType", type);
        PlayerPrefs.Save();

        // Example: Using a transition named "BattleWipe"
        goToSceneName("Unad", "Terang");
        AudioManager.Instance.PlayMusic("AyamBoss");
    }
    #endregion

    public void SFXButtonPressed()
    {
        AudioManager.Instance.PlaySFX("ButtonPressed");
    }
}