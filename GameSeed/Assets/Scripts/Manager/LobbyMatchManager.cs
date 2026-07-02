using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LobbyMatchManager : MonoBehaviour
{
    public Button BossButton;
    public Button MatchButton;
    public TMP_Text wincount;
    
    // Changed from TextMeshProUGUI to GameObject
    public GameObject warning; 
    
    public bool TutorialFinished; //ini nnti juga ganti kl dah ada scene tutorial
    public int winNumber;
    public int winNumberCondition; //ini nanti ganti aja sesuai apa sih kondisi sebenarnya
    private int win = 0;
    [SerializeField] private SceneController sceneController;
    private PortraitAnimator portraitAnimator;
    
    void Start()
    {
        portraitAnimator = GameObject.Find("Animaton").GetComponent<PortraitAnimator>();

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

        // Use SetActive(false) for GameObjects instead of enabled
        warning.SetActive(false); 
        
        WinCountDisplay();
        portraitAnimator.PlayEventIn("KELUAR");
        print("ini masuk ga si");
        portraitAnimator.PlayEventOut("KELUAR");
        DOVirtual.DelayedCall(1f, () => portraitAnimator.PlayEventIn("MASUK"));
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
        // Use SetActive(true/false) for GameObjects
        
        warning.SetActive(true);
        portraitAnimator.PlayEventIn("DIBERENYAHO");
        yield return new WaitForSeconds(1f);
        portraitAnimator.PlayEventOut("DIBERENYAHO");
        yield return new WaitForSeconds(2f);
        warning.SetActive(false);
        
    }
}