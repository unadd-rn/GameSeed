using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BambooProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float damageAmount = 1f;
    public float lifeTime = 3f;

    private string shooterTag;

    public void Initialize(string tag)
    {
        shooterTag = tag;
        Destroy(gameObject, lifeTime); 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(shooterTag)) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth pHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (pHealth != null)
            {
                pHealth.TakeDamage(damageAmount, 'h');
            }
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyHealth eHealth = collision.gameObject.GetComponent<EnemyHealth>();
            if (eHealth != null)
            {
                eHealth.TakeDamage(damageAmount, 'h');
            }
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
