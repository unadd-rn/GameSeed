using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions.Must;

public class GarageManager : MonoBehaviour
{
    public static GarageManager Instance;
    public GameObject gadgetPanel;
    public GameObject bodyPanel;
    public StickSlot data;

    [Header("Kebutuhan button")]
    [SerializeField] Transform gadgetPanelTransform;
    [SerializeField] Transform bodyPanelTransform;

    [Header("Manager lain")]
    [SerializeField] GadgetManager gadgetManager;
    [SerializeField] BodyManager bodyManager;

    [Header("Confirm Button")]
    [SerializeField] GameObject confirmButtonGadget;
    [SerializeField] GameObject confirmButtonBody;

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
    private StickSlot spawnedSlot;

    void Start()
    {
        textField.SetActive(false);
        gadgetPanel.SetActive(false);
        bodyPanel.SetActive(true);
        confirmButtonGadget.SetActive(false);
        confirmButtonBody.SetActive(false);
        
        SetupGadgetButtons();
        SetupBodyButtons();

        SpawnStickSystem();
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
        SceneManager.LoadScene("Cet - lobby1");
    }

    public void EmptyingGadgetButtons()
    {
        buttonsG = gadgetPanelTransform.GetComponentsInChildren<Button>(true);
        for(int i = 0; i < buttonsG.Length; i++)
        {
            Button currentButton = buttonsG[i];
            currentButton.image.sprite = null;
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
                tempColor.a = 0f;
                childImage.color = tempColor;
                continue;
            }
            tempColor.a = 1f;
            childImage.color = tempColor;

            childImage.sprite = currentG.data.model;
            currentButton.onClick.RemoveAllListeners();
            currentButton.onClick.AddListener(() =>
            {
               gadgetManager.StartPreviewGadget(currentG, 0);
               confirmButtonGadget.SetActive(true);
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
                bodyOrGadgetName.text = currentB.data.name.ToString();
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
        for(int i = gadgetManager.gadgetOwned.Length - 1; i > slotIndex; i--)
        {
            gadgetManager.gadgetOwned[i-1] = gadgetManager.gadgetOwned[i];
        }
        gadgetManager.gadgetOwned[gadgetManager.gadgetOwned.Length - 1] = null;
        gadgetManager.gadgetOwnedNeff--;
        EmptyingGadgetButtons();
        SetupGadgetButtons();
    }

    public void InactivateConfirm()
    {
        confirmButtonBody.SetActive(false);
        confirmButtonGadget.SetActive(false);
    }
}
