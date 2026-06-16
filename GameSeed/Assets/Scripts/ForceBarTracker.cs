using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ForceBarTracker : MonoBehaviour
{
    [Header("Core Settings")]
    [SerializeField] private Image bar;
    [SerializeField] private int forceCurrent = 50;
    [SerializeField] private int forceMax = 100;
    [Space]
    [SerializeField] private bool overkillPossible; // idk what does this do let's just move

    // Start is called before the first frame update
    private void Start()
    {
        UpdateBarAndForceText();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateBarAndForceText()
    {
        if(forceMax == 0){
            bar.fillAmount = 0; return;
        }

        float fillAmount = (float) forceCurrent / forceMax;
    }

    public bool ChangeForce(int amount)
    {
        if(!overkillPossible && forceCurrent + amount < 0) return false;
        
        forceCurrent += amount;
        forceCurrent = Mathf.Clamp(value:forceCurrent, min:0, forceMax);

        bar.fillAmount = (float) forceCurrent / forceMax;

        return true;
    }
}
