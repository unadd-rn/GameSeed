using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StickThrowTest : MonoBehaviour
{
    [Header("UI Stuff")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private Slider hitPointSlider;
    [SerializeField] private GameObject sliderContainer;

    [SerializeField] float velocityScale = 1f;
    [SerializeField] float spinScale = 15f;
    private float hitPoint = 0f;
    [SerializeField] float stickLength = 1f;
    private Rigidbody rigid;
    
    [Header("Launch")]
    public float velocityScale = 1f;
    [SerializeField] private float launchForce = 5f;
    [SerializeField] private float up = 2f;

    [Header("Tralala trilili")]
    [SerializeField] private float stickLength = 1f;
    [SerializeField] private float spinScale = 15f;
    [SerializeField] private LayerMask stickLayer;

    [Header("UI Offset")]
    [SerializeField] private float sliderOffsetY = 0.5f;
    private Rigidbody rigid;
    private Collider stickCollider;
    private Camera mainCamera;
    private float hitPoint = 0f;
    private RaycastHit hit;
    private PlayerControls controls;
    private bool isTouching = false;
    private float throwDirectionZ = 1f;
    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;
    private bool hasBeenThrown = false;
    private bool interactionStartedOnUI = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        stickCollider = GetComponent<Collider>();
        mainCamera = Camera.main;
        controls = new PlayerControls();
        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;
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

    private void HandleTouchStart()
    {
        isTouching = true;
        if (IsPointerOverUI())
        {
            interactionStartedOnUI = true;
        }
    }

    private void HandleTouchEnd()
    {
        isTouching = false;
        interactionStartedOnUI = false;
    }

    private bool IsPressingThisFrame()
    {
        #if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            interactionStartedOnUI = IsPointerOverUI();
            return true;
        }
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

        #if UNITY_EDITOR
        return EventSystem.current.IsPointerOverGameObject();
        #elif UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > 0)
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        return EventSystem.current.IsPointerOverGameObject();
        #else
        return EventSystem.current.IsPointerOverGameObject();
        #endif
    }

    void Update()
    {
        if (!hasBeenThrown)
        {
            UpdateSliderPosition();
            if (IsPressingThisFrame() && !interactionStartedOnUI)
            {
                ProcessInput();
            }
        }
    }

    private void SetUIVisible(bool visible)
    {
        if (sliderContainer != null) sliderContainer.SetActive(visible);
    }

    private void UpdateSliderPosition()
    {
        if (sliderContainer == null) return;
        
        float directionMult = (throwDirectionZ >= 0) ? -0.5f : 0.5f;
        Vector3 sliderOffset = transform.forward * (sliderOffsetY * directionMult);
        
        sliderContainer.transform.position = transform.position + sliderOffset;
        sliderContainer.transform.rotation = Quaternion.LookRotation(
            sliderContainer.transform.position - mainCamera.transform.position
        );
    }

    private void ProcessInput()
    {
        if (interactionStartedOnUI || IsPointerOverUI()) return;
        Vector2 screenPosition = GetInputScreenPosition();
        if (float.IsInfinity(screenPosition.x) || float.IsNaN(screenPosition.x) ||
            float.IsInfinity(screenPosition.y) || float.IsNaN(screenPosition.y))
            return;
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        Debug.DrawRay(ray.origin, ray.direction * 20f, Color.yellow, 0.5f);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, stickLayer))
        {
            if (hit.collider == stickCollider)
            {
                Vector3 localHitPoint = transform.InverseTransformPoint(hit.point);
                float calculatedHit = localHitPoint.x / stickLength;
                calculatedHit = Mathf.Clamp(calculatedHit, -0.5f, 0.5f);
                Debug.DrawLine(mainCamera.transform.position, hit.point, Color.green, 0.5f);
                Debug.Log($"[STICK TAP] Hit X-Offset: {calculatedHit}");
                
                if (hitPointSlider != null)
                    hitPointSlider.value = calculatedHit;
                return;
            }
        }
        Plane movementPlane = new Plane(Vector3.up, transform.position);
        float rayDistance;
        if (movementPlane.Raycast(ray, out rayDistance))
        {
            Vector3 targetWorldPoint = ray.GetPoint(rayDistance);
            Vector3 toTap = targetWorldPoint - transform.position;
            float dotForward = Vector3.Dot(toTap, transform.forward);
            throwDirectionZ = dotForward >= 0 ? -1f : 1f;
            Debug.DrawLine(mainCamera.transform.position, targetWorldPoint, Color.red, 0.5f);
            Debug.Log($"[DIRECTION TAP] dot={dotForward:F2} -> {(throwDirectionZ > 0 ? "BACKWARD" : "FORWARD")}");
        }
    }

    public void Throw()
    {
        if (hasBeenThrown) return;

        if (hitPointSlider != null)
            hitPoint = hitPointSlider.value;

        SetUIVisible(false);
        hasBeenThrown = true;

        rigid.isKinematic = false;
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;

        float calcForce = launchForce * velocityScale;

        Vector3 forward = transform.forward * (calcForce * throwDirectionZ);
        Vector3 upward = transform.up * up;
        Vector3 finalVelocity = forward + upward;

        Vector3 localHitOffset = transform.right * (hitPoint * stickLength);
        Vector3 worldHitPoint = transform.position + localHitOffset;

        rigid.AddForceAtPosition(finalVelocity, worldHitPoint, ForceMode.VelocityChange);

        Vector3 logRoll = transform.right * (hitPoint * spinScale);
        Vector3 flatSpin = transform.up * (hitPoint * (spinScale * 0.3f));
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
        if (hitPointSlider != null)
            hitPointSlider.value = 0f;
        hasBeenThrown = false;
        interactionStartedOnUI = false;
        SetUIVisible(true);
    }
}