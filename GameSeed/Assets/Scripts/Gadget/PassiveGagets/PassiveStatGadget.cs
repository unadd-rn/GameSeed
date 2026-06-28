using UnityEngine;

[CreateAssetMenu(fileName = "Passive Gadget", menuName = "Gadgets/Passive Stat Gadget")]
public class PassiveStatGadget : BaseGadget
{
    [Header("Stat Modifiers")]
    public float bonusHP;
    public float bonusDamage;

    public override void Apply(GameObject target)
    {
        PlayerCapabilities cap = target.GetComponent<PlayerCapabilities>();
        
        if (cap != null)
        {
            if (gadgetName == "Bandage")
            {
                cap.haveBandage=true;
                Debug.Log("Bandage applied");
            } else if (gadgetName == "Spiky Bamboo")
            {
                cap.haveBandage=true;
                Debug.Log("Spiky Bamboo applied");
            } else if (gadgetName == "Motorcycle Helmet")
            {
                cap.haveMotorcycleHelmet = true;
                Debug.Log("Motorcycle Helmet");
            } else if (gadgetName == "Adrenaline Shot")
            {
                cap.haveAdrenalinShot=true;
                Debug.Log("Adrenaline Shot applied");
            } 
            
        }
    
        PlayerHealth myPlayerHealth = target.GetComponent<PlayerHealth>();
        EnemyHealth myEnemyHealth = target.GetComponent<EnemyHealth>();

        if (myPlayerHealth != null)
        {
            myPlayerHealth.health -= bonusHP;

            if (myPlayerHealth.health > myPlayerHealth.maxHp)
            {
                myPlayerHealth.health = myPlayerHealth.maxHp;
            } else if (myPlayerHealth.health <= 0)
            {
                myPlayerHealth.health = 0;
            }
            
            Debug.Log($"health player rn: {myPlayerHealth.health}");
            // healthplayeh.UpdateUI();
            //prob doesnt need this cuz applide in garage?
        } else if (myEnemyHealth != null)
        {
            myEnemyHealth.health -= bonusHP;

            if (myEnemyHealth.health > myEnemyHealth.maxHp)
            {
                myEnemyHealth.health = myEnemyHealth.maxHp;
            } else if (myEnemyHealth.health <= 0)
            {
                myEnemyHealth.health = 0;
            }
            
            Debug.Log($"health enemy rn: {myEnemyHealth.health}");
        }
    }

    public override void Remove(GameObject target)
    {
                PlayerCapabilities cap = target.GetComponent<PlayerCapabilities>();
        
        if (cap != null)
        {
            if (gadgetName == "Bandage")
            {
                cap.haveBandage=false;
                Debug.Log("Bandage applied");
            } else if (gadgetName == "Spiky Bamboo")
            {
                cap.haveSpikyBamboo=false;
                Debug.Log("Spiky Bamboo applied");
            } else if (gadgetName == "Motorcycle Helmet")
            {
                cap.haveMotorcycleHelmet = false;
                Debug.Log("Motorcycle Helmet");
            } else if (gadgetName == "Adrenaline Shot")
            {
                cap.haveAdrenalinShot=false;
                Debug.Log("Adrenaline Shot applied");
            } 
            
        }

        Debug.Log("passive gadget removed");
        PlayerHealth myPlayerHealth = target.GetComponent<PlayerHealth>();
        EnemyHealth myEnemyHealth = target.GetComponent<EnemyHealth>();

        if (myPlayerHealth != null)
        {
            myPlayerHealth.health -= bonusHP;

            if (myPlayerHealth.health > myPlayerHealth.maxHp)
            {
                myPlayerHealth.health = myPlayerHealth.maxHp;
            } else if (myPlayerHealth.health <= 0)
            {
                myPlayerHealth.health = 0;
            }
            
            Debug.Log($"health player rn: {myPlayerHealth.health}");

        } else if (myEnemyHealth != null)
        {
            myEnemyHealth.health -= bonusHP;

            if (myEnemyHealth.health > myEnemyHealth.maxHp)
            {
                myEnemyHealth.health = myEnemyHealth.maxHp;
            } else if (myEnemyHealth.health <= 0)
            {
                myEnemyHealth.health = 0;
            }
            
            Debug.Log($"health enemy rn: {myEnemyHealth.health}");
        }

        // StickBody body = target.GetComponent<StickBody>();
        // if (body != null)
        // {
        //     body.currentDamage -= bonusDamage;
        // }
    }

    public override void Activate(GameObject target)
    {
        //gk ad
    }
}