using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMatch : MonoBehaviour
{
    private string currentMatchType;
    [SerializeField] EnemyAI enemyAI;
    [SerializeField] GameObject gameUI;
    [SerializeField] GameObject vs;
    [SerializeField] StickThrowTest playerThrowScript;

    void Start()
    {
        AudioManager.Instance.PlaySFX("VersusAlt");
    }

    public void CheckMatchTypeLogic()
    {
            if (enemyAI != null) enemyAI.minSimulations = 100;
            if (enemyAI != null) enemyAI.maxSimulations = 100;
            StartCoroutine(runBossMatchAnimation());
    }

    IEnumerator runBossMatchAnimation()
    {
        if (playerThrowScript != null) playerThrowScript.SetUIVisible(false);
        
        yield return new WaitForSeconds(3.33f);
        if (vs != null) vs.SetActive(false);
        
        TurnManager.Instance.PlayEntranceTransition();

        yield return new WaitForSeconds(1.0f); 
        TurnManager.Instance.SetState(TurnState.PlayerPlacement);
    }
}
