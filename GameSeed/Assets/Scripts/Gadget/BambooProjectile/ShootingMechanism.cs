using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingMechanism : MonoBehaviour
{
    public float shootForce = 20f;

    public void Shoot(Vector3 spawnPosition, Vector3 direction, GameObject projectilePrefab, string shooterTag)
    {
        if (projectilePrefab == null) 
        {
            Debug.LogWarning("Projectile Prefab kosong!");
            return;
        }

        GameObject bullet = Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(direction));
        
        BambooProjectile projScript = bullet.GetComponent<BambooProjectile>();
        if (projScript != null)
        {
            projScript.Initialize(shooterTag);
        }
        
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = direction * shootForce;
        }
    }
}