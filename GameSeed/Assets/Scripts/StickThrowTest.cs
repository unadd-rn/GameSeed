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
    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;
    private bool hasBeenThrown = false;
    private bool interactionStartedOnUI = false;
    private Vector3 debugWorldHitPoint;
    private Vector3 debugSpinAxisX;
    private Vector3 debugSpinAxisY;
    private bool isDebugDataCalculated = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        stickCollider = GetComponent<Collider>();
        mainCamera = Camera.main;
        controls = new PlayerControls();
        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;
    }

    public void OnStickPlaced()
    {
        SetUIVisible(true);
        UpdateSliderPosition();
    }

    void OnEnable()
    {
        // enable buat baca input terus masukin ke event listener
        controls.Player.Enable();
        controls.Player.TouchPress.started += ctx => HandleTouchStart();
        controls.Player.TouchPress.canceled += ctx => HandleTouchEnd();
    }

    void OnDisable()
    {
        // ini gara2 memory leak pas anunya di destroy
        controls.Player.TouchPress.started -= ctx => HandleTouchStart();
        controls.Player.TouchPress.canceled -= ctx => HandleTouchEnd();
        controls.Player.Disable();
    }

    // ini berdua buat guard di hp cuma idk bener atau kagak
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

    private bool IsPointerOverUI()
    {
        // biar kalau mencet ui worldnya ga kepencet
        //kenapa? soalnya kalau ngubah direction 
        // kadang kan lu mencet uinya
        //kek misal lu maw maju terus pencet throw kan UInya di atas
        // eh malah ubah arah, jadi ini buat guard aja
        if (EventSystem.current == null) return false;
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = GetInputScreenPosition();
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
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

    private void SetUIVisible(bool visible)
    {
        if (sliderContainer != null) sliderContainer.SetActive(visible);
    }

    private void UpdateSliderPosition()
    {
        // LU JUGA JELEK
        if (sliderContainer == null) return;
        float directionMult = (throwDirectionZ >= 0) ? -0.5f : 0.5f;
        Vector3 stableForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        if (stableForward == Vector3.zero) stableForward = Vector3.forward;
        
        // gw lupa kenapa valuenya ini?
        // intinya ini ditaro depan atau belakang
        Vector3 sliderOffset = stableForward * (stickData.sliderOffsetY * directionMult);
        sliderContainer.transform.position = transform.position + sliderOffset;
        
        // ini biar UInya ga ke flip walaupun stiknya keflip
        // JANGAN DIUBAHHH INI RUSAK MULU
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
        if (TurnManager.Instance != null && TurnManager.Instance.GetCurrentState() != TurnState.PlayerThrowing)
        {
            return;
        }
        if (interactionStartedOnUI || IsPointerOverUI()) return;
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
                float calculatedHit = throwDirectionZ * (projectionOnStableRight / stickData.stickLength);
                calculatedHit = Mathf.Clamp(calculatedHit, -0.5f, 0.5f);
                Debug.DrawLine(mainCamera.transform.position, hit.point, Color.green, 0.5f);
                if (hitPointSlider != null)
                    hitPointSlider.value = calculatedHit*-1f;
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
        TurnManager.Instance.SetState(TurnState.Waiting);

        // ini slider JELEK itu. 
        // mati pas dilempar
        if (hitPointSlider != null)
            hitPoint = hitPointSlider.value;

        SetUIVisible(false);
        hasBeenThrown = true;

        // reset dulu velocitynya
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        
        // tapi dia tuh kayak lupa mulu kalau dia kinematic
        // ini gw nyala matiin 
        // biar unitynya kek 'oh anjay ini ada physicsnya deng'
        rigid.isKinematic = true;
        rigid.isKinematic = false;

        // mulai ini ke bawah itu pusing banget jadi gw jelasin pelan2

        // 1. calcForce itu kecepatan linear
        //    jadi beneran ke depan doang itu berapa (sumbu z)
        // 2. launchForce itu seberapa kuat jadi nilai 100%nya berapa
        // 3. velocityScale itu 0-1 (0%-100%)
        //    buat ngubah force ngesuaiin velocity
        //    jadi dari launchForce maks kepake berapa buat maju
        float calcForce = stickData.launchForce * stickData.velocityScale;

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

        // ini ditambah aja idk
        Vector3 finalVelocity = forward + upward;

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
        Vector3 flatSpin = spinAxisY * (hitPoint * (stickData.spinScale * 0.15f)); 
        Debug.Log("Throw Direction:" + throwDirectionZ);
        Debug.Log("Hitpoint Strength:" + throwDirectionZ * hitPoint);
        
        rigid.angularVelocity += logRoll + flatSpin;

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
        TurnManager.Instance.SetState(TurnState.EnemyTurn);
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
}