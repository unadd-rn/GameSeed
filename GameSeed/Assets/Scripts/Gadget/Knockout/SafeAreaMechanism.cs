using UnityEngine;

public class SafeAreaMechanism : MonoBehaviour
{
    [Header("References")]
    public PlayerCapabilities playerCap; 
    public GameObject safeAreaPrefab; // Masukkan Prefab yang sudah ditempeli script SafeAreaZone

    public void TryDeploySafeArea()
    {
        if (playerCap != null && playerCap.canActivateSafeArea > 0)
        {
            playerCap.canActivateSafeArea--;

            GameObject zoneObj = Instantiate(safeAreaPrefab, transform.position, Quaternion.identity);

            SafeAreaZone zoneScript = zoneObj.GetComponent<SafeAreaZone>();
            if (zoneScript != null)
            {
                zoneScript.targetTag = gameObject.tag; 
            }
            
            Debug.Log($"{gameObject.name} memasang Safe Area untuk tim: {gameObject.tag}");
        }
    }
}