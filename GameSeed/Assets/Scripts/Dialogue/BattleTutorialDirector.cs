using UnityEngine;

public class BattleTutorialDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private StickSpawn stickSpawn;

    [Header("Ink Knots")]
    [SerializeField] private string introKnot = "battle_intro";
    [SerializeField] private string afterEnemyTurnKnot = "after_enemy_turn";

    [Header("Spawn Preview")]
    [SerializeField] private GameObject spawnPreviewObject;

    private bool introFinished = false;

    private void Start()
    {
        dialogueManager.OnDialogueEnd += OnDialogueFinished;
        dialogueManager.OnHighlightTag += OnHighlightTag;
        stickSpawn.StickPlaced += OnStickPlaced;

        dialogueManager.EnterDialogue(introKnot);
    }

    private void OnDialogueFinished()
    {
        if (!introFinished)
        {
            introFinished = true;
            turnManager.SetState(TurnState.PlayerPlacement);
        }
        else
        {
            // After enemy turn dialogue ends
            // Load next scene, show results, etc.
            Debug.Log("Tutorial complete");
        }
    }

    private void OnHighlightTag(string elementName)
    {
        if (elementName.ToLower() == "spawn")
        {
            if (spawnPreviewObject != null)
            {
                spawnPreviewObject.SetActive(true);
            }
        }
    }

    private void OnStickPlaced()
    {
        if (spawnPreviewObject != null)
        {
            spawnPreviewObject.SetActive(false);
        }
        tutorialManager.HideTutorial();
    }

    public void OnEnemyTurnEnd()
    {
        dialogueManager.EnterDialogue(afterEnemyTurnKnot);
    }

    private void OnDestroy()
    {
        dialogueManager.OnDialogueEnd -= OnDialogueFinished;
        dialogueManager.OnHighlightTag -= OnHighlightTag;
        stickSpawn.StickPlaced -= OnStickPlaced;
    }
}