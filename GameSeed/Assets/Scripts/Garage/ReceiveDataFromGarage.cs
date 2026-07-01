using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveDataFromGarage : MonoBehaviour
{
    public StickBody stickBody;
    public StickSlot gadgetSlot;
    // Start is called before the first frame update
    void Start()
    {
        stickBody.stickMesh = BodyManager.Instance.currentEquippedBody.data.stickMesh;
        stickBody.stickMaterial = BodyManager.Instance.currentEquippedBody.data.stickMaterial;
        // stickBody.

        for(int i = 0; i < GadgetManager.Instance.gadgetOwnedNeff; i++)
        {
            GadgetInstance curGadget = GadgetManager.Instance.gadgetOwned[i];
            if (curGadget.isEquipped)
            {
                AttachGadgetInMatch(curGadget, curGadget.slotIdx);
            }
        }
    }

    void AttachGadgetInMatch(GadgetInstance gadget, int slotIdx)
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
