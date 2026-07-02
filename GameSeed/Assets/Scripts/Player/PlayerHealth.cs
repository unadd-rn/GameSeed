using System;
using UnityEngine;
using UnityEngine.UI; 
using System.Collections; 
using System.Collections.Generic;

public class PlayerHealth : MonoBehaviour
{
    public GameObject DeathUI;
    public float health;

    public float maxHp = 15f;
    
    [Header("UI References")]
    public Image HPos1, HPos2, HPos3, HPos4, HPos5, HPos6, Bar;
    public Color layer1 = Color.white;
    public Color layer2 = Color.blue; 
    public Color layer3 = Color.red;
    public Color layer4 = Color.blue; 
    public Color layer5 = Color.red;

    public static event Action OnPlayerHit;
    public static event Action OnPlayerOutOfBound;

    [Header("Invincibility Settings")]
    public float invincibilityDuration = 0.5f; // Berapa lama stik kebal setelah kena hit (dalam detik)
    private float lastDamageTime = -100f;

    [Header("Respawn Settings")]
    public float pindahLength = 2f; //buat kl ada enemy
    public float checkRadius = 1.5f; // r sensor untuk mengecek musuh
    [SerializeField] private StickSpawn stickSpawn;

    
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rigid;

   // [Header("Effects")]
    // [SerializeField] private HitFlash _hitFlash; 
    private HashSet<Image> flashingBar = new HashSet<Image>();
    // ni buat nnti aja la

    void Start()
    {
        if(DeathUI != null) DeathUI.SetActive(false);
        rigid = GetComponent<Rigidbody>();
        UpdateUI();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // if (collision.gameObject.CompareTag("Enemy"))
        // {
        //     Debug.Log($"[LOG TABRAKAN] Player menabrak: {collision.gameObject.name} dengan Tag: {collision.gameObject.tag}");
        //     if (collision.transform.position.y > transform.position.y)
        //     {
        //         TakeDamage(1f, 'h'); 
        //         Debug.Log("Kena Hit boi");
        //     }
        //     else
        //     {
        //         Debug.Log("nyentuh tp bukan dr atas ble");
        //     }
        // }
        
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

        if (SafeAreaZone.CheckAndConsumeZone(transform.position, "Player"))
        {
            Debug.Log("Damage DIGAGALKAN karena Player berada di dalam Knockout Safe Area!");
            return; // Keluar dari fungsi, darah player TIDAK AKAN berkurang!
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
        Vector3 targetRespawnPos = stickSpawn.spawnPositionPlayer;
        bool isSpaceClear = false;

        // looping buat mastiin tempat respawn benar-benar kosong dari musuh
        int attempts = 0; 
        while (!isSpaceClear && attempts < 50)
        {
            attempts++;
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
        Debug.Log($"Player Health:{health}");
        if(Bar!=null) Bar.gameObject.SetActive(true);//nnti kl dah ada mode invisible masukkin ke if
        // array biar gampang di-looping
        Image[] heartSlots = { HPos1, HPos2, HPos3, HPos4, HPos5, HPos6 };

        for (int i = 0; i < heartSlots.Length; i++)
        {
            if(heartSlots[i]==null) continue;

            float Blayer5 = health - 12f - i*0.5f;
            float Blayer4 = health - 9f - i*0.5f;
            float Blayer3 = health - 6f - i*0.5f;
            float Blayer2 = health - 3f - i*0.5f;
            float Blayer1 = health - i*0.5f;

            if (Blayer5 >= 0.5f)
            {
                heartSlots[i].gameObject.SetActive(true);
                heartSlots[i].color = layer5;
            }
            else if (Blayer4 >= 0.5f)
            {
                heartSlots[i].gameObject.SetActive(true);
                heartSlots[i].color = layer4;
            } 
            else if (Blayer3 >= 0.5f)
            {
                heartSlots[i].gameObject.SetActive(true);
                heartSlots[i].color = layer3;
            } 
            else if (Blayer2 >= 0.5f)
            {
                heartSlots[i].gameObject.SetActive(true);
                heartSlots[i].color = layer2;
            }
            else if (Blayer1 >= 0.5f)
            {
                heartSlots[i].gameObject.SetActive(true);
                heartSlots[i].color = layer1;
            } 
            else 
            {
                if (heartSlots[i].gameObject.activeSelf && !flashingBar.Contains(heartSlots[i]))
                {
                    StartCoroutine(GlitchAndHide(heartSlots[i]));
                }
            }
            
        }
    }

    private IEnumerator GlitchAndHide(Image barImage)
    {
        flashingBar.Add(barImage); 

        barImage.color = Color.white;
        yield return new WaitForSeconds(0.06f);
        barImage.enabled = false; 
        yield return new WaitForSeconds(0.06f);
        barImage.enabled = true; 
        yield return new WaitForSeconds(0.08f);
        barImage.gameObject.SetActive(false);
    
        flashingBar.Remove(barImage); // Lepas tandanya
    }

    private void Die()
    {
        TurnManager.Instance.SetState(TurnState.End);
        PlayerPrefs.SetString("MatchStatus", "match selesai");
        PlayerPrefs.Save();
        // death stuff
        if(DeathUI != null) DeathUI.SetActive(true);
    }
}