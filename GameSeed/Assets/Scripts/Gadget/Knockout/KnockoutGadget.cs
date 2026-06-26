using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gadgets/KnockoutGadget")]

public class KnockoutGadget : BaseGadget
{
    public override void Apply(GameObject target)
    {
        target.canActivateSafeArea++;
    }

    public override void Remove(GameObject target)
    {
        // Mengurangi kuota saat gadget dilepas
        if (target.canActivateSafeArea > 0) 
            target.canActivateSafeArea--;
        else 
            Debug.LogWarning("Tidak ada kuota Safe Area yang bisa dikurangi.");
    }
}