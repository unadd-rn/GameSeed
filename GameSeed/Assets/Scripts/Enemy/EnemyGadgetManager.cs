using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGadgetManager : MonoBehaviour
{
    [Header("Gadget Inventory Setup")]
    [SerializeField] private BaseGadget[] mandatoryGadgets;
    [SerializeField] private BaseGadget[] randomizableGadgetsPool;
    [SerializeField] private bool useRandomGadgets = true;
    [SerializeField] private int minRandomGadgets = 1;
    [SerializeField] private int maxRandomGadgets = 3;
    [SerializeField] [Range(0f, 1f)] private float chanceToUseGadget = 0.3f;

    [Header("Placement Setup (Biar Visual Muncul)")]
    [SerializeField] private StickSlot enemyStickSlot; 
    [SerializeField] private Transform enemyStickBodyTransform; 

    private List<GadgetInstance> enemyActiveGadgets = new List<GadgetInstance>();
    public List<GadgetInstance> GetActiveGadgets()
    {
        return enemyActiveGadgets;
    }

    private void Awake()
    {
        SetupGadgets();
    }

    private void SetupGadgets()
    {
        enemyActiveGadgets.Clear();

        if (mandatoryGadgets != null)
        {
            foreach (var gadgetData in mandatoryGadgets)
            {
                if (gadgetData != null)
                {
                    enemyActiveGadgets.Add(new GadgetInstance(gadgetData));
                    gadgetData.Apply(gameObject);
                }
            }
        }

        if (useRandomGadgets && randomizableGadgetsPool != null && randomizableGadgetsPool.Length > 0)
        {
            int amountToPick = Random.Range(minRandomGadgets, maxRandomGadgets + 1);
            List<BaseGadget> tempPool = new List<BaseGadget>(randomizableGadgetsPool);

            for (int i = 0; i < amountToPick; i++)
            {
                if (tempPool.Count == 0) break;
                
                int randIndex = Random.Range(0, tempPool.Count);
                enemyActiveGadgets.Add(new GadgetInstance(tempPool[randIndex]));
                
                BaseGadget chosenData = tempPool[randIndex];
                chosenData.Apply(gameObject);
                tempPool.RemoveAt(randIndex); 
            }
        }

        RandomizeGadgetPlacement();
    }

    private void RandomizeGadgetPlacement()
    {
        if (enemyStickSlot == null || enemyStickBodyTransform == null)
        {
            Debug.LogWarning("[EnemyGadgetManager] StickSlot atau BodyTransform musuh belum di-assign di Inspector!");
            return;
        }

        List<int> availableSlots = new List<int>();
        for (int i = 0; i < enemyStickSlot.frontSlots.Length; i++)
        {
            availableSlots.Add(i);
        }

        foreach (var gadget in enemyActiveGadgets)
        {
            if (availableSlots.Count == 0) 
            {
                Debug.LogWarning("[EnemyGadgetManager] Slot musuh kepenuhan!");
                break; 
            }

            int randomListIdx = Random.Range(0, availableSlots.Count);
            int chosenSlotIndex = availableSlots[randomListIdx];
            availableSlots.RemoveAt(randomListIdx);

            gadget.slotIdx = chosenSlotIndex;
            gadget.isEquipped = true;

            AttachVisualToEnemySlot(gadget, chosenSlotIndex);
        }
    }

    private void AttachVisualToEnemySlot(GadgetInstance gadget, int slotIndex)
    {
        SlotDefinition frontSlot = enemyStickSlot.frontSlots[slotIndex];
        SlotDefinition backSlot = enemyStickSlot.backSlots[slotIndex];

        GameObject frontVisual = CreateEnemyGadgetVisual(gadget.data, enemyStickBodyTransform);
        frontVisual.transform.localPosition = frontSlot.localPosition;
        
        frontVisual.transform.localRotation = Quaternion.Euler(0f, 0f, 0f); 
        
        SetGadgetScale(frontVisual, gadget.data);
        frontSlot.spawnedVisual = frontVisual;
        frontSlot.occupant = gadget;

        GameObject backVisual = CreateEnemyGadgetVisual(gadget.data, enemyStickBodyTransform);
        backVisual.transform.localPosition = backSlot.localPosition;
        
        backVisual.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); 
        
        SetGadgetScale(backVisual, gadget.data);
        backSlot.spawnedVisual = backVisual;
        backSlot.occupant = gadget;
    }

    private GameObject CreateEnemyGadgetVisual(BaseGadget gadgetData, Transform parent)
    {
        GameObject go = new GameObject(gadgetData.gadgetName + "_Enemy");
        go.transform.SetParent(parent);

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = gadgetData.mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = gadgetData.material;
        
        return go;
    }

    private void SetGadgetScale(GameObject go, BaseGadget gadgetData)
    {
        go.transform.localScale = new Vector3(
            gadgetData.sizeX,
            gadgetData.sizeY,
            gadgetData.sizeZ
        );
    }

    public bool TryUseGadget(bool hasLineOfSight)
    {
        List<GadgetInstance> usableGadgets = new List<GadgetInstance>();
        
        foreach (var gadget in enemyActiveGadgets)
        {
            if (gadget.data is PassiveStatGadget) continue;
            if (!hasLineOfSight && gadget.data is BambooPistolGadget) continue;
            usableGadgets.Add(gadget);
        }

        if (usableGadgets.Count > 0 && Random.value <= chanceToUseGadget)
        {
            int randIdx = Random.Range(0, usableGadgets.Count);
            GadgetInstance chosenGadget = usableGadgets[randIdx];

            Debug.Log($"[EnemyGadgetManager] Musuh memutuskan pakai gadget: {chosenGadget.data.gadgetName}");
            chosenGadget.data.Activate(gameObject);
            
            enemyActiveGadgets.Remove(chosenGadget);
            return true;
        }
        return false;
    }
}