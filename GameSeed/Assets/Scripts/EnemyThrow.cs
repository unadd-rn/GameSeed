using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThrowEnemy : MonoBehaviour
{
    [Header("Ini Buat Data")]
    [SerializeField] private StickData stickData;

    [Header("UI Stuff")]
    [SerializeField] private Slider hitPointSlider;
    [SerializeField] private GameObject sliderContainer;

    private Rigidbody rigid;
    private Collider stickCollider;
    private float hitPoint = 0f;
    private float throwDirectionZ = 1f;
    private bool hasBeenThrown = false;

    private WaitForSeconds throwWaitInitial;
    private WaitForSeconds throwWaitFinal;
    

    public StickData StickDataRef
    {
        get { return stickData; }
    }

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        stickCollider = GetComponent<Collider>();
        
        throwWaitInitial = new WaitForSeconds(0.2f);
        throwWaitFinal = new WaitForSeconds(0.5f);
    }

    public void OnStickPlaced()
    {
        SetUIVisible(true);
        UpdateSliderPosition();
    }

    void Update()
    {
        if (!hasBeenThrown)
        {
            UpdateSliderPosition();
        }
    }

    private void SetUIVisible(bool visible)
    {
        if (sliderContainer != null) sliderContainer.SetActive(visible);
    }

    public void GetStableStickAxes(out Vector3 stableForward, out Vector3 stableRight)
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

    public void SetAIHitPoint(float targetValue)
    {
        if (hasBeenThrown) return;
        
        float clampedValue = Mathf.Clamp(targetValue, -0.5f, 0.5f);
        if (hitPointSlider != null)
        {
            hitPointSlider.value = clampedValue * -1f; 
        }
    }

    public void SetAIThrowDirection(float directionZ)
    {
        if (hasBeenThrown) return;
        throwDirectionZ = directionZ >= 0 ? 1f : -1f;
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

        rigid.constraints = RigidbodyConstraints.None;
        
        // ini bego tipis
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
        Vector3 localHitOffset = stableRight * (hitPoint * throwDirectionZ * stickData.stickLength);
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

        Vector3 logRoll = spinAxisX * (stickData.spinScale * stickData.up * (0.06f + (0.4f * hitPoint)));
        Vector3 flatSpin = spinAxisY * (hitPoint * (stickData.spinScale * 0.15f)); 
        
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
        TurnManager.Instance.SetState(TurnState.PlayerThrowing);
    }
}