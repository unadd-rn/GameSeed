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
    //private Vector3 startLocalPosition;
    //private Quaternion startLocalRotation;
    private bool hasBeenThrown = false;
    private bool interactionStartedOnUI = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        stickCollider = GetComponent<Collider>();
        mainCamera = Camera.main;
        controls = new PlayerControls();
        //startLocalPosition = transform.localPosition;
        //startLocalRotation = transform.localRotation; 
        // gw pindahin health yh
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

    private void UpdateSliderPosition()
    {
        if (sliderContainer == null) return;
        float directionMult = (throwDirectionZ >= 0) ? -0.5f : 0.5f;
        Vector3 stableForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        if (stableForward == Vector3.zero) stableForward = Vector3.forward;
        Vector3 sliderOffset = stableForward * (stickData.sliderOffsetY * directionMult);
        sliderContainer.transform.position = transform.position + sliderOffset;
        Vector3 desiredUp = (directionMult < 0) ? stableForward : -stableForward;
        Vector3 desiredForward = -transform.up;
        if (desiredForward.y > 0)
        {
            desiredForward = transform.up;
        }
        sliderContainer.transform.rotation = Quaternion.LookRotation(desiredForward, desiredUp);
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
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, stickData.stickLayer))
        {
            if (hit.collider == stickCollider)
            {
                Vector3 stableForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
                if (stableForward == Vector3.zero) stableForward = Vector3.forward;
                Vector3 localHitPoint = transform.InverseTransformPoint(hit.point);
                float calculatedHit = localHitPoint.x / stickData.stickLength;
                calculatedHit = Mathf.Clamp(calculatedHit, -0.5f, 0.5f);
                Debug.DrawLine(mainCamera.transform.position, hit.point, Color.green, 0.5f);
                if (hitPointSlider != null)
                    hitPointSlider.value = calculatedHit*-1f;
                return;
            }
        }
        Plane movementPlane = new Plane(Vector3.up, transform.position);
        float rayDistance;
        if (movementPlane.Raycast(ray, out rayDistance))
        {
            Vector3 targetWorldPoint = ray.GetPoint(rayDistance);
            Vector3 toTap = targetWorldPoint - transform.position;
            Vector3 stableForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            if (stableForward == Vector3.zero) stableForward = Vector3.forward;
            float dotForward = Vector3.Dot(toTap, stableForward);
            throwDirectionZ = dotForward >= 0 ? -1f : 1f;
            Debug.DrawLine(mainCamera.transform.position, targetWorldPoint, Color.red, 0.5f);
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

        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        if (flatForward == Vector3.zero) flatForward = Vector3.forward;

        Vector3 forward = flatForward * (calcForce * throwDirectionZ);
        Vector3 upward = Vector3.up * stickData.up;
        Vector3 finalVelocity = forward + upward;

        Vector3 localHitOffset = transform.right * (hitPoint * stickData.stickLength);
        Vector3 worldHitPoint = transform.position + localHitOffset;

        rigid.AddForceAtPosition(finalVelocity, worldHitPoint, ForceMode.VelocityChange);

        Vector3 spinAxisX = transform.right; 
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