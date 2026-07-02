using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportationMechanism : MonoBehaviour
{
    
    [Header("In Range")]
    public float minX = -4.9f; // ini arena in general, mungkin nanti ganti
    public float maxX = 1.9f;
    public float minZ = -0.55f;
    public float maxZ = 4.1f;

    public float teleportYOffset = 0.5f;

    [Header("Limit Arena")]
    public Transform enemy;
    public float minDistanceToEnemy = 0.5f;

    [Header("Teleport Settings")]
    [Tooltip("Duration in seconds that the object stays invisible before reappearing.")]
    public float invisibilityDuration = 1.0f; 
    public GameObject visualModel; // Assign your object's 3D model/graphics here

    // public void Teleport()
    // {  
    //     Vector3 randomPoint;
    //     do
    //     {
    //         float randomX = Random.Range(minX, maxX);
    //         float randomZ = Random.Range(minZ, maxZ);
    //         randomPoint = new Vector3(randomX, 0.1f, randomZ);
    //     } while(TooClose(randomPoint) || IsPointOutOfBounds(randomPoint));
    //     Rigidbody rb = GetComponent<Rigidbody>();
    //     if (rb != null) rb.velocity = Vector3.zero;
    //     transform.position = randomPoint;
    // }

    // public bool TooClose(Vector3 point)
    // {
    //     float dist = Vector3.Distance(point, enemy.position);
    //     return dist < minDistanceToEnemy;
    // }

    // public bool IsPointOutOfBounds(Vector3 point)
    // {
    //     Ray groundLevel = new Ray(new Vector3(point.x, 100f, point.z), Vector3.down);
    //     RaycastHit hit;
    //     if (Physics.Raycast(groundLevel, out hit, 200f))
    //     {
    //         if (hit.collider.CompareTag("OutOfBound"))
    //         {
    //             return true;
    //         }
    //     }
    //     return false;
    // }

    public void Teleport()
    {
        StartCoroutine(TeleportSequence());
        AudioManager.Instance.PlaySFX("TeleportActivate");
    }

    private IEnumerator TeleportSequence()
    {
        Vector3 finalPoint = Vector3.zero;
        bool foundValidSpot = false;
        
        int maxAttempts = 30; 
        int attempts = 0;

        while (!foundValidSpot && attempts < maxAttempts)
        {
            attempts++;
            float randomX = Random.Range(minX, maxX);
            float randomZ = Random.Range(minZ, maxZ);
            
            Vector3 rayStart = new Vector3(randomX, 100f, randomZ);
            
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 200f))
            {
                if (hit.collider.CompareTag("OutOfBound")) continue;

                Vector2 pointXZ = new Vector2(hit.point.x, hit.point.z);
                Vector2 enemyXZ = new Vector2(enemy.position.x, enemy.position.z);
                
                if (Vector2.Distance(pointXZ, enemyXZ) < minDistanceToEnemy) continue;

                finalPoint = hit.point + (Vector3.up * teleportYOffset);
                foundValidSpot = true;
            }
        }

        if (foundValidSpot)
        {
            // 1. DISAPPEAR: Hide the visual model immediately
            if (visualModel != null) visualModel.SetActive(false);

            // 2. MOVE IMMEDIATELY: Relocate physics & position right away while hidden
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) 
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero; 
                rb.position = finalPoint; 
                transform.position = finalPoint; // Sync transform instantly alongside Rigidbody
            }
            else
            {
                transform.position = finalPoint;
            }

            // 3. WAIT: Pause execution at the NEW destination, completely hidden
            yield return new WaitForSeconds(invisibilityDuration);

            // 4. REAPPEAR: Safely turn the visual model back on
            if (visualModel != null) visualModel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Gagal nemu titik yang aman di arena");
        }
    }
}