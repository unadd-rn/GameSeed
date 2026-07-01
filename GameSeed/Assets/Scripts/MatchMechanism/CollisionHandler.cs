using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CollisionHandler : MonoBehaviour
{
    private float knockbackForce = 10f; 
    private float collisionCooldown = 2f; // cooldown dalam detik biar gak collision 2 kali
    private static int activeKnockbacks = 0; // karena script dipasang pada 2 object dan spaghetti code

    [SerializeField] private PortraitAnimator portraitAnimator;

    [Header("Arena")]
    public GameObject ArenaWall;

    [Header("Effects")]
    public GameObject sparkParticlePrefab;
    [Header("Winning UI Effect")]
    public Image statusImageUI;       // Drag your UI Image component here in the Inspector
    public Sprite playerWinningSprite; // Drag the sprite for when Player wins
    public Sprite enemyWinningSprite;

    private BodyInstance body;
    private Rigidbody rb;
    private Collider col;
    private bool hasKnockback = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        body = BodyManager.Instance.currentEquippedBody;
        
        if (statusImageUI != null)
        {
            statusImageUI.gameObject.SetActive(false);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasKnockback) return; // kalo udah knockback gajadi

        CollisionHandler other = collision.gameObject.GetComponent<CollisionHandler>();
        if (other == null) return; // kalo gaada object lain gajadi

        if (sparkParticlePrefab != null && collision.contacts.Length > 0)
        {
            Vector3 contactPoint = collision.contacts[0].point;
            GameObject spark = Instantiate(sparkParticlePrefab, contactPoint, Quaternion.identity);
            Destroy(spark, 1.5f); 
        }

        Rigidbody otherRb = other.GetComponent<Rigidbody>();

        rb.WakeUp();
        if (otherRb != null) otherRb.WakeUp();

        hasKnockback = true;
        other.hasKnockback = true;
        activeKnockbacks++; 

        bool isOtherHigher = collision.transform.position.y > transform.position.y;

        if (isOtherHigher)
        {
            PlayerHealth myPlayerHealth = GetComponent<PlayerHealth>();
            EnemyHealth myEnemyHealth = GetComponent<EnemyHealth>();

            if (myPlayerHealth != null) myPlayerHealth.TakeDamage(body.data.damage, 'h');
            if (myEnemyHealth != null) myEnemyHealth.TakeDamage(body.data.damage, 'h');

            if (GetComponent<PlayerHealth>() != null) {
                ShowWinningImage(enemyWinningSprite);  
            } else {
                ShowWinningImage(playerWinningSprite); 
            }
        } 
        else
        {
            PlayerHealth otherPlayerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            EnemyHealth otherEnemyHealth = collision.gameObject.GetComponent<EnemyHealth>();

            if (otherPlayerHealth != null) otherPlayerHealth.TakeDamage(body.data.damage, 'h');
            if (otherEnemyHealth != null) otherEnemyHealth.TakeDamage(body.data.damage, 'h');

            if (GetComponent<PlayerHealth>() != null) {
                ShowWinningImage(playerWinningSprite);  
            } else {
                ShowWinningImage(enemyWinningSprite); 
            }
        }

        Vector3 dirToMyRb = (transform.position - collision.transform.position).normalized;
        Vector3 dirToOtherRb = (collision.transform.position - transform.position).normalized; // Ini kebalikannya

        dirToMyRb = (dirToMyRb + Vector3.up * 1.5f).normalized;
        dirToOtherRb = (dirToOtherRb + Vector3.up * 1.5f).normalized;

        rb.velocity = Vector3.zero;
        if (otherRb != null) otherRb.velocity = Vector3.zero;
        Debug.Log("1. Collision Detected! UI shown. Starting 2-second wait...");
        StartCoroutine(PlayAnimationThenKnockback(col, collision.collider, other, otherRb, dirToMyRb, dirToOtherRb));
    }

    private IEnumerator PlayAnimationThenKnockback(Collider a, Collider b, CollisionHandler otherHandler, Rigidbody otherRb, Vector3 dirToMyRb, Vector3 dirToOtherRb)
    {
        Physics.IgnoreCollision(a, b, true); // matiin collision biar gak nabrak 2 kali

        // Wait for the 2-second cut-in/winning animation to finish before applying the forces
        yield return new WaitForSeconds(2.5f);
        Debug.Log("2. 2 Seconds are UP! Applying Damage and Forces NOW.");
        yield return null; // skip satu frame biar physics ke-apply dulu

        rb.AddForce(dirToMyRb * knockbackForce, ForceMode.Impulse); // tembak
        if (otherRb != null) otherRb.AddForce(dirToOtherRb * knockbackForce, ForceMode.Impulse); // tembak ke arah berlawanan

        StickThrowTest myPlayerThrow = GetComponent<StickThrowTest>();
        if (myPlayerThrow != null) myPlayerThrow.HandleKnockback();
        else GetComponent<ThrowEnemy>()?.HandleKnockback();

        StickThrowTest otherPlayerThrow = otherHandler.GetComponent<StickThrowTest>();
        if (otherPlayerThrow != null) otherPlayerThrow.HandleKnockback();
        else otherHandler.GetComponent<ThrowEnemy>()?.HandleKnockback();

        if (ArenaWall != null)
            ArenaWall.SetActive(true); // nyalain wall

        StartCoroutine(KnockbackCooldown(a, b, otherHandler)); // start timer or something
    }

    private void ShowWinningImage(Sprite winningSprite)
    {
        if (statusImageUI != null && winningSprite != null)
        {
            statusImageUI.gameObject.SetActive(true); // Make sure the UI object is visible
            statusImageUI.sprite = winningSprite;     // Swap the image texture
            portraitAnimator.PlayEventIn("cutIn");
            // pause2 detik
            Invoke(nameof(HideWinningImage), 2f);
        }
    }

    private void HideWinningImage()
    {
        portraitAnimator.PlayEventOut("cutIn");
    }

    IEnumerator KnockbackCooldown(Collider a, Collider b, CollisionHandler otherHandler)
    {
        yield return new WaitForSeconds(collisionCooldown);
        Physics.IgnoreCollision(a, b, false); // nyala lagi setelah 2 detik (cooldown 2f)

        hasKnockback = false;
        if (otherHandler != null) otherHandler.hasKnockback = false;
        activeKnockbacks--;

        if (statusImageUI != null)
            statusImageUI.gameObject.SetActive(false);

        if (activeKnockbacks <= 0 && ArenaWall != null)
        {
            activeKnockbacks = 0;
            ArenaWall.SetActive(false); // matiin wall lagi pas knockback udah beres semua
        }
    }
}