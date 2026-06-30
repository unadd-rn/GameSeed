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
                    enemyActiveGadgets.Add(new GadgetInstance(gadgetData));
                    gadgetData.Apply(gameObject);
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
    }

    public bool TryUseGadget()
    {
        if (enemyActiveGadgets.Count > 0 && Random.value <= chanceToUseGadget)
        {
            int randGadgetIdx = Random.Range(0, enemyActiveGadgets.Count);
            GadgetInstance chosenGadget = enemyActiveGadgets[randGadgetIdx];

            Debug.Log($"[EnemyGadgetManager] Musuh memutuskan pakai gadget: {chosenGadget.data.gadgetName}");
            chosenGadget.data.Activate(gameObject);
            enemyActiveGadgets.RemoveAt(randGadgetIdx);

            return true;
        }
        
        return false;
    }
}