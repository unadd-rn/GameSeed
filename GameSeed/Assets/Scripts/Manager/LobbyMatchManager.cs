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
    public bool TutorialFinished; //ini nnti juga ganti kl dah ada scene tutorial
    public int winNumber;
    public int winNumberCondition; //ini nanti ganti aja sesuai apa sih kondisi sebenarnya
    void Start()
    {
        BossButton.interactable = false;
        MatchButton.interactable = false;

        if (TutorialFinished)
        {
            MatchButton.interactable = true;
            if (winNumber>=winNumberCondition) {
                BossButton.interactable = true;
            }
            else
            {
                BossButton.interactable = false;
            }
        }
        WinCountDisplay();
    }

    private void WinCountDisplay()
    {
        int win;
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

    public void BossAnimation()
    {
        //??? gw bingung bgt ini mending di taro di scene unad apa disini
    }
}
