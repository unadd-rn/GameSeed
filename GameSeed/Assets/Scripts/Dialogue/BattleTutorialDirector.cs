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
    private bool enemyTurnStarted = false;

    private void Start()
    {
        stickSpawn.SetPlacementAllowed(false);

        dialogueManager.OnDialogueEnd += OnDialogueFinished;
        dialogueManager.OnHighlightTag += OnHighlightTag;
        stickSpawn.StickPlaced += OnStickPlaced;
        forceButton.OnForcePressed += OnForcePressed;
        forceButton.OnForceReleased += OnForceReleased;

        dialogueManager.EnterDialogueWithDelay(introKnot, 5f);
    }

    private void OnDialogueFinished()
    {
        if (!introFinished)
        {
            introFinished = true;
            if (tutorialManager != null) tutorialManager.HideTutorial(); 
            if (!enemyTurnStarted)
            {
                enemyTurnStarted = true;
                turnManager.SetState(TurnState.EnemyTurn);
            }
        }
        else
        {
            if (tutorialManager != null) tutorialManager.HideTutorial();
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
            stickSpawn.SetPlacementAllowed(true); // kata admin boleh place
        }
    }

    public void OnPlayerThrowComplete()
    {
        if (enemyTurnStarted) return;

        dialogueManager.SetWaitForAction(false);
        dialogueManager.ContinueStory();
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
        dialogueManager.ContinueStoryWithDelay(2f);
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
        turnManager.SetState(TurnState.PlayerThrowing);
        dialogueManager.EnterDialogue(afterEnemyTurnKnot);
    }

    private void OnDestroy()
    {
        if (tutorialManager != null) tutorialManager.ShowFullDim();
        dialogueManager.OnDialogueEnd -= OnDialogueFinished;
        dialogueManager.OnHighlightTag -= OnHighlightTag;
        stickSpawn.StickPlaced -= OnStickPlaced;
        forceButton.OnForcePressed -= OnForcePressed;
        forceButton.OnForceReleased -= OnForceReleased;
    }
}