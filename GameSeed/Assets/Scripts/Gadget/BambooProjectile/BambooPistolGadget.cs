using UnityEngine;

[CreateAssetMenu(menuName = "Gadgets/BambooPistol")]
public class BambooPistolGadget : BaseGadget
{
    [Header("Bamboo Pistol Settings")]
    public GameObject projectilePrefab;
    public override void Apply(GameObject target)
    {
        PlayerCapabilities playerCap = target.GetComponent<PlayerCapabilities>();
        if (playerCap != null) 
        {
            playerCap.canShootBambooPistol++;
        }
    }

    public override void Remove(GameObject target)
    {
        PlayerCapabilities playerCap = target.GetComponent<PlayerCapabilities>();
        if (playerCap != null && playerCap.canShootBambooPistol > 0)
        {
            playerCap.canShootBambooPistol--;
        }
    }

    public override void Activate(GameObject target)
    {
        PlayerCapabilities playerCap = target.GetComponent<PlayerCapabilities>();
        ShootingMechanism shooter = target.GetComponent<ShootingMechanism>();
        StickSlot stickSlot = target.GetComponentInChildren<StickSlot>(); 

        if (playerCap == null || shooter == null || stickSlot == null) return;

        if (playerCap.canShootBambooPistol > 0)
        {
            Transform spawnPoint = GetGadgetTransform(stickSlot);
            if (spawnPoint == null)
                return;

            playerCap.canShootBambooPistol--; 

            Vector3 correctedForward = spawnPoint.up;
            Vector3 shootDirection = Vector3.ProjectOnPlane(spawnPoint.forward, Vector3.up).normalized;


            if (shootDirection.magnitude < 0.1f) 
                shootDirection = Vector3.ProjectOnPlane(spawnPoint.up, Vector3.up).normalized;

            if (target.CompareTag("Player"))
            {
                StickThrowTest throwTest = target.GetComponent<StickThrowTest>();
                if (throwTest != null)
                {
                    shootDirection = throwTest.GetStableForward() * throwTest.GetThrowDirectionZ();
                }
            }
            else if (target.CompareTag("Enemy"))
            {
                Vector3 stableForward = Vector3.ProjectOnPlane(spawnPoint.forward, Vector3.up).normalized;
                if (stableForward == Vector3.zero) stableForward = Vector3.forward;

                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    Vector3 dirToPlayer = playerObj.transform.position - spawnPoint.position;
                    dirToPlayer.y = 0;
                    
                    float dot = Vector3.Dot(dirToPlayer.normalized, stableForward);
                    
                    shootDirection = dot > 0f ? stableForward : -stableForward;
                }
                else
                {
                    shootDirection = stableForward;
                }
            }
            shootDirection.y = 0;
            if (shootDirection == Vector3.zero) 
            {
                shootDirection = target.transform.forward;
                shootDirection.y = 0; 
            }
            shootDirection.Normalize();

            Vector3 spawnPosition = spawnPoint.position + (shootDirection * 0.5f);

            shooter.Shoot(spawnPosition, shootDirection, projectilePrefab, target.tag);
        }
    }

    private Transform GetGadgetTransform(StickSlot stickSlot)
    {
        foreach (var slot in stickSlot.frontSlots)
        {
            if (slot.occupant != null && slot.occupant.data == this && slot.spawnedVisual != null)
            {
                return slot.spawnedVisual.transform;
            }
        }

        foreach (var slot in stickSlot.backSlots)
        {
            if (slot.occupant != null && slot.occupant.data == this && slot.spawnedVisual != null)
            {
                return slot.spawnedVisual.transform;
            }
        }

        return null;
    }
}