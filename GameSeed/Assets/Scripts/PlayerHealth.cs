using System;
using UnityEngine;
using UnityEngine.UI; // Wajib ditambahkan untuk akses komponen Image

public class PlayerHealth : MonoBehaviour
{
    public GameObject DeathUI;
    public float health = 3;
    
    [Header("UI References")]
    public GameObject HPos1, HPos2, HPos3;
    public Sprite fullHeartSprite; 
    public Sprite halfHeartSprite; 
    public Color redColor = Color.red;
    public Color blueColor = Color.blue; // Bisa diubah warnanya di inspector

    public static event Action OnPlayerHit;
    public static event Action OnPlayerOutOfBound;

   // [Header("Effects")]
    // [SerializeField] private HitFlash _hitFlash; 
    // ni buat nnti aja la

    void Start()
    {
        if(DeathUI != null) DeathUI.SetActive(false);
        health = 3;
        UpdateUI();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(1f, 'h'); 
        }

        if (collision.gameObject.CompareTag("OutOfBound"))
        {
            TakeDamage(1f, 'o');
        }
    }

    public void TakeDamage(float amount, char status)
    {
        health -= amount;

        if (status == 'h') 
        {
            OnPlayerHit?.Invoke(); 
            // pindahin script knockback disini
        }

        if (status == 'o') 
        {
            OnPlayerOutOfBound?.Invoke(); 
            // pindahin script respawn disini
        }

        UpdateUI();      

        if (health <= 0) 
        {
            Die();
            return;
        }
    }
    
    void UpdateUI()
    {
        Debug.Log($"Health:{health}");
        
        // array biar gampang di-looping
        GameObject[] heartSlots = { HPos1, HPos2, HPos3 };

        for (int i = 0; i < heartSlots.Length; i++)
        {
            // Ambil komponen Image dari masing-masing GameObject
            Image heartImage = heartSlots[i].GetComponent<Image>();
            
            if (heartImage == null) continue; 

            float blueHealthAmount = health - 3f - i;
            float redHealthAmount = health - i;

            if (blueHealthAmount >= 1f)
            {
                heartSlots[i].SetActive(true);
                heartImage.sprite = fullHeartSprite;
                heartImage.color = blueColor;
            }
            else if (blueHealthAmount == 0.5f)
            {
                heartSlots[i].SetActive(true);
                heartImage.sprite = halfHeartSprite;
                heartImage.color = blueColor;
            }
            else if (redHealthAmount >= 1f)
            {
                heartSlots[i].SetActive(true);
                heartImage.sprite = fullHeartSprite;
                heartImage.color = redColor;
            }
            else if (redHealthAmount == 0.5f)
            {
                heartSlots[i].SetActive(true);
                heartImage.sprite = halfHeartSprite;
                heartImage.color = redColor;
            }
            else
            {
                heartSlots[i].SetActive(false);
            }
        }
    }

    private void Die()
    {

        // death stuff
        if(DeathUI != null) DeathUI.SetActive(true);
    }
}