using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class StickSpawn : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider placementAreaCollider;
    [SerializeField] private StickThrowTest throwScript;
    [SerializeField] private bool isTutorialScene = false;

    private Camera mainCamera;
    private PlayerControls controls;
    private bool hasPlaced = false;
    private bool isTouching = false;
    public Vector3 spawnPositionPlayer;
    public System.Action StickPlaced;
    private bool tutorialMode = false;
    private bool placementAllowed = false;

    void Awake()
    {
        mainCamera = Camera.main;
        controls = new PlayerControls();
        if (throwScript != null)
        {
            throwScript.enabled = false;
        }

        if (isTutorialScene) placementAllowed = false;
    }

    void OnEnable()
    {
        controls.Player.Enable();
        controls.Player.TouchPress.started += HandleTouchStart;
        controls.Player.TouchPress.canceled += HandleTouchEnd;
    }

    void OnDisable()
    {
        controls.Player.TouchPress.started -= HandleTouchStart;
        controls.Player.TouchPress.canceled -= HandleTouchEnd;
        controls.Player.Disable();
    }

    // ini berdua buat guard di hp cuma idk bener atau kagak
    private void HandleTouchStart(InputAction.CallbackContext ctx)
    {
        isTouching = true;
    }

    private void HandleTouchEnd(InputAction.CallbackContext ctx)
    {
        isTouching = false;
    }

    void Update()
    {
        if (IsPressJustStarted() && !IsPointerOverUI())
        {
            HandlePlacement();
        }
    }

    public void SetPlacementAllowed(bool allowed) // this is for tutorial placement
    {
        placementAllowed = allowed;
    }

    private void HandlePlacement()
    {
        if (placementAreaCollider == null) return;
        if (!placementAllowed) return; // this is for tutorial placement

        if(!hasPlaced){
            Vector2 screenPosition = GetInputScreenPosition();
            if (float.IsInfinity(screenPosition.x) || float.IsNaN(screenPosition.x) ||
                float.IsInfinity(screenPosition.y) || float.IsNaN(screenPosition.y))
                return;
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit areaHit;
            if (placementAreaCollider.Raycast(ray, out areaHit, Mathf.Infinity))
            {
                transform.position = areaHit.point;
                spawnPositionPlayer = areaHit.point;
                hasPlaced = true;
                AudioManager.Instance.PlaySFX("ButtonPressed");
                this.enabled = false;

                FindObjectOfType<DialogueManager>().IgnoreTapThisFrame();

                Debug.Log($"StickSpawn: placement confirmed, tutorialMode: {tutorialMode}, invoking StickPlaced");
                StickPlaced?.Invoke();
                if (!tutorialMode)
                {
                    TurnManager.Instance.SetState(TurnState.EnemyTurn);
                }
            }
        }

        if (throwScript != null && TurnManager.Instance.GetCurrentState() == TurnState.PlayerThrowing)
        {
            throwScript.enabled = true; 
            throwScript.OnStickPlaced();
        }
    }

    private bool IsPressJustStarted()
    {
        #if UNITY_EDITOR
        return Input.GetMouseButtonDown(0);
        #else
        return controls.Player.TouchPress.WasPressedThisFrame();
        #endif
    }

    private Vector2 GetInputScreenPosition()
    {
        #if UNITY_EDITOR
        return Input.mousePosition;
        #else
        return controls.Player.TouchPosition.ReadValue<Vector2>();
        #endif
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = GetInputScreenPosition();
        System.Collections.Generic.List<RaycastResult> results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}