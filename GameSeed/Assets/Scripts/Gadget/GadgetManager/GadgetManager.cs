using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// basically buat apply si gadgetnya, which means hrs ditaruh di lobby bagian modify?
public class GadgetManager : MonoBehaviour
{
    // ide: sebelum dia main, kalau gadgetOwned udah 10, ingetin dulu dia gabisa ngeclaim stik musuh, jadi gak ribet nampilin inventory lagi
    [Header("Arrays")]
    public GadgetInstance[] gadgetOwned = new GadgetInstance[10]; // maksimal 10
    // public BaseGadget[] gadgetEquipped;
    // kayaknya gadgetEquipped gausah ada soalnya nnti pusing lagi gimana ngapusnya

    [Header("Data")]
    public StickData data;

    [Header("Spawned Reference")]
    public Transform stickBodyTransform;

    /* untuk preview */
    private GameObject previewVisualFront;
    private int currentPreviewSlotIndex = -1;
    private GadgetInstance currentPreviewGadget;

    [Header("Player Reference")]
    public GameObject playerTarget;
    [Header("Testing Purpose Only")]
    [SerializeField] private BaseGadget[] startingGadgets;

    private void Start()
    {
        for (int i = 0; i < startingGadgets.Length; i++)
        {
            if (startingGadgets[i] != null && i < gadgetOwned.Length)
            {
                gadgetOwned[i] = new GadgetInstance(startingGadgets[i]);
            }
        }
    }

    private void SetGadgetScale(GameObject go, BaseGadget gadgetData)
    {
        go.transform.localScale = new Vector3(
            gadgetData.sizeX,
            go.transform.localScale.y,
            gadgetData.sizeZ
        );
    }

    private GameObject CreateGadgetVisual(BaseGadget gadgetData, Transform parent)
    {
        GameObject go = new GameObject(gadgetData.gadgetName);
        go.transform.SetParent(parent);

        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();

        meshFilter.sharedMesh = gadgetData.mesh;
        meshRenderer.sharedMaterial = gadgetData.material;

        return go;
    }

    public void DetachGadget(int slotIndex)
    {
        SlotDefinition frontSlot = data.frontSlots[slotIndex];
        SlotDefinition backSlot = data.backSlots[slotIndex];

        GadgetInstance detachedGadget = frontSlot.occupant;
        if(detachedGadget == null) return;

        if(frontSlot.spawnedVisual != null) Destroy(frontSlot.spawnedVisual);
        if(backSlot.spawnedVisual != null) Destroy(backSlot.spawnedVisual);

        detachedGadget.data.Remove(playerTarget != null ? playerTarget : gameObject);
        detachedGadget.isEquipped = false;
        
        frontSlot.occupant = null;
        backSlot.occupant = null;
        frontSlot.spawnedVisual = null;
        backSlot.spawnedVisual = null;
    }

    public void DetachGadgetbyID(string id)
    {
        for(int i = 0; i < data.frontSlots.Length; i++)
        {
            if(data.frontSlots[i].occupant != null && data.frontSlots[i].occupant.id == id)
            {
                DetachGadget(i);
                return;
            }
        }
        Debug.LogWarning($"ID {id} gak ada di stik");
    }

    public void StartPreviewGadget(GadgetInstance gadget, int startingSlotIndex)
    {
        CancelPreview(); // klo sblmnya preview yg lain

        currentPreviewGadget = gadget;
        Transform parentTransform = stickBodyTransform != null ? stickBodyTransform : transform;

        previewVisualFront = CreateGadgetVisual(gadget.data, parentTransform);
        previewVisualFront.transform.localRotation = Quaternion.identity;
        SetGadgetScale(previewVisualFront, gadget.data);

        UpdatePreviewPosition(startingSlotIndex);
    }

    public void UpdatePreviewPosition(int newSlotIndex)
    {
        if(newSlotIndex < 0 || newSlotIndex >= data.frontSlots.Length) return;
        if(previewVisualFront == null) return;

        currentPreviewSlotIndex = newSlotIndex;

        previewVisualFront.transform.localPosition = data.frontSlots[newSlotIndex].localPosition;
    }

    public void ConfirmPlacement()
    {
        if(currentPreviewGadget == null || currentPreviewSlotIndex == -1) return;
        if(data.frontSlots[currentPreviewSlotIndex].occupant != null)
            DetachGadget(currentPreviewSlotIndex); // bersihin yg sebelumnya
        
        Transform parentTransform = stickBodyTransform == null ? transform : stickBodyTransform;
        SlotDefinition frontSlot = data.frontSlots[currentPreviewSlotIndex];
        SlotDefinition backSlot = data.backSlots[currentPreviewSlotIndex];

        GameObject backVisual = CreateGadgetVisual(currentPreviewGadget.data, parentTransform);
        backVisual.transform.localPosition = backSlot.localPosition;
        backVisual.transform.localRotation = Quaternion.Euler(0, 180f, 0);
        SetGadgetScale(backVisual, currentPreviewGadget.data);

        backSlot.spawnedVisual = backVisual;

        frontSlot.spawnedVisual = previewVisualFront;
        frontSlot.occupant = currentPreviewGadget;
        backSlot.occupant = currentPreviewGadget;

        currentPreviewGadget.data.Apply(playerTarget != null ? playerTarget : gameObject);
        currentPreviewGadget.isEquipped = true;

        previewVisualFront = null;
        currentPreviewGadget = null;
        currentPreviewSlotIndex = -1;
    }

    public void CancelPreview()
    {
        if(previewVisualFront != null)
            Destroy(previewVisualFront);
        
        currentPreviewGadget = null;
        currentPreviewSlotIndex = -1;   
    }

    // public void AttachGadget(GadgetInstance gadget, int slotIndex)
    // {
    //     if(slotIndex < 0 || slotIndex >= data.frontSlots.Length) return;
    //     Transform parentTransform = stickBodyTransform != null ? stickBodyTransform : transform;

    //     SlotDefinition frontSlot = data.frontSlots[slotIndex];
    //     GameObject frontVisual = Instantiate(gadget.data.prefab, parentTransform);

    //     frontVisual.transform.localPosition = frontSlot.localPosition;
    //     frontVisual.transform.localRotation = Quaternion.identity; // bikin dia lagi ngadep ke depan
    //     SetGadgetScale(frontVisual, gadget.data);

    //     SlotDefinition backSlot = data.backSlots[slotIndex];
    //     GameObject backVisual = Instantiate(gadget.data.prefab, parentTransform);

    //     backVisual.transform.localPosition = backSlot.localPosition;
    //     backVisual.transform.localRotation = Quaternion.Euler(0, 180f, 0);
    //     SetGadgetScale(backVisual, gadget.data);

    //     gadget.data.Apply(data);

    //     frontSlot.spawnedVisual = frontVisual;
    //     backSlot.spawnedVisual = backVisual;

    //     frontSlot.occupant = gadget;
    //     backSlot.occupant = gadget;

    //     gadget.isEquipped = true;
    // }
}