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

    public void OnStickPlaced()
    {
        SetUIVisible(true);
        UpdateSliderPosition();
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
        // ini buat ngitung arahnya karena UINYA JELEK FAK
        stableForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        if (stableForward == Vector3.zero) stableForward = Vector3.forward;
        stableRight = Vector3.Cross(Vector3.up, stableForward).normalized;
    }

    private void UpdateSliderPosition()
    {
        // LU JUGA JELEK
        if (sliderContainer == null) return;
        GetStableStickAxes(out Vector3 stableForward, out Vector3 stableRight);

        //gw lupa kenapa valuenya ini?
        // intinya ini ditaro depan atau belakang
        float directionMult = (throwDirectionZ >= 0) ? -0.5f : 0.5f;
        Vector3 sliderOffset = stableForward * (stickData.sliderOffsetY * directionMult);
        sliderContainer.transform.position = transform.position + sliderOffset;

        // ini biar UInya ga ke flip walaupun stiknya keflip
        // JANGAN DIUBAHHH INI RUSAK MULU
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

        //input jenis A
        //ini buat ngitung si slider intinya
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, stickData.stickLayer))
        {
            if (hit.collider == stickCollider)
            {
                //apa ini? INI BUAT UI KARENA JELEK
                GetStableStickAxes(out Vector3 stableForward, out Vector3 stableRight);

                //ini intinya ngitung jari tuh sejauh apa dari tengah
                Vector3 stickToHitVector = hit.point - transform.position;
                float projectionOnStableRight = Vector3.Dot(stickToHitVector, stableRight);
                float calculatedHit = projectionOnStableRight / stickData.stickLength;
                calculatedHit = Mathf.Clamp(calculatedHit, -0.5f, 0.5f);
                if (hitPointSlider != null)
                    hitPointSlider.value = calculatedHit * -1f;
                return;
            }
        }

        //input version b 
        // ini yang ngubah arah lemparan
        Plane movementPlane = new Plane(Vector3.up, transform.position);
        float rayDistance;
        if (movementPlane.Raycast(ray, out rayDistance))
        {
            // ini beneran dot product doang sih?
            // idk mau jelasin apa ini tap2 doang
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

        // ini slider JELEK itu. 
        // mati pas dilempar
        if (hitPointSlider != null)
            hitPoint = hitPointSlider.value;

        SetUIVisible(false);
        hasBeenThrown = true;

        // reset dulu velocitynya
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;

        // ini yang biar dia kagak berdiri
        // pokoknya yang gw bahas di discord
        // disini gw matiin dulu biar dia bebas maw muter2
        rigid.constraints = RigidbodyConstraints.None;
        
        // ini bego tipis
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
        GetStableStickAxes(out Vector3 stableForward, out Vector3 stableRight);

        // stay with me
        // 1. forward itu depan, jadi arah ke mana sama seberapa kuat 
        // 2. stableForward itu depan di world, ini sebenernya balik ke UI JELEK ITU
        // dikaliin stableForward biar sesuai UI walau ke flip
        // buat ngubah force ada di launchForce ama velocity
        Vector3 forward = stableForward * (calcForce * throwDirectionZ);

        // nah kalau ini tuh nilai naik
        // tadi tuh maju beneran maju doang
        // jadi biar natural di naik juga yey
        // nilainya nanti gantinya di scriptable objectnya (buat force)
        Vector3 upward = Vector3.up * stickData.up;

        // ini ditambah aja idk
        Vector3 finalVelocity = forward + upward;

        //ini ke bawah ga sepenting itu sih cuma posisi hit
        Vector3 localHitOffset = stableRight * (hitPoint * stickData.stickLength);
        Vector3 worldHitPoint = transform.position + localHitOffset;

        // p.s. ini pake ForceMode.VelocityChange
        // artinya mau ubah mass gimanapun ga ngaruh ke throw
        // kalau mau bikin dia makin 'berat' ubah dragnya aja
        rigid.AddForceAtPosition(finalVelocity, worldHitPoint, ForceMode.VelocityChange);

        //spin biar estetik tapi kayaknya terlalu kuat nanti gw kurangin
        // nanti bilangin yak kalau terlalu spinny
        // kalau pukul di tengah ga spin
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
        
        // 1. Get the current angles it landed with
        Vector3 currentEuler = transform.localRotation.eulerAngles;
        
        // Normalize angles so they sit cleanly between -180 and 180 degrees
        float rotX = (currentEuler.x > 180) ? currentEuler.x - 360 : currentEuler.x;
        float rotZ = (currentEuler.z > 180) ? currentEuler.z - 360 : currentEuler.z;

        // 2. CHECK FOR EDGE BALANCING: 
        // If it's balancing near 90 or -90 degrees on its side, force it flat to 0 or 180.
        // If it's already flat (near 0 or 180), this code does absolutely nothing!
        if (Mathf.Abs(Mathf.Abs(rotX) - 90f) < 20f) currentEuler.x = (rotX > 0) ? 180f : 0f;
        if (Mathf.Abs(Mathf.Abs(rotZ) - 90f) < 20f) currentEuler.z = (rotZ > 0) ? 180f : 0f;
        
        // 3. Apply the rotation shift safely
        rigid.isKinematic = true;
        transform.localRotation = Quaternion.Euler(currentEuler);
        rigid.isKinematic = false;

        // 4. Freeze X and Z axes so the player can use the slider without it wobbling
        rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        
        if (hitPointSlider != null) hitPointSlider.value = 0f;
        hasBeenThrown = false;
        interactionStartedOnUI = false;
        SetUIVisible(true);
    }
}