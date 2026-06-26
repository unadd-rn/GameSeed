using System.Collections.Generic;
using UnityEngine;

public class SafeAreaZone : MonoBehaviour
{
    [Header("Settings")]
    public float safeRadius = 3f; 
    [HideInInspector] public string targetTag;
    public static List<SafeAreaZone> ActiveZones = new List<SafeAreaZone>();

    private void OnEnable()
    {
        ActiveZones.Add(this);
    }

    private void OnDisable()
    {
        ActiveZones.Remove(this);
    }

    /// <summary>
    /// Fungsi statis untuk mengecek apakah player berada di dalam salah satu Safe Area yang aktif.
    /// Jika iya, zona tersebut akan langsung hancur (hanya menyelamatkan 1 kali).
    /// </summary>
    public static bool CheckAndConsumeZone(Vector3 position, string askerTag)
    {
        for (int i = 0; i < ActiveZones.Count; i++)
        {
            if (ActiveZones[i] == null) continue;

            if (ActiveZones[i].targetTag == askerTag)
            {
                float distance = Vector3.Distance(ActiveZones[i].transform.position, position);
                if (distance <= ActiveZones[i].safeRadius)
                {
                    Destroy(ActiveZones[i].gameObject);
                    return true; // Damage digagalkan!
                }
            }
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.15f); // Hijau transparan
        Gizmos.DrawSphere(transform.position, safeRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, safeRadius);
    }
}