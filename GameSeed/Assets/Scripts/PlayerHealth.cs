using System;
using UnityEngine;
using UnityEngine.UI; // Wajib ditambahkan untuk akses komponen Image

public class PlayerHealth : MonoBehaviour
{
    public GameObject DeathUI;
    public float health = 3;
    
    [Header("UI References")]
    public Image HPos1, HPos2, HPos3, HPos4, HPos5, HPos6, Bar;
    public Color redColor = Color.red;
    public Color blueColor = Color.blue; // Bisa diubah warnanya di inspector

    public static event Action OnPlayerHit;
    public static event Action OnPlayerOutOfBound;

    [Header("Invincibility Settings")]
    public float invincibilityDuration = 0.5f; // Berapa lama stik kebal setelah kena hit (dalam detik)
    private float lastDamageTime = -100f;

    [Header("Respawn Settings")]
    public float pindahLength = 2f; //buat kl ada enemy
    public float checkRadius = 1.5f; // Radius bola sensor untuk mengecek musuh
    
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rigid;

   // [Header("Effects")]
    // [SerializeField] private HitFlash _hitFlash; 
    // ni buat nnti aja la

    void Start()
    {
        if(DeathUI != null) DeathUI.SetActive(false);
        health = 3;
        startPosition = transform.position;
        startRotation = transform.rotation;
        rigid = GetComponent<Rigidbody>();
        UpdateUI();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log($"[LOG TABRAKAN] Player menabrak: {collision.gameObject.name} dengan Tag: {collision.gameObject.tag}");
            if (collision.transform.position.y > transform.position.y)
            {
                TakeDamage(1f, 'h'); 
                Debug.Log("Kena Hit boi");
            }
            else
            {
                Debug.Log("nyentuh tp bukan dr atas ble");
            }
        }
        
        if (collision.gameObject.CompareTag("OutOfBound"))
        {
            TakeDamage(1f, 'o');
            Debug.Log("Out Of Bound !!!");
        }
    }

    public void TakeDamage(float amount, char status)
    {
        //ini invinciblenya cuma kalo hit aja gk outofbound cuz takutnya dia jatoh pas masih invicible dn gk bisa respawn....
        if (status != 'o' && Time.time < lastDamageTime + invincibilityDuration)
        {
            return; //ini nih masih invincible
        }
        lastDamageTime = Time.time; //kl ngedamage di luar cooldown

        health -= amount;

        if (status == 'h') 
        {
            OnPlayerHit?.Invoke(); 
            // pindahin script knockback disini
        }

        if (status == 'o') 
        {
            OnPlayerOutOfBound?.Invoke(); 
            RespawnPlayer();
        }

        UpdateUI();      

        if (health <= 0) 
        {
            Die();
            return;
        }
    }

    private void RespawnPlayer()
    {
        Vector3 targetRespawnPos = startPosition;
        bool isSpaceClear = false;

        // looping buat mastiin tempat respawn benar-benar kosong dari musuh
        while (!isSpaceClear)
        {
            isSpaceClear = true;
            
            // Bikin sensor berbentuk bola untuk mengecek area sekitar
            Collider[] hitColliders = Physics.OverlapSphere(targetRespawnPos, checkRadius);
            
            foreach (Collider hit in hitColliders)
            {
                if (hit.CompareTag("Enemy"))
                {
                    // kl ada musuh di dorong ke z positif
                    targetRespawnPos += startRotation * Vector3.forward * pindahLength;
                    isSpaceClear = false; // Karena digeser, harus dicek ulang
                    break;
                }
            }
        }

        transform.position = targetRespawnPos;
        transform.rotation = startRotation;

        // jg jg kl blm velocity 0 biar balik 0 lg
        if (rigid != null)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }
    
    void UpdateUI()
    {
        // Debug.Log($"Health:{health}");
        if(Bar!=null) Bar.gameObject.SetActive(true);//nnti kl dah ada mode invisible masukkin ke if
        // array biar gampang di-looping
        Image[] heartSlots = { HPos1, HPos2, HPos3, HPos4, HPos5, HPos6 };

        for (int i = 0; i < heartSlots.Length; i++)
        {
            if(heartSlots[i]==null) continue;

            float blueHealthAmount = health - 3f - i*0.5f;
            float redHealthAmount = health - i*0.5f;

            if (blueHealthAmount >= 0.5f)
            {
                heartSlots[i].gameObject.SetActive(true);
                heartSlots[i].color = blueColor;
            }
            else if (redHealthAmount >= 0.5f)
            {
                heartSlots[i].gameObject.SetActive(true);
                heartSlots[i].color = redColor;
            }
            else
            {
                heartSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private void Die()
    {

        // death stuff
        if(DeathUI != null) DeathUI.SetActive(true);
    }
}