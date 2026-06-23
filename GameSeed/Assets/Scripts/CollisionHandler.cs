using System.Collections;
using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    private float knockbackForce = 10f; 
    private float collisionCooldown = 2f; // cooldown dalam detik biar gak collision 2 kali
    private static int activeKnockbacks = 0; // karena script dipasang pada 2 object dan spaghetti code

    [Header("Arena")]
    public GameObject ArenaWall;

    private Rigidbody rb;
    private Collider col;
    private bool hasKnockback = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasKnockback) return; // kalo udah knockback gajadi

        CollisionHandler other = collision.gameObject.GetComponent<CollisionHandler>();
        if (other == null) return; // kalo gaada object lain gajadi

        hasKnockback = true;
        other.hasKnockback = true;
        activeKnockbacks++; 

        Vector3 knockbackDirection = (transform.position - collision.transform.position).normalized; // knockback opposite directions
        knockbackDirection = (knockbackDirection + Vector3.up * 1.5f).normalized; // ini biar keatas juga tapi kayak kurang gitu damn

        Rigidbody otherRb = other.GetComponent<Rigidbody>();

        rb.velocity = Vector3.zero;
        otherRb.velocity = Vector3.zero;

        rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse); // tembak
        otherRb.AddForce(-knockbackDirection * knockbackForce, ForceMode.Impulse); // tembak ke arah berlawanan

        if (ArenaWall != null)
            ArenaWall.SetActive(true); // nyalain wall

        StartCoroutine(KnockbackCooldown(col, collision.collider)); // start timer or something
    }

    IEnumerator KnockbackCooldown(Collider a, Collider b)
    {
        Physics.IgnoreCollision(a, b, true); // matiin collision biar gak nabrak 2 kali
        yield return new WaitForSeconds(collisionCooldown);
        Physics.IgnoreCollision(a, b, false); // nyala lagi setelah 2 detik (cooldown 2f)

        hasKnockback = false;
        activeKnockbacks--;

        if (activeKnockbacks <= 0 && ArenaWall != null)
        {
            activeKnockbacks = 0;
            ArenaWall.SetActive(false); // matiin wall lagi pas knockback udah beres semua
        }
    }
}