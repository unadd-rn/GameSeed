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

    [Header("Final Sequence")]
    [SerializeField] private string finalWinKnot = "TutorialWin";
    [SerializeField] private string finalLoseKnot = "TutorialLose";
    [SerializeField] private string garageSceneName = "Senna - garagetutorial";
    [SerializeField] private string garageTransitionName = "Gelap";

    [Header("Spawn Preview")]
    [SerializeField] private GameObject spawnPreviewObject;

    private bool introFinished = false;
    private bool enemyTurnStarted = false;
    private bool tutorialComplete = false;
    private bool finalSequenceActive = false;

    private void Start()
    {
        dialogueManager.OnDialogueEnd += OnDialogueFinished;
        dialogueManager.OnHighlightTag += OnHighlightTag;
        stickSpawn.StickPlaced += OnStickPlaced;
        forceButton.OnForcePressed += OnForcePressed;
        forceButton.OnForceReleased += OnForceReleased;

        dialogueManager.EnterDialogueWithDelay(introKnot, 5f);
    }

    private void OnDialogueFinished()
    {
        if (finalSequenceActive)
        {
            finalSequenceActive = false;
            if (tutorialManager != null) tutorialManager.HideTutorial();
            SceneController.Instance.goToSceneName(garageSceneName, garageTransitionName);
            return;
        }

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
            tutorialComplete = true;
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
        // Tutorial's turn-by-turn scripting is done; fall back to normal turn flow.
        if (tutorialComplete)
        {
            turnManager.SetState(TurnState.EnemyTurn);
            return;
        }

        if (enemyTurnStarted) return;

        dialogueManager.SetWaitForAction(false);
        dialogueManager.ContinueStory();
    }

    private void OnStickPlaced()
    {
        Debug.Log("BattleTutorialDirector: OnStickPlaced called");
        Debug.Log($"dialogueManager null? {dialogueManager == null}");
        Debug.Log($"waitingForAction before: {dialogueManager.IsWaitingForAction}");

        if (spawnPreviewObject != null)
        {
            spawnPreviewObject.SetActive(false);
        }

        tutorialManager.HideTutorial();
        dialogueManager.SetWaitForAction(false);
        Debug.Log($"waitingForAction after: {dialogueManager.IsWaitingForAction}");
        turnManager.SetState(TurnState.PlayerThrowing);
        dialogueManager.ContinueStoryWithDelay(2f);
        Debug.Log("ContinueStoryWithDelay called");
    }

    private void OnForcePressed()
    {
        // Only advance dialogue if it's actually paused waiting on this input.
        if (!dialogueManager.IsWaitingForAction) return;

        dialogueManager.SetWaitForAction(false);
        dialogueManager.ContinueStory();
    }

    private void OnForceReleased()
    {
        if (!dialogueManager.IsWaitingForAction) return;

        dialogueManager.SetWaitForAction(false);
        dialogueManager.ContinueStory();
    }

    public void OnEnemyTurnEnd()
    {
        // Tutorial's turn-by-turn scripting is done; fall back to normal turn flow.
        if (tutorialComplete)
        {
            turnManager.SetState(TurnState.PlayerThrowing);
            return;
        }

        turnManager.SetState(TurnState.PlayerThrowing);
        dialogueManager.EnterDialogue(afterEnemyTurnKnot);
    }

    // Called by PlayerHealth.Die() / EnemyHealth.Win() when isTutorialScene is true,
    // instead of their normal Win/Lose UI + panel animation flow.
    public void PlayFinalSequence(bool playerWon)
    {
        if (finalSequenceActive) return;
        finalSequenceActive = true;

        if (tutorialManager != null) tutorialManager.ShowFullDim();

        string knot = playerWon ? finalWinKnot : finalLoseKnot;
        dialogueManager.EnterDialogue(knot);
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