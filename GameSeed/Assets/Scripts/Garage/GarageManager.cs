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
    [SerializeField] GadgetManager gadgetManager;
    [SerializeField] BodyManager bodyManager;
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
        textField.SetActive(false);
        gadgetPanel.SetActive(false);
        bodyPanel.SetActive(true);
        confirmButtonGadget.SetActive(false);
        confirmButtonBody.SetActive(false);
        cancelButton.SetActive(false);
        
        SetupGadgetButtons();
        SetupBodyButtons();

        SpawnStickSystem();

        sliderGadget.onValueChanged.AddListener(OnSliderValueChanged);
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

        if(bodyManager.currentEquippedBody.data == null)
            Debug.LogError("gak nyampe for whatever reason");
        else Debug.Log("aman juga di sini");

        if(bodyManager.currentEquippedBody != null)
            spawnedBody.ApplyPreview(bodyManager.currentEquippedBody.data);

        GameObject slotGO = Instantiate(stickSlotPrefab, bodyGO.transform);
        slotGO.transform.localPosition = Vector3.zero;
        slotGO.transform.localRotation = Quaternion.identity;
        spawnedSlot = slotGO.GetComponent<StickSlot>();

        if (bodyManager != null)
        {
            bodyManager.stickBody = spawnedBody;
            if (bodyManager.currentEquippedBody != null)
            {
                spawnedBody.ApplyPreview(bodyManager.currentEquippedBody.data);
            }
        }
        if (gadgetManager != null)
        {
            gadgetManager.stickBodyTransform = bodyGO.transform;
        }
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
        for(int i = 0; i < gadgetManager.gadgetOwned.Length; i++)
        {
            GadgetInstance currentG = gadgetManager.gadgetOwned[i];
            Button currentButton = buttonsG[i];

            // currentButton.image.sprite = currentG.data.model; // ini bisa dibikin biar dia ambil anak dari button (karena bisa jadi bentuk button beda)
            Image childImage = currentButton.transform.GetChild(0).GetComponent<Image>();
            var tempColor = childImage.color;
            if(i >= gadgetManager.gadgetOwnedNeff)
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
               gadgetManager.StartPreviewGadget(currentG, 0);
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

            BodyInstance currentB = bodyManager.bodyOwned[i];
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
            if(i >= bodyManager.bodyOwnedNeff)
            {
                // Debug.Log("i >= bodyManager.bodyOwnedNeff");
                tempColor.a = 0f;
                currentImage.color = tempColor;
                currentButton.interactable = false;
                continue;
            }
            // Debug.Log("i < bodyManager.bodyOwnedNeff");
            currentButton.interactable = true;
            tempColor.a = 1f;
            currentImage.color = tempColor;

            currentImage.sprite = currentB.data.stickIcon;
            currentButton.onClick.RemoveAllListeners();
            // Debug.Log("Remove all listener");
            currentButton.onClick.AddListener(() =>
            {
                bodyManager.PreviewBody(i);
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
        if (gadgetManager.gadgetOwned[slotIndex].isEquipped)
        {
            gadgetManager.DetachGadgetbyID(gadgetManager.gadgetOwned[slotIndex].id);
        }
        for(int i = gadgetManager.gadgetOwnedNeff - 1; i > slotIndex; i--)
        {
            gadgetManager.gadgetOwned[i-1] = gadgetManager.gadgetOwned[i];
        }
        gadgetManager.gadgetOwned[gadgetManager.gadgetOwnedNeff - 1] = null;
        gadgetManager.gadgetOwnedNeff--;
        EmptyingGadgetButtons();
        SetupRemoveGadgetButtons();
    }

    public void RemoveBodyFromInventory(int slotIndex)
    {
        if(bodyManager.bodyOwned[slotIndex].data == bodyManager.def)
        {
            Debug.LogWarning("Can't remove default body!");
            return;
        }
        if(bodyManager.bodyOwned[slotIndex].isEquipped)
        {
            Debug.LogWarning("Can't remove equipped body!");
            return;
        }

        for(int i = bodyManager.bodyOwnedNeff - 1; i > slotIndex; i--)
            bodyManager.bodyOwned[i-1] = bodyManager.bodyOwned[i];

        bodyManager.bodyOwned[bodyManager.bodyOwnedNeff - 1] = null;
        bodyManager.bodyOwnedNeff--;
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

            BodyInstance currentB = bodyManager.bodyOwned[i];
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
            if(i >= bodyManager.bodyOwnedNeff)
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

            GadgetInstance currentG = gadgetManager.gadgetOwned[i];
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
            if(i >= gadgetManager.gadgetOwnedNeff)
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
        data.stickName = bodyManager.currentEquippedBody.data.stickName;
        data.description = bodyManager.currentEquippedBody.data.description;
        data.stickIcon = bodyManager.currentEquippedBody.data.stickIcon;
        data.stickMesh = bodyManager.currentEquippedBody.data.stickMesh;
        data.stickMaterial = bodyManager.currentEquippedBody.data.stickMaterial;
        data.weight = bodyManager.currentEquippedBody.data.weight;
        data.damage = bodyManager.currentEquippedBody.data.damage;
        // data.HP = bodyManager.currentEquippedBody.data.HP; // hp di mana yah

        /* kirim slot */
        data.frontSlots = spawnedSlot.frontSlots;
        data.backSlots = spawnedSlot.backSlots;
    }

    public void OnSliderValueChanged(float value)
    {
        int curIdx = Mathf.RoundToInt(value);
        gadgetManager.UpdatePreviewPosition(curIdx);
    }

    
}