using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks; //testing aja

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

            // Start the delayed execution process in the background
            StartCoroutine(ExecuteGadgetAfterDelay(chosenGadget, randGadgetIdx));

            // Return true instantly so EnemyAI knows a gadget was successfully triggered
            return true;
        }
        
        return false;
    }

    // This handles the FX, the 2-second wait, and the final activation
    private IEnumerator ExecuteGadgetAfterDelay(GadgetInstance chosenGadget, int gadgetIndex)
    {
        // 1. Play the FX immediately
        if (chosenGadget.data.gadgetName == "Bag Of Rice") {
            Transform characterTransform = this.transform; 
            WorldToScreenFXManager.Instance.PlayFX("Heal", characterTransform);
            yield return new WaitForSeconds(0f);
        }

        if (chosenGadget.data.gadgetName == "nononon") {
            Transform characterTransform = this.transform; 
            WorldToScreenFXManager.Instance.PlayFX("Teleport", characterTransform);
            yield return new WaitForSeconds(0.4f);
        }


        if (chosenGadget.data.gadgetName == "Adrenaline Shot") {
            Transform characterTransform = this.transform; 
            WorldToScreenFXManager.Instance.PlayFX("Power up", characterTransform);
            yield return new WaitForSeconds(0f);
        }

        // 3. Safety Check: Did the enemy die while waiting?
        if (this == null || gameObject == null) yield break;

        // 4. Activate the gadget and clean up the list
        Debug.Log($"[EnemyGadgetManager] Musuh memutuskan pakai gadget: {chosenGadget.data.gadgetName}");
        chosenGadget.data.Activate(gameObject);
        if (chosenGadget.data.gadgetName == "nononon") {
            yield return new WaitForSeconds(0.2f);
            Transform characterTransform = this.transform;
            WorldToScreenFXManager.Instance.PlayFX("Teleport", characterTransform);
        }
        
        // Safety check for list range just in case the list changed during those 2 seconds
        if (gadgetIndex < enemyActiveGadgets.Count) {
            enemyActiveGadgets.RemoveAt(gadgetIndex);
        }
    }
}