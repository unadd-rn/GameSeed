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

    [Header("Limit Arena")]
    public Transform enemy;
    public float minDistanceToEnemy = 0.5f;


    public void Teleport()
    {  
        Vector3 randomPoint;
        do
        {
            float randomX = Random.Range(minX, maxX);
            float randomZ = Random.Range(minZ, maxZ);
            randomPoint = new Vector3(randomX, transform.position.y, randomZ);
        } while(TooClose(randomPoint));
        transform.position = randomPoint;
    }

    public bool TooClose(Vector3 point)
    {
        float dist = Vector3.Distance(point, enemy.position);
        return dist < minDistanceToEnemy;
    }

}
