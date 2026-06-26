using UnityEngine;

public class SafeAreaMechanism : MonoBehaviour
{
    [Header("References")]
    public StickData stickData;       // Hubungkan ke StickData player saat ini
    public GameObject safeAreaPrefab; // Masukkan Prefab yang sudah ditempeli script SafeAreaZone

    void Update()
    {
        // Contoh trigger menggunakan tombol "G" di keyboard (bisa diganti sesuai sistem inputmu)
        if (Input.GetKeyDown(KeyCode.G))
        {
            TryDeploySafeArea();
        }
    }

    public void TryDeploySafeArea()
    {
        if (stickData != null && stickData.canActivateSafeArea > 0)
        {
            GameObject zoneObj = Instantiate(safeAreaPrefab, transform.position, Quaternion.identity);

            SafeAreaZone zoneScript = zoneObj.GetComponent<SafeAreaZone>();
            if (zoneScript != null)
            {
                zoneScript.targetTag = gameObject.tag; // Mengambil tag dari object yg megang script ini
            }
            
            Debug.Log($"{gameObject.name} memasang Safe Area untuk tim: {gameObject.tag}");
        }
    }
}