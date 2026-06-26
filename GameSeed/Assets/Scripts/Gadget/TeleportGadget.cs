using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gadgets/TeleportGadget")]
public class TeleportGadget : BaseGadget
{
    public override void Apply(GameObject target)
    {
        // Cari komponen kapabilitas di tubuh player
        PlayerCapabilities playerCap = target.GetComponent<PlayerCapabilities>();
        if(playerCap != null) 
        {
            playerCap.canTeleport++;
        }
        else 
        {
            Debug.LogWarning("PlayerCapabilities tidak ditemukan di target!");
        }
    }

    public override void Remove(GameObject target)
    {
        PlayerCapabilities playerCap = target.GetComponent<PlayerCapabilities>();
        if(playerCap != null)
        {
            if(playerCap.canTeleport > 0) 
                playerCap.canTeleport--;
            else 
                Debug.LogWarning("Tidak ada sisa teleport untuk dihapus.");
        }
    }
}