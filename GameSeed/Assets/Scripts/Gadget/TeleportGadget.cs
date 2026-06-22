using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gadgets/TeleportGadget")]

public class TeleportGadget : BaseGadget
{
    // bikin canTeleport di player, awalnya false
    public override void Apply(GameObject target)
    {
        // canTeleport = true
    }

    public override void Remove(GameObject target)
    {
        // canTeleport = false
    }
}