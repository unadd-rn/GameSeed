using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// basically buat apply si gadgetnya, which means hrs ditaruh di lobby bagian modify?
public class GadgetManager : MonoBehaviour
{
    public static GadgetManager Instance;

    // ide: sebelum dia main, kalau gadgetOwned udah 10, ingetin dulu dia gabisa ngeclaim stik musuh, jadi gak ribet nampilin inventory lagi
    public GarageManager garageManager;

    [Header("Arrays")]
    public const int maxGadget = 10;
    public GadgetInstance[] gadgetOwned = new GadgetInstance[maxGadget]; // maksimal 10
    public int gadgetOwnedNeff = 0;

    [HideInInspector] public StickSlot data => garageManager != null ? garageManager.spawnedSlot : null;

    [Header("Spawned Reference")]
    public Transform stickBodyTransform;

    /* untuk preview */
    private GameObject previewVisualFront;
    private int currentPreviewSlotIndex = -1;
    private GadgetInstance currentPreviewGadget;
    private int previewOriginalSlotIdx = -1;

    [Header("Player Reference")]
    public GameObject playerTarget;
    [Header("Testing Purpose Only")]
    [SerializeField] private BaseGadget[] startingGadgets;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        for (int i = 0; i < startingGadgets.Length; i++)
        {
            if (startingGadgets[i] != null && i < gadgetOwned.Length)
            {
                gadgetOwned[i] = new GadgetInstance(startingGadgets[i]);
                gadgetOwned[i].isEquipped = false;
                gadgetOwnedNeff++;
            }
        }

        // for(int i = 0; i < startingGadgets.Length; i++)
        // {
            // if(startingGadgets[i] != null)
                // Debug.Log($"Nama gadget: {startingGadgets[i].gadgedOwned[]}")
        // }
    }

    private void SetGadgetScale(GameObject go, BaseGadget gadgetData)
    {
        go.transform.localScale = new Vector3(
            gadgetData.sizeX,
            // go.transform.localScale.y,
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
        detachedGadget.slotIdx = -1;
        
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
        Debug.LogWarning($"gapunya id {id}");
    }

    public void StartPreviewGadget(GadgetInstance gadget, int startingSlotIndex)
    {
        CancelPreview(); // klo sblmnya preview yg lain

        currentPreviewGadget = gadget;

        previewOriginalSlotIdx = gadget.slotIdx;

        if(previewOriginalSlotIdx != -1)
        {
            SlotDefinition oldfront = garageManager.spawnedSlot.frontSlots[previewOriginalSlotIdx];
            SlotDefinition oldback = garageManager.spawnedSlot.backSlots[previewOriginalSlotIdx];
            if(oldfront.spawnedVisual != null) oldfront.spawnedVisual.SetActive(false);
            if(oldback.spawnedVisual != null) oldback.spawnedVisual.SetActive(false);
            startingSlotIndex = previewOriginalSlotIdx;
        }
        Transform parentTransform = stickBodyTransform != null ? stickBodyTransform : transform;
        
        previewVisualFront = CreateGadgetVisual(gadget.data, parentTransform);
        previewVisualFront.layer = LayerMask.NameToLayer("GadgetPreview");
        // previewVisualFront.transform.localPosition = data.frontSlots[startingSlotIndex].localPosition;
        previewVisualFront.transform.localRotation = Quaternion.identity;
        SetGadgetScale(previewVisualFront, gadget.data);

        // SetVisualAlpha(previewVisualFront, 0.5f); 

        garageManager.sliderGadget.value = startingSlotIndex;
        UpdatePreviewPosition(startingSlotIndex);
    }

    public void UpdatePreviewPosition(int newSlotIndex)
    {
        if(newSlotIndex < 0 || newSlotIndex >= data.frontSlots.Length) return;
        if(previewVisualFront == null) return;

        currentPreviewSlotIndex = newSlotIndex;

        Vector3 worldPos = stickBodyTransform.TransformPoint(data.frontSlots[newSlotIndex].localPosition);
        // worldPos += Camera.main.transform.forward * -0.03f;
        previewVisualFront.transform.position = worldPos;

        // previewVisualFront.transform.localPosition = data.frontSlots[newSlotIndex].localPosition;
        // Debug.Log($"parent: {previewVisualFront.transform.parent.name}");
        // Debug.Log($"parent world pos: {previewVisualFront.transform.parent.position}");
        // Debug.Log($"local pos set: {data.frontSlots[newSlotIndex].localPosition}");
        // Debug.Log($"actual world pos: {previewVisualFront.transform.position}");
    }

    public void ConfirmPlacement()
    {
        if(currentPreviewGadget == null || currentPreviewSlotIndex == -1) return;
        previewVisualFront.layer = LayerMask.NameToLayer("Default");
        // SetVisualAlpha(previewVisualFront, 1f); 

        if(previewOriginalSlotIdx != -1 && previewOriginalSlotIdx != currentPreviewSlotIndex)
        {
            DetachGadget(previewOriginalSlotIdx);
        }
        else if(previewOriginalSlotIdx != -1 && previewOriginalSlotIdx == currentPreviewSlotIndex)
        {
            SlotDefinition oldfront = garageManager.spawnedSlot.frontSlots[previewOriginalSlotIdx];
            SlotDefinition oldback = garageManager.spawnedSlot.backSlots[previewOriginalSlotIdx];
            if (oldfront.spawnedVisual != null) oldfront.spawnedVisual.SetActive(true);
            if (oldback.spawnedVisual != null) oldback.spawnedVisual.SetActive(true);

            Destroy(previewVisualFront);

            previewVisualFront = null;
            currentPreviewGadget = null;
            currentPreviewSlotIndex = -1;
            previewOriginalSlotIdx = -1;
            return;
        }

        if(data.frontSlots[currentPreviewSlotIndex].occupant != null)
            DetachGadget(currentPreviewSlotIndex); // bersihin yg sebelumnya
        
        Transform parentTransform = stickBodyTransform == null ? transform : stickBodyTransform;
        SlotDefinition frontSlot = data.frontSlots[currentPreviewSlotIndex];
        SlotDefinition backSlot = data.backSlots[currentPreviewSlotIndex];

        GameObject backVisual = CreateGadgetVisual(currentPreviewGadget.data, parentTransform);
        backVisual.transform.localPosition = backSlot.localPosition;
        backVisual.transform.localRotation = Quaternion.Euler(0, 180f, 0);
        SetGadgetScale(backVisual, currentPreviewGadget.data);
        Vector3 backWorldPos = parentTransform.TransformPoint(backSlot.localPosition);
        backVisual.transform.position = backWorldPos;
        backSlot.spawnedVisual = backVisual;

        frontSlot.spawnedVisual = previewVisualFront;
        frontSlot.occupant = currentPreviewGadget;
        backSlot.occupant = currentPreviewGadget;

        currentPreviewGadget.data.Apply(playerTarget != null ? playerTarget : gameObject);
        currentPreviewGadget.isEquipped = true;
        currentPreviewGadget.slotIdx = currentPreviewSlotIndex;

        previewVisualFront = null;
        currentPreviewGadget = null;
        currentPreviewSlotIndex = -1;
        previewOriginalSlotIdx = -1;
    }

    public void CancelPreview()
    {
        if(previewVisualFront != null)
            Destroy(previewVisualFront);
        
        if(previewOriginalSlotIdx != -1)
        {
            SlotDefinition oldfront = garageManager.spawnedSlot.frontSlots[previewOriginalSlotIdx];
            SlotDefinition oldback = garageManager.spawnedSlot.backSlots[previewOriginalSlotIdx];
            if (oldfront.spawnedVisual != null) oldfront.spawnedVisual.SetActive(true);
            if (oldback.spawnedVisual != null) oldback.spawnedVisual.SetActive(true);
        }

        currentPreviewGadget = null;
        currentPreviewSlotIndex = -1;   
    }

    public void AddBaseGadgetToInventory(BaseGadget gadgetData)
    {
        if (gadgetData == null) return;

        if (gadgetOwnedNeff >= maxGadget)
            return;
        GadgetInstance newInstance = new GadgetInstance(gadgetData);
        newInstance.isEquipped = false;
        AddGadgetToInventory(newInstance);
        
        Debug.Log($"Drop nambah gadget {gadgetData.gadgetName} ke inventory player!");
    }

    public void AddGadgetToInventory(GadgetInstance gadget)
    {
        if(gadgetOwnedNeff >= maxGadget)
        {
            Debug.LogWarning("Gadget kebanyakan");
            return;
        }
        gadgetOwned[gadgetOwnedNeff] = gadget;
        gadgetOwnedNeff++;
    }

    public void HandleMatchEndDurability()
    {
        for (int i = gadgetOwned.Length - 1; i >= 0; i--)
        {
            GadgetInstance gadget = gadgetOwned[i];

            if (gadget != null && gadget.isEquipped)
            {
                gadget.currentDurability--;
                Debug.Log($"durability {gameObject.name}: Gadget {gadget.data.gadgetName} berkurang! Sisa: {gadget.currentDurability}");

                if (gadget.currentDurability <= 0)
                {
                    Debug.Log($"durability {gameObject.name}: Gadget {gadget.data.gadgetName} HANCUR karena durability habis!");
                    
                    DetachGadgetbyID(gadget.id);
                    RemoveGadgetAtIndex(i);
                }
            }
        }
    }

    private void RemoveGadgetAtIndex(int index)
    {
        //geser gadget di kanan index ke kiri
        for (int i = index; i < gadgetOwnedNeff - 1; i++)
        {
            gadgetOwned[i] = gadgetOwned[i + 1];
        }
        //baru di apus
        gadgetOwned[gadgetOwnedNeff - 1] = null;
        gadgetOwnedNeff--;
    } //bismillah bismillah bismillah berhasil yaAllah

    // private void SetVisualAlpha(GameObject go, float alpha)
    // {
    //     MeshRenderer mr = go.GetComponent<MeshRenderer>();
    //     if (mr == null) return;

    //     // bikin instance material baru biar gak ngubah asset asli
    //     Material mat = mr.material;
    //     Color c = mat.color;
    //     c.a = alpha;
    //     mat.color = c;
    // }

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