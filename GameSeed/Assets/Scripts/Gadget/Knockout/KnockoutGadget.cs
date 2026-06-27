using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gadgets/KnockoutGadget")]

public class KnockoutGadget : BaseGadget
{
    public override void Apply(GameObject target)
    {
        PlayerCapabilities cap = target.GetComponent<PlayerCapabilities>();
        
        if (cap != null)
        {
            cap.canActivateSafeArea++;
        }

        SafeAreaMechanism safeAreaMech = target.GetComponent<SafeAreaMechanism>();
        if (safeAreaMech != null)
        {
            safeAreaMech.TryDeploySafeArea();
        }
    }

    public override void Remove(GameObject target)
    {
        PlayerCapabilities cap = target.GetComponent<PlayerCapabilities>();
        
        if (cap != null)
        {
            if (cap.canActivateSafeArea > 0)
                cap.canActivateSafeArea--;
        }
    }
}
