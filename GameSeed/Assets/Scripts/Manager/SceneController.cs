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

    public void goToSceneName(string name)
    {
        StartCoroutine(LoadLevel(name));
    }

    IEnumerator LoadLevel(string name)
    {
        //transitionAnim.SetTrigger("End");
        yield return new WaitForSeconds(0.45f);
        SceneManager.LoadScene(name);
        //transitionAnim.SetTrigger("Start");
        //ini trignya nyalain kl dah ada scene transitionnya
    }

    #region Main Menu
    public void Play()
    {
        Debug.Log("Play");
        if (!PlayerPrefs.HasKey("SavedLevel"))
        {
            Debug.Log("[MainMenu] no save so new game");
            goToSceneName("Cet - matchLobby");
            return;
        }

        // string levelName = PlayerPrefs.GetString("SavedLevel");
        // Debug.Log($"[MainMenu] continye {levelName}");

        // // wait for 1.5f second before load scene
        // StartCoroutine(LoadLevel(levelName));
        //nnti duluu
    }

    public void Reset()
    {
        Debug.Log("Reset Game");

        PlayerPrefs.DeleteAll();
    }

    public void Lobby1()
    {
        Debug.Log("balik ke lobby1");
        goToSceneName("Cet - lobby1");
    }

    public void Garage()
    {
        Debug.Log("ke lobby 2");
        goToSceneName("Rae - Garage");
    }
#endregion //mainmenu End

#region Match lobby
    public void StartMatch(string type)
    {
        Debug.Log("ke tempat tarung");

        PlayerPrefs.SetString("MatchStatus", "match mulai");
        PlayerPrefs.SetString("MatchType", type);
        PlayerPrefs.Save();

        goToSceneName("Unad");
    }
#endregion
}
