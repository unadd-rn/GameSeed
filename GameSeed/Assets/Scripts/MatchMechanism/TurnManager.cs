using System.Collections;
using UnityEngine;

public enum TurnState { PlayerPlacement, PlayerThrowing, EnemyTurn, End }

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    [Header("References")]
    [SerializeField] private StickSpawn playerSpawnScript;
    [SerializeField] private StickThrowTest playerThrowScript;
    [SerializeField] private EnemyAI enemyAIScript;
    [SerializeField] private bool isTutorialScene = false;
    private TurnState currentState;
    private PortraitAnimator portraitAnimator;
    private BossMatch bossMatch;
    private bool tutorialMode = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        portraitAnimator = GameObject.Find("Animaton").GetComponent<PortraitAnimator>();
        tutorialMode = isTutorialScene;
    }

    void Start()
    {
        bossMatch = GetComponent<BossMatch>();

        if (bossMatch != null)
        {
            if (playerThrowScript != null) playerThrowScript.SetUIVisible(false);
            bossMatch.CheckMatchTypeLogic(); 
            Debug.Log("bossmatch found");
        }
        else 
        {
            if (portraitAnimator != null) portraitAnimator.PlayEventIn("IN/OUT");
            SetState(TurnState.PlayerPlacement);
            Debug.Log("bossmatch null");
        }
    }

    public void PlayEntranceTransition()
    {
        if (portraitAnimator != null)
        {
            portraitAnimator.PlayEventIn("IN/OUT");
        }
    }

    public void SetState(TurnState newState)
    {
        if (currentState == TurnState.End) return;
        currentState = newState;

        switch (currentState)
        {
            case TurnState.PlayerPlacement:
                Debug.Log("Turn placement");
                if (playerSpawnScript != null) playerSpawnScript.enabled = true;
                if (playerThrowScript != null) playerThrowScript.enabled = true;
                if (enemyAIScript != null) enemyAIScript.enabled = false;
                playerThrowScript.SetUIVisible(false);
                if (!tutorialMode)
                {
                    playerSpawnScript.SetPlacementAllowed(true);
                }
                else
                {
                    playerSpawnScript.SetPlacementAllowed(false);
                }
                break;

            case TurnState.PlayerThrowing:
                if (playerThrowScript != null) playerThrowScript.enabled = true;
                playerThrowScript.SetUIVisible(true);
                break;

            case TurnState.EnemyTurn:
                Debug.Log("TurnManager: EnemyTurn state set");
                if (playerSpawnScript != null) playerSpawnScript.enabled = false;
                if (playerThrowScript != null) playerThrowScript.enabled = false;
                
                if (enemyAIScript != null)
                {
                    enemyAIScript.enabled = true;
                    enemyAIScript.StartTurn();
                }
                playerThrowScript.SetUIVisible(false);
                break;
            case TurnState.End:
                if (playerSpawnScript != null) playerSpawnScript.enabled = false;
                if (playerThrowScript != null) playerThrowScript.enabled = false;
                if (enemyAIScript != null) enemyAIScript.enabled = false;

                playerThrowScript.SetUIVisible(false);
                break;
        }
    }

    public TurnState GetCurrentState() => currentState;
}