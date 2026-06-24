using System.Collections;
using UnityEngine;

public enum TurnState { PlayerPlacement, PlayerThrowing, EnemyTurn, Waiting }

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    [Header("References")]
    [SerializeField] private StickSpawn playerSpawnScript;
    [SerializeField] private StickThrowTest playerThrowScript;
    [SerializeField] private EnemyAI enemyAIScript;
    private TurnState currentState;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        SetState(TurnState.PlayerPlacement);
    }

    public void SetState(TurnState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case TurnState.PlayerPlacement:
                if (playerSpawnScript != null) playerSpawnScript.enabled = true;
                if (playerThrowScript != null) playerThrowScript.enabled = true;
                if (enemyAIScript != null) enemyAIScript.enabled = false;
                break;

            case TurnState.PlayerThrowing:
                if (playerThrowScript != null) playerThrowScript.enabled = true;
                playerThrowScript.SetUIVisible(true);
                break;

            case TurnState.EnemyTurn:
                if (playerSpawnScript != null) playerSpawnScript.enabled = false;
                if (playerThrowScript != null) playerThrowScript.enabled = false;
                
                if (enemyAIScript != null)
                {
                    enemyAIScript.enabled = true;
                    enemyAIScript.StartTurn();
                }
                break;
        }
    }

    public TurnState GetCurrentState() => currentState;
}