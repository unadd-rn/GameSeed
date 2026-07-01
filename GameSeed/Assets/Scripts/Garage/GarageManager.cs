using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions.Must;
using Unity.Mathematics;

public class GarageManager : MonoBehaviour
{
    public static GarageManager Instance;
    public GameObject gadgetPanel;
    public GameObject bodyPanel;
    public StickData data;

    [Header("Kebutuhan button")]
    [SerializeField] Transform gadgetPanelTransform;
    [SerializeField] Transform bodyPanelTransform;

    [Header("Manager lain")]
    // [SerializeField] GadgetManager GadgetManager.Instance;
    // [SerializeField] BodyManager BodyManager.Instance;
    // [SerializeField] SendDataToMatch sendDataToMatch;

    [Header("Confirm Button")]
    [SerializeField] GameObject confirmButtonGadget;
    [SerializeField] GameObject confirmButtonBody;
    [SerializeField] GameObject cancelButton;

    [Header("Text")]
    [SerializeField] private GameObject textField;
    [SerializeField] private TextMeshProUGUI bodyOrGadgetName;
    [SerializeField] private TextMeshProUGUI bodyOrGadgetDesc;

    [Header("Spawning References")]
    public Transform stickSpawnPoint;
    public GameObject stickBodyPrefab; 
    public GameObject stickSlotPrefab;

    private Button[] buttonsG = new Button[10];
    private Button[] buttonsB = new Button[10];

    private StickBody spawnedBody;
    [HideInInspector] public StickSlot spawnedSlot;

    [Header("Slider Gadget")]
    public Slider sliderGadget;

    void Start()
    {
        if (textField != null) textField.SetActive(false);
        if (gadgetPanel != null) gadgetPanel.SetActive(false);
        if (bodyPanel != null) bodyPanel.SetActive(true);
        if (gadgetPanel != null) confirmButtonGadget.SetActive(false);
        if (confirmButtonBody != null) confirmButtonBody.SetActive(false);
        if (cancelButton != null) cancelButton.SetActive(false);
        
        if (gadgetPanel != null) SetupGadgetButtons();
        if (bodyPanel != null) SetupBodyButtons();

        if (textField != null) SpawnStickSystem();

        sliderGadget.onValueChanged.AddListener(OnSliderValueChanged);

        // 1. Gadget Confirm Button
        Button btnGadgetConfirm = confirmButtonGadget.GetComponent<Button>();
        btnGadgetConfirm.onClick.RemoveAllListeners(); // Clear old listeners just in case
        btnGadgetConfirm.onClick.AddListener(() => {
            GadgetManager.Instance.ConfirmPlacement();
            InactivateConfirm();
        });

        // 2. Body Confirm Button
        Button btnBodyConfirm = confirmButtonBody.GetComponent<Button>();
        btnBodyConfirm.onClick.RemoveAllListeners();
        btnBodyConfirm.onClick.AddListener(() => {
            BodyManager.Instance.ConfirmBody();
            InactivateConfirm();
        });

        // 3. Cancel Button
        Button btnCancel = cancelButton.GetComponent<Button>();
        btnCancel.onClick.RemoveAllListeners();
        btnCancel.onClick.AddListener(() => {
            GadgetManager.Instance.CancelPreview();
            BodyManager.Instance.CancelPreviewBody();
            InactivateConfirm();
        });
    }

    void SpawnStickSystem()
    {
        if (stickSpawnPoint == null || stickBodyPrefab == null || stickSlotPrefab == null)
        {
            Debug.LogError("Setup Spawning belum lengkap di Inspector!");
            return;
        }
        

        GameObject bodyGO = Instantiate(stickBodyPrefab, stickSpawnPoint.position, stickSpawnPoint.rotation);
        spawnedBody = bodyGO.GetComponent<StickBody>();

        if(BodyManager.Instance.currentEquippedBody.data == null)
            Debug.LogError("gak nyampe for whatever reason");
        else Debug.Log("aman juga di sini");

        if(BodyManager.Instance.currentEquippedBody != null)
            spawnedBody.ApplyPreview(BodyManager.Instance.currentEquippedBody.data);

        GameObject slotGO = Instantiate(stickSlotPrefab, bodyGO.transform);
        slotGO.transform.localPosition = Vector3.zero;
        slotGO.transform.localRotation = Quaternion.identity;
        spawnedSlot = slotGO.GetComponent<StickSlot>();

        if (BodyManager.Instance != null)
        {
            BodyManager.Instance.stickBody = spawnedBody;
            if (BodyManager.Instance.currentEquippedBody != null)
            {
                spawnedBody.ApplyPreview(BodyManager.Instance.currentEquippedBody.data);
            }
        }
        if (GadgetManager.Instance != null)
        {
            GadgetManager.Instance.stickBodyTransform = bodyGO.transform;
            GadgetManager.Instance.garageManager = this;
        }

        RebuildEquippedGadgets();
    }

    public void BodyButton()
    {
        gadgetPanel.SetActive(false);
        bodyPanel.SetActive(true);
    }

    public void GadgetButton()
    {
        gadgetPanel.SetActive(true);
        bodyPanel.SetActive(false);
    }

    public void BackToMenu()
    {
        // sendDataToMatch.GetData();
        SceneManager.LoadScene("Cet - lobby1");
    }

    public void EmptyingGadgetButtons()
    {
        buttonsG = gadgetPanelTransform.GetComponentsInChildren<Button>(true);
        for(int i = 0; i < buttonsG.Length; i++)
        {
            Button currentButton = buttonsG[i];
            currentButton.image.sprite = null;
            currentButton.onClick.RemoveAllListeners();
        }
    }

    public void EmptyingBodyButtons()
    {
        buttonsB = bodyPanelTransform.GetComponentsInChildren<Button>(true);
        for(int i = 0; i < buttonsB.Length; i++)
        {
            Button currentButton = buttonsB[i];
            currentButton.image.sprite = null;
            currentButton.onClick.RemoveAllListeners();
        }
    }

    public void SetupGadgetButtons()
    {
        buttonsG = gadgetPanelTransform.GetComponentsInChildren<Button>(true);
        for(int i = 0; i < GadgetManager.Instance.gadgetOwned.Length; i++)
        {
            GadgetInstance currentG = GadgetManager.Instance.gadgetOwned[i];
            Button currentButton = buttonsG[i];

            // currentButton.image.sprite = currentG.data.model; // ini bisa dibikin biar dia ambil anak dari button (karena bisa jadi bentuk button beda)
            Image childImage = currentButton.transform.GetChild(0).GetComponent<Image>();
            var tempColor = childImage.color;
            if(i >= GadgetManager.Instance.gadgetOwnedNeff)
            {
                currentButton.interactable = false;
                tempColor.a = 0f;
                childImage.color = tempColor;
                continue;
            }
            currentButton.interactable = true;
            tempColor.a = 1f;
            childImage.color = tempColor;

            if(currentG.data.model != null)
                childImage.sprite = currentG.data.model;

            currentButton.onClick.RemoveAllListeners();
            currentButton.onClick.AddListener(() =>
            {
               GadgetManager.Instance.StartPreviewGadget(currentG, 0);
               confirmButtonGadget.SetActive(true);
               cancelButton.SetActive(true);
               bodyOrGadgetName.text = currentG.data.gadgetName.ToString();
               bodyOrGadgetDesc.text = currentG.data.description.ToString();
               textField.SetActive(true);
            });
        }
    }

    public void SetupBodyButtons()
    {
        // Debug.Log("mulai setup body buttons");
        buttonsB = bodyPanelTransform.GetComponentsInChildren<Button>(true);
        for(int i = 0; i < buttonsB.Length; i++)
        {

            BodyInstance currentB = BodyManager.Instance.bodyOwned[i];
            Button currentButton = buttonsB[i];
            if(currentButton == null)
            {
                Debug.LogError("currentButton null");
                return;
            }

            // currentButton.image.sprite = currentB.data.stickIcon;
            Image currentImage = currentButton.transform.GetChild(0).GetComponent<Image>();
            // Debug.Log("udah masukin currentImage");
            if(currentImage == null)
            {
                Debug.LogError("current image == null");
                return;
            }
            var tempColor = currentImage.color;
            // Debug.Log("udah masukin tempColor");
            if(i >= BodyManager.Instance.bodyOwnedNeff)
            {
                // Debug.Log("i >= BodyManager.Instance.bodyOwnedNeff");
                tempColor.a = 0f;
                currentImage.color = tempColor;
                currentButton.interactable = false;
                continue;
            }
            // Debug.Log("i < BodyManager.Instance.bodyOwnedNeff");
            currentButton.interactable = true;
            tempColor.a = 1f;
            currentImage.color = tempColor;

            currentImage.sprite = currentB.data.stickIcon;
            currentButton.onClick.RemoveAllListeners();
            // Debug.Log("Remove all listener");
            currentButton.onClick.AddListener(() =>
            {
                BodyManager.Instance.PreviewBody(i);
                confirmButtonBody.SetActive(true);
                cancelButton.SetActive(true);
                bodyOrGadgetName.text = currentB.data.stickName.ToString();
                bodyOrGadgetDesc.text = currentB.data.description.ToString();
                textField.SetActive(true);
            });
            // Debug.Log("Dah beres");
        }
    }

    public void RemoveGadgetFromInventory(int slotIndex)
    {
        if (GadgetManager.Instance.gadgetOwned[slotIndex].isEquipped)
        {
            GadgetManager.Instance.DetachGadgetbyID(GadgetManager.Instance.gadgetOwned[slotIndex].id);
        }
        for(int i = GadgetManager.Instance.gadgetOwnedNeff - 1; i > slotIndex; i--)
        {
            GadgetManager.Instance.gadgetOwned[i-1] = GadgetManager.Instance.gadgetOwned[i];
        }
        GadgetManager.Instance.gadgetOwned[GadgetManager.Instance.gadgetOwnedNeff - 1] = null;
        GadgetManager.Instance.gadgetOwnedNeff--;
        EmptyingGadgetButtons();
        SetupRemoveGadgetButtons();
    }

    public void RemoveBodyFromInventory(int slotIndex)
    {
        if(BodyManager.Instance.bodyOwned[slotIndex].data == BodyManager.Instance.def)
        {
            Debug.LogWarning("Can't remove default body!");
            return;
        }
        if(BodyManager.Instance.bodyOwned[slotIndex].isEquipped)
        {
            Debug.LogWarning("Can't remove equipped body!");
            return;
        }

        for(int i = BodyManager.Instance.bodyOwnedNeff - 1; i > slotIndex; i--)
            BodyManager.Instance.bodyOwned[i-1] = BodyManager.Instance.bodyOwned[i];

        BodyManager.Instance.bodyOwned[BodyManager.Instance.bodyOwnedNeff - 1] = null;
        BodyManager.Instance.bodyOwnedNeff--;
        EmptyingBodyButtons();
        SetupRemoveBodyButtons();
    }

    public void InactivateConfirm()
    {
        confirmButtonBody.SetActive(false);
        confirmButtonGadget.SetActive(false);
        cancelButton.SetActive(false);
    }

    public void RemoveState()
    {
        SetupRemoveBodyButtons();
        SetupRemoveGadgetButtons();
    }

    public void NormalState()
    {
        SetupBodyButtons();
        SetupGadgetButtons();
    }

    public void SetupRemoveBodyButtons()
    {
        buttonsB = bodyPanelTransform.GetComponentsInChildren<Button>(true);
        for(int i = 0; i < buttonsB.Length; i++)
        {

            BodyInstance currentB = BodyManager.Instance.bodyOwned[i];
            Button currentButton = buttonsB[i];
            if(currentButton == null)
            {
                Debug.LogError("currentButton null");
                return;
            }

            Image currentImage = currentButton.transform.GetChild(0).GetComponent<Image>();
            if(currentImage == null)
            {
                Debug.LogError("current image == null");
                return;
            }
            var tempColor = currentImage.color;
            if(i >= BodyManager.Instance.bodyOwnedNeff)
            {
                tempColor.a = 0f;
                currentImage.color = tempColor;
                currentButton.interactable = false;
                continue;
            }
            currentButton.interactable = true;
            tempColor.a = 1f;
            currentImage.color = tempColor;

            currentImage.sprite = currentB.data.stickIcon;
            currentButton.onClick.RemoveAllListeners();
            currentButton.onClick.AddListener(() =>
            {
                
                bodyOrGadgetName.text = currentB.data.stickName.ToString();
                bodyOrGadgetDesc.text = currentB.data.description.ToString();
                textField.SetActive(true);
            });
        }
    }

    public void SetupRemoveGadgetButtons()
    {
        buttonsG = gadgetPanelTransform.GetComponentsInChildren<Button>(true);
        for(int i = 0; i < buttonsG.Length; i++)
        {

            GadgetInstance currentG = GadgetManager.Instance.gadgetOwned[i];
            Button currentButton = buttonsG[i];
            if(currentButton == null)
            {
                Debug.LogError("currentButton null");
                return;
            }

            Image currentImage = currentButton.transform.GetChild(0).GetComponent<Image>();
            if(currentImage == null)
            {
                Debug.LogError("current image == null");
                return;
            }
            var tempColor = currentImage.color;
            if(i >= GadgetManager.Instance.gadgetOwnedNeff)
            {
                tempColor.a = 0f;
                currentImage.color = tempColor;
                currentButton.interactable = false;
                continue;
            }
            currentButton.interactable = true;
            tempColor.a = 1f;
            currentImage.color = tempColor;

            currentImage.sprite = currentG.data.model;
            currentButton.onClick.RemoveAllListeners();
            currentButton.onClick.AddListener(() =>
            {
                
                bodyOrGadgetName.text = currentG.data.gadgetName.ToString();
                bodyOrGadgetDesc.text = currentG.data.description.ToString();
                textField.SetActive(true);
            });
        }
    }

    public void SendData()
    {
        if(spawnedBody == null || spawnedSlot == null || data == null) return;

        /* kirim body */
        data.stickName = BodyManager.Instance.currentEquippedBody.data.stickName;
        data.description = BodyManager.Instance.currentEquippedBody.data.description;
        data.stickIcon = BodyManager.Instance.currentEquippedBody.data.stickIcon;
        data.stickMesh = BodyManager.Instance.currentEquippedBody.data.stickMesh;
        data.stickMaterial = BodyManager.Instance.currentEquippedBody.data.stickMaterial;
        data.weight = BodyManager.Instance.currentEquippedBody.data.weight;
        data.damage = BodyManager.Instance.currentEquippedBody.data.damage;
        // data.HP = BodyManager.Instance.currentEquippedBody.data.HP; // hp di mana yah

        /* kirim slot */
        data.frontSlots = spawnedSlot.frontSlots;
        data.backSlots = spawnedSlot.backSlots;
    }

    public void OnSliderValueChanged(float value)
    {
        int curIdx = Mathf.RoundToInt(value);
        GadgetManager.Instance.UpdatePreviewPosition(curIdx);
    }

    public void RebuildEquippedGadgets()
    {
        for (int i = 0; i < GadgetManager.Instance.gadgetOwned.Length; i++)
        {
            GadgetInstance gadget = GadgetManager.Instance.gadgetOwned[i];
            
            if (gadget != null && gadget.isEquipped && gadget.slotIdx != -1)
            {
                GadgetManager.Instance.AttachVisualToSlot(gadget, gadget.slotIdx);
            }
        }
    }
    
}