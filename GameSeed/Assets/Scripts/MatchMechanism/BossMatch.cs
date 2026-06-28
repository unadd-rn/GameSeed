using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMatch : MonoBehaviour
{
    private string currentMatchType;
    [SerializeField] EnemyAI enemyAI;
    [SerializeField] GameObject gameUI;
    [SerializeField] GameObject vs;

    void Start()
    {
        CheckMatchTypeLogic();
    }

    private void CheckMatchTypeLogic()
    {
            if (gameUI != null) gameUI.SetActive(false);
            if (enemyAI != null) enemyAI.monteCarloSimulations = 100;
            StartCoroutine(runBossMatchAnimation());
    }

    IEnumerator runBossMatchAnimation()
    {
        yield return new WaitForSeconds(2.5f);

        if (gameUI != null) gameUI.SetActive(true);
        if (vs != null) vs.SetActive(false);
    }
}
