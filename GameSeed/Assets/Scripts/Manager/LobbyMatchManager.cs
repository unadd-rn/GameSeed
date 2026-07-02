using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyMatchManager : MonoBehaviour
{
    public Button BossButton;
    public Button MatchButton;
    public TMP_Text wincount;
    public TextMeshProUGUI warning;
    public bool TutorialFinished; //ini nnti juga ganti kl dah ada scene tutorial
    public int winNumber;
    public int winNumberCondition; //ini nanti ganti aja sesuai apa sih kondisi sebenarnya
    private int win = 0;
    [SerializeField] private SceneController sceneController;
    void Start()
    {
        // BossButton.interactable = false;
        // MatchButton.interactable = false;

        // if (TutorialFinished)
        // {
        //     MatchButton.interactable = true;
        //     if (winNumber>=winNumberCondition) {
        //         BossButton.interactable = true;
        //     }
        //     else
        //     {
        //         BossButton.interactable = false;
        //     }
        // }
        warning.enabled =false;
        WinCountDisplay();
    }

    private void WinCountDisplay()
    {
        if (!PlayerPrefs.HasKey("WinCount"))
        {
            win = 0;
        }
        else
        {
            win = PlayerPrefs.GetInt("WinCount");
        } 
        wincount.text = win.ToString();
    }

    public void BossPressed()
    {
        if(win < winNumberCondition)
        {
            //suara warning apa kek
            StartCoroutine(Warning());

        }
        else
        {
            sceneController.StartBoss("boss");
        }
    }

    IEnumerator Warning()
    {
        warning.enabled = true;
        yield return new WaitForSeconds(2f);
        warning.enabled = false;
    }
}
