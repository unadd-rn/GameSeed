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

        AudioManager.Instance.PlaySFX("SlingshotActivate");
    }

    // Tambahkan ini di ShootingMechanism.cs
    private void OnDrawGizmosSelected()
    {
        StickSlot slot = GetComponentInChildren<StickSlot>();
        if (slot == null) return;

        foreach (var front in slot.frontSlots)
        {
            if (front.spawnedVisual != null)
            {
                // Posisi Visual
                Vector3 pos = front.spawnedVisual.transform.position;
                
                // 1. Gambar titik spawn
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(pos, 0.1f);
                
                // 2. Gambar arah FORWARD visual (Warna Kuning)
                // Ini arah yang dipake buat nembak peluru
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(pos, front.spawnedVisual.transform.forward * 0.8f);
                
                // 3. Gambar arah UP visual (Warna Hijau)
                Gizmos.color = Color.green;
                Gizmos.DrawRay(pos, front.spawnedVisual.transform.up * 0.3f);
            }
        }
    }
}