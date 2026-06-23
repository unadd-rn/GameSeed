using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StickSpawn : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider placementAreaCollider;
    [SerializeField] private StickThrowTest throwScript;

    private Camera mainCamera;
    private PlayerControls controls;
    private bool hasPlaced = false;
    private bool isTouching = false;

    void Awake()
    {
        mainCamera = Camera.main;
        controls = new PlayerControls();
        if (throwScript != null)
        {
            throwScript.enabled = false;
        }
    }

    void OnEnable()
    {
        controls.Player.Enable();
        controls.Player.TouchPress.started += ctx => isTouching = true;
        controls.Player.TouchPress.canceled += ctx => isTouching = false;
    }

    void OnDisable()
    {
        controls.Player.TouchPress.started -= ctx => isTouching = true;
        controls.Player.TouchPress.canceled -= ctx => isTouching = false;
        controls.Player.Disable();
    }

    void Update()
    {
        if (IsPressJustStarted() && !IsPointerOverUI())
        {
            HandlePlacement();
        }
    }

    private void HandlePlacement()
    {
        if (placementAreaCollider == null) return;

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
                this.enabled = false;
            }
            TurnManager.Instance.SetState(TurnState.EnemyTurn);
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