using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class EnemyHealth : MonoBehaviour
{
    public GameObject WinUI;
    public float health;
    private int win;
    public float maxHp = 15f;
    
    [Header("UI References")]
    public Image HPos1, HPos2, HPos3, HPos4, HPos5, HPos6, Bar;
    public Color layer1 = Color.white;
    public Color layer2 = Color.blue; 
    public Color layer3 = Color.red;
    public Color layer4 = Color.blue; 
    public Color layer5 = Color.red;

    [Header("Invincibility Settings")]
    public float invincibilityDuration = 0.5f; // Berapa lama stik kebal setelah kena hit (dalam detik)
    private float lastDamageTime = -100f;

    [Header("Respawn Settings")]
    public float pindahLength = 2f; //buat kl ada enemy
    public float checkRadius = 1.5f; // r sensor untuk mengecek musuh
    [SerializeField] private EnemyAI enemyAI;
    
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rigid;

   // [Header("Effects")]
    // [SerializeField] private HitFlash _hitFlash; 
    private HashSet<Image> flashingBar = new HashSet<Image>();
    // ni buat nnti aja la

    [Header("Drop Settings")]
    [SerializeField] private StickBodyENemy enemyBodyScript; 
    [SerializeField] private EnemyGadgetManager enemyGadgetScript;
    [Range(0f, 1f)] public float bodyDropChance = 0.5f; 
    [Range(0f, 1f)] public float gadgetDropChance = 0.4f;
    [SerializeField] private GameObject bodyGetUI;
    private List<BaseGadget> droppableGadgets = new List<BaseGadget>();

    void Start()
    {
        if(WinUI != null) WinUI.SetActive(false);
        rigid = GetComponent<Rigidbody>();
        if (enemyGadgetScript != null)
        {
            List<GadgetInstance> initialGadgets = enemyGadgetScript.GetActiveGadgets();
            if (initialGadgets != null)
            {
                foreach (var gadget in initialGadgets)
                {
                    if (gadget.data != null)
                    {
                        droppableGadgets.Add(gadget.data);
                    }
                }
            }
        }
        UpdateUI();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // if (collision.gameObject.CompareTag("Player"))
        // {
        //     Debug.Log($"[LOG TABRAKAN] Enemy menabrak: {collision.gameObject.name} dengan Tag: {collision.gameObject.tag}");
        //     if (collision.transform.position.y > transform.position.y)
        //     {
        //         TakeDamage(1f, 'h'); 
        //         Debug.Log("Asik kenain demej");
        //     }
        // }
        
        if (collision.gameObject.CompareTag("OutOfBound"))
        {
            TakeDamage(1f, 'o');
            Debug.Log(" Enemy Out Of Bound !!!");
        }
    }

    public void TakeDamage(float amount, char status)
    {
        //ini invinciblenya cuma kalo hit aja gk outofbound cuz takutnya dia jatoh pas masih invicible dn gk bisa respawn....
        if (status != 'o' && Time.time < lastDamageTime + invincibilityDuration)
        {
            return; //ini nih masih invincible
        }

        if (SafeAreaZone.CheckAndConsumeZone(transform.position, "Enemy"))
        {
            Debug.Log("Damage DIGAGALKAN karena Player berada di dalam Knockout Safe Area!");
            return; // Keluar dari fungsi, darah enemy TIDAK AKAN berkurang!
        }

        lastDamageTime = Time.time; //kl ngedamage di luar cooldown

        health -= amount;

        if (status == 'h') 
        {
            // pindahin script knockback disini
        }

        if (status == 'o') 
        {
            RespawnEnemy();
        }

        UpdateUI();      

        if (health <= 0) 
        {
            Debug.Log("hey i win??");
            Win();
            return;
        }
    }

    private void RespawnEnemy()
    {
        Vector3 targetRespawnPos = enemyAI.refEnemyPosition;
        bool isSpaceClear = false;

        // looping buat mastiin tempat respawn benar-benar kosong dari musuh
        int attempts = 0; 
        while (!isSpaceClear && attempts < 50)
        {
            attempts++;
            isSpaceClear = true;
            
            // bikin sensor berbentuk bola untuk mengecek area sekitar
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
        Debug.Log($"Enemy Health:{health}");
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

    private void Win()
    {
        TurnManager.Instance.SetState(TurnState.End);
        PlayerPrefs.SetString("MatchStatus", "match selesai");

        if (!PlayerPrefs.HasKey("WinCount"))
        {
            win = 1;
        }
        else
        {
            win = PlayerPrefs.GetInt("WinCount") + 1;
        }

        PlayerPrefs.SetInt("WinCount", win);

        PlayerPrefs.Save();

        TryDropEnemyBody();
        TryDropEnemyGadget();

        // if(WinUI != null) WinUI.SetActive(true); entah knp gk bisa kl di cek null dl???
        WinUI.SetActive(true);
    }

    private void TryDropEnemyBody()
    {
        if (enemyBodyScript == null || enemyBodyScript.CurrentBodyData == null)
        {
            Debug.Log("BodyScrip null");
            return;
        }

        float roll = Random.Range(0f, 1f);
        if (roll <= bodyDropChance)
        {
            BodyType droppedBody = enemyBodyScript.CurrentBodyData;
            
            if (BodyManager.Instance != null)
            {
                BodyManager.Instance.AddBodyTypeToInventory(droppedBody);
                Debug.Log($"Hoki! {droppedBody.stickName}");
                bodyGetUI.SetActive(true);

            }
            else
            {
                Debug.Log("stickName null");
            }
        }
        else
        {
            Debug.Log("Not hoki musuh tidak menjatuhkan item");
        }
    }


    private void TryDropEnemyGadget()
    {
        if (droppableGadgets == null || droppableGadgets.Count == 0)
        {
            Debug.Log("Active gadget Null - Musuh memang tidak punya gadget dari awal");
            return;
        }

        float roll = Random.Range(0f, 1f);
        if (roll <= gadgetDropChance)
        {
            int randomIndex = Random.Range(0, droppableGadgets.Count);
            BaseGadget droppedGadgetData = droppableGadgets[randomIndex];

            if (droppedGadgetData != null)
            {
                if (GadgetManager.Instance != null)
                    GadgetManager.Instance.AddBaseGadgetToInventory(droppedGadgetData);
            }

            bodyGetUI.SetActive(true);
            Debug.Log($"Hoki! Mendapatkan gadget: {droppedGadgetData.name}");
        }
        else
        {
            Debug.Log("Tidak hoki, no gadget.");
        }
    }
}
