using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveDataFromGarage : MonoBehaviour
{
    [Header("Kebutuhan")]
    public StickBody stickBody;
    public StickSlot gadgetSlot;
    public Transform player;
    public Transform stickBodyTransform;
    public GameObject playerGO;

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
        // playerHealth.health = body.data.HP;
    }

    void AttachGadgetInMatch(GadgetInstance gadget, int slotIdx)
    {
        if(slotIdx < 0 || slotIdx >= gadgetSlot.frontSlots.Length) return;
        Transform parentTransform = stickBodyTransform != null ? stickBodyTransform : transform;
        SlotDefinition frontSlot = gadgetSlot.frontSlots[slotIdx];
        SlotDefinition backSlot = gadgetSlot.backSlots[slotIdx];

        GameObject frontVisual = CreateGadgetVisual(gadget.data, parentTransform);
        frontVisual.transform.localPosition = frontSlot.localPosition;
        frontVisual.transform.localRotation = Quaternion.identity;
        SetGadgetScale(frontVisual, gadget.data);

        Vector3 frontWorldPos = parentTransform.TransformPoint(frontSlot.localPosition);
        frontVisual.transform.position = frontWorldPos;
        frontSlot.spawnedVisual = frontVisual;

        GameObject backVisual = CreateGadgetVisual(gadget.data, parentTransform);
        backVisual.transform.localPosition = backSlot.localPosition;
        backVisual.transform.localRotation = Quaternion.Euler(0, 180f, 0);
        SetGadgetScale(backVisual, gadget.data);

        Vector3 backWorldPos = parentTransform.TransformPoint(backSlot.localPosition);
        backVisual.transform.position = backWorldPos;
        backSlot.spawnedVisual = backVisual;

        frontSlot.occupant = gadget;
        backSlot.occupant = gadget;

        gadget.data.Apply(playerGO != null ? playerGO : gameObject);
        // gadget.isEquipped = true;
        // gadget.slotIdx = currentPreviewSlotIndex;
    }

    private void SetGadgetScale(GameObject go, BaseGadget gadgetData)
    {
        go.transform.localScale = new Vector3(
            gadgetData.sizeX,
            gadgetData.sizeY,
            gadgetData.sizeZ
        );
    }

    private GameObject CreateGadgetVisual(BaseGadget gadgetData, Transform parent)
    {
        GameObject go = new GameObject(gadgetData.gadgetName);
        go.transform.SetParent(parent);

        // SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        // sr.sprite = gadgetData.worldSprite;

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = gadgetData.mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = gadgetData.material;
        
        return go;
    }

}
