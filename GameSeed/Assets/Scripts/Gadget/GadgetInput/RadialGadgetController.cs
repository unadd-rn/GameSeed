using UnityEngine;
using System.Collections.Generic;

public class RadialGadgetController : MonoBehaviour
{
    [Header("References")]
    public GadgetManager gadgetManager;
    public RadialMenu radialMenu;

    [Header("Prefabs")]
    public GameObject arcGadgetPrefab;
    public Transform menuParent;

    private List<ArcGadget> spawnedGadgetUI = new List<ArcGadget>();

    void Start()
    {
        Invoke(nameof(PopulateRadialMenu), 0.1f); 
    }

    public void PopulateRadialMenu()
    {
        radialMenu.ClearMenu();
        spawnedGadgetUI.Clear();

        if (gadgetManager == null || arcGadgetPrefab == null) return;

        foreach (GadgetInstance gadget in gadgetManager.gadgetOwned)
        {
            if (gadget == null) 
            {
                continue; 
            }
            if (!gadget.data.isActiveGadget) 
            {
                continue; 
            }

            Debug.Log($"MELAHIRKAN UI UNTUK: {gadget.data.gadgetName}");
            if (!gadget.isEquipped) continue;

            if (!gadget.data.isActiveGadget) continue; 

            GameObject go = Instantiate(arcGadgetPrefab, menuParent != null ? menuParent : radialMenu.transform);
            ArcGadget uiScript = go.GetComponent<ArcGadget>();

            if (uiScript != null)
            {
                uiScript.Init(gadget);
                
                radialMenu.items.Add(uiScript.rectTransform);
                spawnedGadgetUI.Add(uiScript);
            }
        }

        if (radialMenu.items.Count > 0)
        {
            radialMenu.SelectItem(0); // Set item pertama aktif
            UpdateUIHighlight();
        }
    }

    public void UpdateUIHighlight()
    {
        int activeIdx = radialMenu.GetActiveIndex();

        for (int i = 0; i < spawnedGadgetUI.Count; i++)
        {
            spawnedGadgetUI[i].SetActiveSlot(i == activeIdx);
        }
    }

    public GadgetInstance GetSelectedGadget()
    {
        int activeIdx = radialMenu.GetActiveIndex();
        if (activeIdx >= 0 && activeIdx < spawnedGadgetUI.Count)
        {
            return spawnedGadgetUI[activeIdx].Instance;
        }
        return null;
    }
}