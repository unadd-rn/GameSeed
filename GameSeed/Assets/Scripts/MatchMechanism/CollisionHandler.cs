using System.Collections;
using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    private float knockbackForce = 10f; 
    private float collisionCooldown = 2f; // cooldown dalam detik biar gak collision 2 kali
    private static int activeKnockbacks = 0; // karena script dipasang pada 2 object dan spaghetti code

    [Header("Arena")]
    public GameObject ArenaWall;

    [Header("Effects")]
    public GameObject sparkParticlePrefab;

    private Rigidbody rb;
    private Collider col;
    private PlayerHealth playerHealth;
    private bool hasKnockback = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        playerHealth = GetComponent<PlayerHealth>();
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

        Vector3 knockbackDirection = (transform.position - collision.transform.position).normalized; // knockback opposite directions
        knockbackDirection = (knockbackDirection + Vector3.up * 1.5f).normalized; // ini biar keatas juga tapi kayak kurang gitu damn

        if (collision.transform.position.y > transform.position.y){
            PlayerHealth myPlayerHealth = GetComponent<PlayerHealth>();
            EnemyHealth myEnemyHealth = GetComponent<EnemyHealth>();

            if (myPlayerHealth != null) myPlayerHealth.TakeDamage(1f, 'h');
            if (myEnemyHealth != null) myEnemyHealth.TakeDamage(1f, 'h');
        } 
        else
        {
            PlayerHealth otherPlayerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            EnemyHealth otherEnemyHealth = collision.gameObject.GetComponent<EnemyHealth>();

            if (otherPlayerHealth != null) otherPlayerHealth.TakeDamage(1f, 'h');
            if (otherEnemyHealth != null) otherEnemyHealth.TakeDamage(1f, 'h');
        }

        Vector3 dirToMyRb = (transform.position - collision.transform.position).normalized;
        Vector3 dirToOtherRb = (collision.transform.position - transform.position).normalized; // Ini kebalikannya

        dirToMyRb = (dirToMyRb + Vector3.up * 1.5f).normalized;
        dirToOtherRb = (dirToOtherRb + Vector3.up * 1.5f).normalized;

        rb.velocity = Vector3.zero;
        if (otherRb != null) otherRb.velocity = Vector3.zero;

        rb.AddForce(dirToMyRb * knockbackForce, ForceMode.Impulse); // tembak
        if (otherRb != null) otherRb.AddForce(dirToOtherRb * knockbackForce, ForceMode.Impulse); // tembak ke arah berlawanan

        StickThrowTest myPlayerThrow = GetComponent<StickThrowTest>();
        if (myPlayerThrow != null) myPlayerThrow.HandleKnockback();
        else GetComponent<ThrowEnemy>()?.HandleKnockback();

        StickThrowTest otherPlayerThrow = other.GetComponent<StickThrowTest>();
        if (otherPlayerThrow != null) otherPlayerThrow.HandleKnockback();
        else other.GetComponent<ThrowEnemy>()?.HandleKnockback();

        if (ArenaWall != null)
            ArenaWall.SetActive(true); // nyalain wall

        StartCoroutine(KnockbackCooldown(col, collision.collider, other)); // start timer or something
    }

    IEnumerator KnockbackCooldown(Collider a, Collider b, CollisionHandler otherHandler)
    {
        Physics.IgnoreCollision(a, b, true); // matiin collision biar gak nabrak 2 kali
        yield return new WaitForSeconds(collisionCooldown);
        Physics.IgnoreCollision(a, b, false); // nyala lagi setelah 2 detik (cooldown 2f)

        hasKnockback = false;
        if (otherHandler != null) otherHandler.hasKnockback = false;
        activeKnockbacks--;

        if (activeKnockbacks <= 0 && ArenaWall != null)
        {
            activeKnockbacks = 0;
            ArenaWall.SetActive(false); // matiin wall lagi pas knockback udah beres semua
        }
    }
}