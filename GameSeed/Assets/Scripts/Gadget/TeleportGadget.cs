using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gadgets/TeleportGadget")]

public class TeleportGadget : BaseGadget
{
    // bikin canTeleport di player, awalnya false
    public override void Apply(GameObject target)
    {
        PlayerCapabilities playerCap = target.GetComponent<PlayerCapabilities>();
        if(playerCap != null) playerCap.canTeleport = true;
    }

    public override void Remove(GameObject target)
    {
        PlayerCapabilities playerCap = target.GetComponent<PlayerCapabilities>();
        if(playerCap != null) playerCap.canTeleport = false;
    }
}