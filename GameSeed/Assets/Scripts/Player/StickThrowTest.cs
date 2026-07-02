using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class StickThrowTest : MonoBehaviour
{
    [Header("Ini Buat Data")]
    [SerializeField] private StickData stickData;

    [Header("UI Stuff")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private Slider hitPointSlider;
    [SerializeField] private GameObject sliderContainer;
    [SerializeField] private GameObject buttonThrow;
    [SerializeField] private GameObject forceController;
    [SerializeField] private Image forceBar;
    [SerializeField] private GameObject otherUI;
    [SerializeField] private GameObject otherOtherUI;
    [SerializeField] private GameObject buttonSkill;
    [SerializeField] private GameObject buttonHide;
    

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
    private Vector3 debugWorldHitPoint;
    private Vector3 debugSpinAxisX;
    private Vector3 debugSpinAxisY;
    private bool isDebugDataCalculated = false;
    private Coroutine activeResetCoroutine;
    private PortraitAnimator portraitAnimator;
    public event System.Action OnSliderUsed;
    private bool hasUsedSlider = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        stickCollider = GetComponent<Collider>();
        mainCamera = Camera.main;
        controls = new PlayerControls();
        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;
        portraitAnimator = GameObject.Find("Animaton").GetComponent<PortraitAnimator>();
        SetUIVisible(true);
        if (buttonHide != null) buttonHide.SetActive(true);
    }

    public void OnStickPlaced()
    {
        UpdateSliderPosition();
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
        interactionStartedOnUI = false;
    }

    private bool IsPressJustStarted()
    {
        // ge bedain soalnye jelek
        // yang else itu buat yang dihp
        // "TouchPress" itu nama event(?) touchnya lupa apa
        // yang kayak ada map kan terus isinya ada actions
        // TouchPress itu nama actionnya
        #if UNITY_EDITOR
        return Input.GetMouseButtonDown(0);
        #else
        return controls.Player.TouchPress.WasPressedThisFrame();
        #endif
    }

    private bool IsPressing()
    {
        // ini yang nyeret buat slider
        #if UNITY_EDITOR
        return Input.GetMouseButton(0);
        #else
        return isTouching;
        #endif
    }

    private Vector2 GetInputScreenPosition()
    {
        // sama kayak tadi
        // TouchPosition itu action juga di input manager thingy
        // nilainya vector buat ngembaliin posisi ajjh
        #if UNITY_EDITOR
        return Input.mousePosition;
        #else
        return controls.Player.TouchPosition.ReadValue<Vector2>();
        #endif
    }

    private List<RaycastResult> raycastResults = new List<RaycastResult>();
    private bool IsPointerOverUI()
    {
        // biar kalau mencet ui worldnya ga kepencet
        //kenapa? soalnya kalau ngubah direction 
        // kadang kan lu mencet uinya
        //kek misal lu maw maju terus pencet throw kan UInya di atas
        // eh malah ubah arah, jadi ini buat guard aja
        if (EventSystem.current == null) return false;

        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = GetInputScreenPosition();
        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        return raycastResults.Count > 0;
    }

    void Update()
    {
        if (TurnManager.Instance != null && TurnManager.Instance.GetCurrentState() != TurnState.PlayerThrowing)
        {
            return; 
        }

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

    public void SetUIVisible(bool visible)
    {
        if (visible)
        {
            // 1. Enable objects first so they can animate
            SetUIElementsActive(true);
            if (sliderContainer != null) sliderContainer.SetActive(true);

            // 2. Play In animation
            portraitAnimator.PlayEventIn("putStick");
        }
        else
        {
            // 1. Play Out animation while objects are still active
            portraitAnimator.PlayEventOut("putStick");
            
            if (sliderContainer != null) sliderContainer.SetActive(false);

            // 2. Wait for the animation to finish before turning off GameObjects
            StartCoroutine(DisableUIAfterDelay(0.2f)); // Match your longest out-duration (e.g., 0.5s)
        }
    }

    private void SetUIElementsActive(bool visible)
    {
        if (buttonThrow != null) buttonThrow.SetActive(visible);
        if (forceController != null) forceController.SetActive(visible);
        if (forceBar != null) forceBar.fillAmount = 0f;
        if (otherUI != null) otherUI.SetActive(visible);
        if (otherOtherUI != null) otherOtherUI.SetActive(visible);
        if (buttonSkill != null) buttonSkill.SetActive(visible);
    }

    private IEnumerator DisableUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetUIElementsActive(false);
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
        if (TurnManager.Instance != null && TurnManager.Instance.GetCurrentState() != TurnState.PlayerThrowing)
        {
            return;
        }
        Vector2 screenPosition = GetInputScreenPosition();
        if (float.IsInfinity(screenPosition.x) || float.IsNaN(screenPosition.x) ||
            float.IsInfinity(screenPosition.y) || float.IsNaN(screenPosition.y))
            return;
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        Debug.DrawRay(ray.origin, ray.direction * 20f, Color.yellow, 0.5f);
        
        // input jenis A
        // ini buat ngitung si slider intinya
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, stickData.stickLayer))
        {
            if (hit.collider == stickCollider)
            {
                // apa ini? INI BUAT UI KARENA JELEK
                GetStableStickAxes(out Vector3 stableForward, out Vector3 stableRight);

                // ini intinya ngitung jari tuh sejauh apa dari tengah
                Vector3 stickToHitVector = hit.point - transform.position;
                float projectionOnStableRight = Vector3.Dot(stickToHitVector, stableRight);
                float calculatedHit = (projectionOnStableRight / stickData.stickLength);
                calculatedHit = Mathf.Clamp(calculatedHit, -0.5f, 0.5f);
                Debug.DrawLine(mainCamera.transform.position, hit.point, Color.green, 0.5f);
                if (hitPointSlider != null)
                {
                    hitPointSlider.value = calculatedHit*-1f;

                    Debug.Log($"ProcessInput running, state: {TurnManager.Instance.GetCurrentState()}");
                    if (!hasUsedSlider && Mathf.Abs(calculatedHit) > 0.05f)
                    {
                        Debug.Log("OnSliderUsed firing");
                        hasUsedSlider = true;
                        OnSliderUsed?.Invoke();    
                    }
                }
                return;
            }
        }
        
        // input version b 
        // ini yang ngubah arah lemparan
        Plane movementPlane = new Plane(Vector3.up, transform.position);
        float rayDistance;
        if (movementPlane.Raycast(ray, out rayDistance))
        {
            // ini beneran dot product doang sih?
            // idk mau jelasin apa ini tap2 doang
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

        // ini slider JELEK itu. 
        // mati pas dilempar
        if (hitPointSlider != null)
            hitPoint = hitPointSlider.value;

        SetUIVisible(false);
        hasBeenThrown = true;

        // reset dulu velocitynya
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;

        rigid.constraints = RigidbodyConstraints.None;
        
        // tapi dia tuh kayak lupa mulu kalau dia kinematic
        // ini gw nyala matiin 
        // biar unitynya kek 'oh anjay ini ada physicsnya deng'
        rigid.WakeUp();

        // mulai ini ke bawah itu pusing banget jadi gw jelasin pelan2

        // 1. calcForce itu kecepatan linear
        //    jadi beneran ke depan doang itu berapa (sumbu z)
        // 2. launchForce itu seberapa kuat jadi nilai 100%nya berapa
        // 3. velocityScale itu 0-1 (0%-100%)
        //    buat ngubah force ngesuaiin velocity
        //    jadi dari launchForce maks kepake berapa buat maju
        float centerCompensation = Mathf.Lerp(1.2f, 1.0f, Mathf.Abs(hitPoint));
        float calcForce = stickData.launchForce * stickData.velocityScale * centerCompensation;

        // ini buat apa? yap UI JELEK ITU LAGI
        GetStableStickAxes(out Vector3 flatForward, out Vector3 stableRight);
        
        // stay with me
        // 1. forward itu depan, jadi arah ke mana sama seberapa kuat 
        // 2. stableForward itu depan di world, ini sebenernya balik ke UI JELEK ITU
        // dikaliin stableForward biar sesuai UI walau ke flip
        // buat ngubah force ada di launchForce ama velocity
        Vector3 forward = flatForward * (calcForce * throwDirectionZ);

        // nah kalau ini tuh nilai naik
        // tadi tuh maju beneran maju doang
        // jadi biar natural di naik juga yey
        // nilainya nanti gantinya di scriptable objectnya (buat force)
        Vector3 upward = Vector3.up * stickData.up;

        float sideDeflectionPower = 5f;
        Vector3 sideways = stableRight * (hitPoint * sideDeflectionPower * throwDirectionZ);

        // ini ditambah aja idk
        Vector3 finalVelocity = forward + upward + sideways;

        // ini ke bawah ga sepenting itu sih cuma posisi hit
        Vector3 localHitOffset = stableRight * (hitPoint * throwDirectionZ * stickData.stickLength);
        Vector3 worldHitPoint = transform.position + localHitOffset;
        debugWorldHitPoint = worldHitPoint;

        // p.s. ini pake ForceMode.VelocityChange
        // artinya mau ubah mass gimanapun ga ngaruh ke throw
        // kalau mau bikin dia makin 'berat' ubah dragnya aja
        rigid.AddForceAtPosition(finalVelocity, worldHitPoint, ForceMode.VelocityChange);

        // spin biar estetik tapi kayaknya terlalu kuat nanti gw kurangin
        // nanti bilangin yak kalau terlalu spinny
        // kalau pukul di tengah ga spin
        Vector3 spinAxisX = stableRight; 
        Vector3 spinAxisY = Vector3.up;

        debugSpinAxisX = spinAxisX;
        debugSpinAxisY = spinAxisY;
        isDebugDataCalculated = true; 

        Vector3 logRoll = spinAxisX * (stickData.spinScale * stickData.up * (0.06f + (0.4f * hitPoint)));
        Vector3 flatSpin = spinAxisY * (hitPoint * stickData.spinScale * stickData.velocityScale); 
        
        rigid.angularVelocity += logRoll + flatSpin;

        activeResetCoroutine = StartCoroutine(ResetAfterThrow());
    }

    private WaitForSeconds throwWaitInitial = new WaitForSeconds(0.2f);
    private WaitForSeconds throwWaitFinal = new WaitForSeconds(0.5f);

    public void HandleKnockback()
    {
        if (!hasBeenThrown) return;
        if (activeResetCoroutine != null)
        {
            StopCoroutine(activeResetCoroutine);
        }
        activeResetCoroutine = StartCoroutine(WaitForKnockbackToSettle());
    }

    private IEnumerator WaitForKnockbackToSettle()
    {
        yield return new WaitForSeconds(0.1f);
        yield return new WaitUntil(() => rigid.velocity.sqrMagnitude < 0.05f);
        yield return throwWaitFinal; 
        
        ResetStick();
    }

    private IEnumerator ResetAfterThrow()
    {
        yield return throwWaitInitial;
        yield return new WaitUntil(() => rigid.velocity.sqrMagnitude < 0.05f);
        yield return throwWaitFinal;
        ResetStick();
    }

    private void ResetStick()
    {
        Debug.Log("ResetStick called, setting EnemyTurn");

        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        
        Vector3 currentEuler = transform.localRotation.eulerAngles;
        
        float rotX = (currentEuler.x > 180) ? currentEuler.x - 360 : currentEuler.x;
        float rotZ = (currentEuler.z > 180) ? currentEuler.z - 360 : currentEuler.z;

        if (Mathf.Abs(Mathf.Abs(rotX) - 90f) < 20f) currentEuler.x = (rotX > 0) ? 180f : 0f;
        if (Mathf.Abs(Mathf.Abs(rotZ) - 90f) < 20f) currentEuler.z = (rotZ > 0) ? 180f : 0f;
        
        rigid.isKinematic = true; 
        transform.localRotation = Quaternion.Euler(currentEuler);
        rigid.isKinematic = false;
        rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        if (hitPointSlider != null) hitPointSlider.value = 0f;
        hasBeenThrown = false;

        BattleTutorialDirector director = FindObjectOfType<BattleTutorialDirector>();
        if (director != null)
        {
            director.OnPlayerThrowComplete();
        }
        else
        {
            TurnManager.Instance.SetState(TurnState.EnemyTurn);
        }
    }

    private void GetStableStickAxes(out Vector3 stableForward, out Vector3 stableRight)
    {
        stableForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        if (stableForward == Vector3.zero) stableForward = Vector3.forward;
        stableRight = Vector3.Cross(Vector3.up, stableForward).normalized;
    }

    private void OnDrawGizmos()
    {
        if (!isDebugDataCalculated) return;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(debugWorldHitPoint, 0.1f);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(debugWorldHitPoint, debugSpinAxisX);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(debugWorldHitPoint, debugSpinAxisY);
    }

    public float GetThrowDirectionZ() 
    {
        return throwDirectionZ;
    }

    public Vector3 GetStableForward() 
    {
        GetStableStickAxes(out Vector3 stableForward, out Vector3 _);
        return stableForward;
    }
}