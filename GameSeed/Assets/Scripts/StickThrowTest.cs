using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StickThrowTest : MonoBehaviour
{
    [Header("Ini Buat Data")]
    [SerializeField] private StickData stickData;

    [Header("UI Stuff")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private Slider hitPointSlider;
    [SerializeField] private GameObject sliderContainer;

    private Rigidbody rigid;
    private Collider stickCollider;
    private Camera mainCamera;
    private float hitPoint = 0f;
    private RaycastHit hit;
    private PlayerControls controls;
    private bool isTouching = false;
    private float throwDirectionZ = 1f;
    private bool hasBeenThrown = false;
    private bool interactionStartedOnUI = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        stickCollider = GetComponent<Collider>();
        mainCamera = Camera.main;
        controls = new PlayerControls();
    }

    void OnEnable()
    {
        controls.Player.Enable();
        controls.Player.TouchPress.started += ctx => HandleTouchStart();
        controls.Player.TouchPress.canceled += ctx => HandleTouchEnd();
    }

    void OnDisable()
    {
        controls.Player.TouchPress.started -= ctx => HandleTouchStart();
        controls.Player.TouchPress.canceled -= ctx => HandleTouchEnd();
        controls.Player.Disable();
    }

    public void OnStickPlaced()
    {
        SetUIVisible(true);
        UpdateSliderPosition();
    }

    private void HandleTouchStart()
    {
        isTouching = true;
    }

    private void HandleTouchEnd()
    {
        isTouching = false;
        interactionStartedOnUI = false;
    }

    private bool IsPressJustStarted()
    {
        #if UNITY_EDITOR
        return Input.GetMouseButtonDown(0);
        #else
        return controls.Player.TouchPress.WasPressedThisFrame();
        #endif
    }

    private bool IsPressing()
    {
        #if UNITY_EDITOR
        return Input.GetMouseButton(0);
        #else
        return isTouching;
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
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    void Update()
    {
        if (!hasBeenThrown)
        {
            UpdateSliderPosition();
            if (IsPressJustStarted())
            {
                interactionStartedOnUI = IsPointerOverUI();
            }
            if (IsPressing() && !interactionStartedOnUI)
            {
                ProcessInput();
            }
        }
    }

    private void SetUIVisible(bool visible)
    {
        if (sliderContainer != null) sliderContainer.SetActive(visible);
    }

    private void GetStableStickAxes(out Vector3 stableForward, out Vector3 stableRight)
    {
        stableForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        if (stableForward == Vector3.zero) stableForward = Vector3.forward;
        stableRight = Vector3.Cross(Vector3.up, stableForward).normalized;
    }

    private void UpdateSliderPosition()
    {
        if (sliderContainer == null) return;
        GetStableStickAxes(out Vector3 stableForward, out Vector3 stableRight);
        float directionMult = (throwDirectionZ >= 0) ? -0.5f : 0.5f;
        Vector3 sliderOffset = stableForward * (stickData.sliderOffsetY * directionMult);
        sliderContainer.transform.position = transform.position + sliderOffset;
        Vector3 desiredUp = (directionMult < 0) ? stableForward : -stableForward;
        sliderContainer.transform.rotation = Quaternion.LookRotation(Vector3.down, desiredUp);
    }

    private void ProcessInput()
    {
        if (interactionStartedOnUI || IsPointerOverUI()) return;
        Vector2 screenPosition = GetInputScreenPosition();
        if (float.IsInfinity(screenPosition.x) || float.IsNaN(screenPosition.x) ||
            float.IsInfinity(screenPosition.y) || float.IsNaN(screenPosition.y))
            return;

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, stickData.stickLayer))
        {
            if (hit.collider == stickCollider)
            {
                GetStableStickAxes(out Vector3 stableForward, out Vector3 stableRight);
                Vector3 stickToHitVector = hit.point - transform.position;
                float projectionOnStableRight = Vector3.Dot(stickToHitVector, stableRight);
                float calculatedHit = projectionOnStableRight / stickData.stickLength;
                calculatedHit = Mathf.Clamp(calculatedHit, -0.5f, 0.5f);
                if (hitPointSlider != null)
                    hitPointSlider.value = calculatedHit * -1f;
                return;
            }
        }
        Plane movementPlane = new Plane(Vector3.up, transform.position);
        float rayDistance;
        if (movementPlane.Raycast(ray, out rayDistance))
        {
            Vector3 targetWorldPoint = ray.GetPoint(rayDistance);
            Vector3 toTap = targetWorldPoint - transform.position;
            GetStableStickAxes(out Vector3 stableForward, out Vector3 stableRight);
            float dotForward = Vector3.Dot(toTap, stableForward);
            throwDirectionZ = dotForward >= 0 ? -1f : 1f;
        }
    }

    public void Throw()
    {
        if (hasBeenThrown) return;

        if (hitPointSlider != null)
            hitPoint = hitPointSlider.value;

        SetUIVisible(false);
        hasBeenThrown = true;

        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        rigid.isKinematic = true;
        rigid.isKinematic = false;

        float calcForce = stickData.launchForce * stickData.velocityScale;

        GetStableStickAxes(out Vector3 stableForward, out Vector3 stableRight);

        Vector3 forward = stableForward * (calcForce * throwDirectionZ);
        Vector3 upward = Vector3.up * stickData.up;
        Vector3 finalVelocity = forward + upward;

        Vector3 localHitOffset = stableRight * (hitPoint * stickData.stickLength);
        Vector3 worldHitPoint = transform.position + localHitOffset;

        rigid.AddForceAtPosition(finalVelocity, worldHitPoint, ForceMode.VelocityChange);

        Vector3 spinAxisX = stableRight; 
        Vector3 spinAxisY = Vector3.up; 

        Vector3 logRoll = spinAxisX * (hitPoint * (stickData.spinScale * 0.4f));
        Vector3 flatSpin = spinAxisY * (hitPoint * (stickData.spinScale * 0.15f)); 
        
        rigid.angularVelocity = logRoll + flatSpin;

        StartCoroutine(ResetAfterThrow());
    }

    private IEnumerator ResetAfterThrow()
    {
        yield return new WaitForSeconds(0.2f);
        yield return new WaitUntil(() => rigid.velocity.sqrMagnitude < 0.05f);
        yield return new WaitForSeconds(0.5f);
        ResetStick();
    }

    private void ResetStick()
    {
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        rigid.isKinematic = true;
        rigid.isKinematic = false;
        if (hitPointSlider != null)
            hitPointSlider.value = 0f;
        hasBeenThrown = false;
        interactionStartedOnUI = false;
        SetUIVisible(true);
    }
}