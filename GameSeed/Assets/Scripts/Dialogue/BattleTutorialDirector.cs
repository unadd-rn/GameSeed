using UnityEngine;

public class BattleTutorialDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private StickSpawn stickSpawn;
    [SerializeField] private StickThrowTest stickThrowTest;
    [SerializeField] private ForceButtonHold forceButton;

    [Header("Ink Knots")]
    [SerializeField] private string introKnot = "Tutorial";
    [SerializeField] private string afterEnemyTurnKnot = "FirstTurn";

    [Header("Spawn Preview")]
    [SerializeField] private GameObject spawnPreviewObject;

    private bool introFinished = false;

    private void Start()
    {
        stickSpawn.SetTutorialMode(true);

        dialogueManager.OnDialogueEnd += OnDialogueFinished;
        dialogueManager.OnHighlightTag += OnHighlightTag;
        stickSpawn.StickPlaced += OnStickPlaced;
        forceButton.OnForcePressed += OnForcePressed;
        forceButton.OnForceReleased += OnForceReleased;

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
        Debug.Log("BattleTutorialDirector: OnStickPlaced called");

        if (spawnPreviewObject != null)
        {
            spawnPreviewObject.SetActive(false);
        }

        tutorialManager.HideTutorial();
        dialogueManager.SetWaitForAction(false);
        turnManager.SetState(TurnState.PlayerThrowing);
        dialogueManager.ContinueStory();
    }

    private void OnForcePressed()
    {
        dialogueManager.SetWaitForAction(false);
        dialogueManager.ContinueStory();
    }

    private void OnForceReleased()
    {
        dialogueManager.SetWaitForAction(false);
        dialogueManager.ContinueStory();
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
        forceButton.OnForcePressed -= OnForcePressed;
        forceButton.OnForceReleased -= OnForceReleased;
    }
}