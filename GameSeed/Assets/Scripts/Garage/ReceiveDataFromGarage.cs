using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveDataFromGarage : MonoBehaviour
{
    [Header("Kebutuhan")]
    public StickBody stickBody;
    public StickSlot gadgetSlot;
    public Transform player;

    [Header("Body Stats")]
    public PlayerHealth playerHealth;
    // Start is called before the first frame update
    void Start()
    {
        BodyInstance body = BodyManager.Instance.currentEquippedBody;
        stickBody.stickMesh = body.data.stickMesh;
        stickBody.stickMaterial = body.data.stickMaterial;
        player.GetComponent<Rigidbody>().drag = body.data.weight;
        // ambil data damage

        for(int i = 0; i < GadgetManager.Instance.gadgetOwnedNeff; i++)
        {
            GadgetInstance curGadget = GadgetManager.Instance.gadgetOwned[i];
            if (curGadget.isEquipped)
            {
                AttachGadgetInMatch(curGadget, curGadget.slotIdx);
            }
        }

        playerHealth.health = body.data.HP;
        
    }

    void AttachGadgetInMatch(GadgetInstance gadget, int slotIdx)
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
