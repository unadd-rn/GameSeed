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

        if (playerCap != null && playerCap.canShootBambooPistol > 0 && shooter != null && stickSlot != null)
        {
            Transform spawnPoint = GetGadgetTransform(stickSlot);

            if (spawnPoint != null)
            {
                playerCap.canShootBambooPistol--;
                shooter.Shoot(spawnPoint.position, spawnPoint.forward, projectilePrefab, target.tag);
            }
            else
            {
                Debug.LogWarning("Visual Bamboo Pistol tidak ditemukan di StickSlot!");
            }
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