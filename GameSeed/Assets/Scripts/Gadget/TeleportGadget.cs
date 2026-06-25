using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gadgets/TeleportGadget")]

public class TeleportGadget : BaseGadget
{
    // bikin canTeleport di player, awalnya false
    public override void Apply(StickData target)
    {
        // PlayerCapabilities playerCap = target.GetComponent<PlayerCapabilities>();
        // if(playerCap != null) playerCap.canTeleport = true;
        target.canTeleport++;
    }

    public override void Remove(StickData target)
    {
        // PlayerCapabilities playerCap = target.GetComponent<PlayerCapabilities>();
        // if(playerCap != null) playerCap.canTeleport = false;
        if(target.canTeleport > 0) target.canTeleport--;
        else Debug.Log("There is no fucking teleport left, wrong call");
    }
}