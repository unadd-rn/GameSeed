using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    //kalau udah ada transisi, nanti langsung pake scene controller aja
    public void Play()
    {
        Debug.Log("Continue");
        if (!PlayerPrefs.HasKey("SavedLevel"))
        {
            Debug.Log("[MainMenu] no save so new game");
            StartCoroutine(LoadLevelAfterDelay("Unad", 1.5f));
            return;
        }

        string levelName = PlayerPrefs.GetString("SavedLevel");
        Debug.Log($"[MainMenu] continye {levelName}");

        // wait for 1.5f second before load scene
        StartCoroutine(LoadLevelAfterDelay(levelName, 1.5f));
    }

    public void Reset()
    {
        Debug.Log("Reset Game");

        PlayerPrefs.DeleteAll();

        StartCoroutine(LoadLevelAfterDelay("Unad", 1.5f));
    }

    public void Map()
    {
        Debug.Log("Map: Level Selection");

//blm ada
        StartCoroutine(LoadLevelAfterDelay("Unad", 1.5f));
    }

    public void Garage()
    {
        Debug.Log("Labby??");
//blm ada
        StartCoroutine(LoadLevelAfterDelay("Unad", 1.5f));
    }

    IEnumerator LoadLevelAfterDelay(string levelName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(levelName);
    }

    public void QuitGame()
    {
        Debug.Log("[MainMenu] I QUIT");
        Application.Quit();
    }
}
