using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gadgets/BackOfRiceGadget")]
public class BagOfRiceGadget : BaseGadget
{
    public float bonusHP;
    public override void Apply(GameObject target)
    {
        PlayerCapabilities cap = target.GetComponent<PlayerCapabilities>();
        
        if (cap != null)
        {
            cap.canActivateRiceOfBag++;
        }
    }

    public override void Remove(GameObject target)
    {
        PlayerCapabilities cap = target.GetComponent<PlayerCapabilities>();
        
        if (cap != null)
        {
            if (cap.canActivateRiceOfBag > 0)
                cap.canActivateRiceOfBag--;
        }
    }

    public override void Activate(GameObject target)
    {
        PlayerHealth myPlayerHealth = target.GetComponent<PlayerHealth>();
        EnemyHealth myEnemyHealth = target.GetComponent<EnemyHealth>();

        if (myPlayerHealth != null)
        {
            myPlayerHealth.health += bonusHP;
        } 
        else if (myEnemyHealth != null)
        {
            myEnemyHealth.health += bonusHP;
        }
        
    }
}
