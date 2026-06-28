using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GarageManager : MonoBehaviour
{
    public GameObject gadgetPanel;
    public GameObject bodyPanel;
    public StickSlot data;

    [Header("Kebutuhan button")]
    [SerializeField] Transform gadgetPanelTransform;
    [SerializeField] Transform bodyPanelTransform;
    [SerializeField] GadgetManager gadgetManager;
    [SerializeField] BodyManager bodyManager;

    [Header("Confirm Button")]
    [SerializeField] GameObject confirmButtonGadget;
    [SerializeField] GameObject confirmButtonBody;

    [Header("Text")]
    [SerializeField] private GameObject textField;
    [SerializeField] private TextMeshProUGUI bodyOrGadgetName;
    [SerializeField] private TextMeshProUGUI bodyOrGadgetDesc;

    private Button[] buttonsG = new Button[10];
    private Button[] buttonsB = new Button[10];

    void Start()
    {
        textField.SetActive(false);
        gadgetPanel.SetActive(false);
        bodyPanel.SetActive(true);
        confirmButtonGadget.SetActive(false);
        confirmButtonBody.SetActive(false);
        
        SetupGadgetButtons();

        // if(data != null && data.stickBody != null)
        // {
        //     GameObject spawnedBody = Instantiate(data.stickBody, transform);

        //     spawnedBody.transform.localPosition = Vector3.zero;
        //     spawnedBody.transform.localRotation = Quaternion.identity;

        //     GetComponent<GadgetManager>().stickBodyTransform = spawnedBody.transform;
        // }
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

    public void EmptyingBodyButton()
    {
        buttonsB = bodyPanelTransform.GetComponentsInChildren<Button>(true);
        for(int i = 0; i < buttonsB.Length; i++)
        {
            Button currentButton = buttonsB[i];
            currentButton.image.sprite = null;
        }
    }

    public void SetupGadgetButtons()
    {
        buttonsG = gadgetPanelTransform.GetComponentsInChildren<Button>(true);
        Debug.Log($"Button in Scene Length: {buttonsG.Length}");
        Debug.Log($"gadgetOwnedLength: {gadgetManager.gadgetOwned.Length} ");
        for(int i = 0; i < buttonsG.Length; i++)
        {
            if(i >= gadgetManager.gadgetOwned.Length)
            {
                // kasih aset button kosong
                continue;
            }
            GadgetInstance currentG = gadgetManager.gadgetOwned[i];
            Button currentButton = buttonsG[i];

            currentButton.image.sprite = currentG.data.model; // ini bisa dibikin biar dia ambil anak dari button (karena bisa jadi bentuk button beda)
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
        buttonsB = bodyPanelTransform.GetComponentsInChildren<Button>(true);
        for(int i = 0; i < buttonsB.Length; i++)
        {
            if(i >= bodyManager.bodyOwned.Length)
            {
                // apala
                continue;
            }

            BodyInstance currentB = bodyManager.bodyOwned[i];
            Button currentButton = buttonsB[i];

            currentButton.image.sprite = currentB.data.stickIcon;

            currentButton.onClick.RemoveAllListeners();
            currentButton.onClick.AddListener(() =>
            {
                bodyManager.PreviewBody(i);
                confirmButtonBody.SetActive(true);
                bodyOrGadgetName.text = currentB.data.name.ToString();
                bodyOrGadgetDesc.text = currentB.data.description.ToString();
                textField.SetActive(true);
            });
        }
    }

    public void RemoveGadgetFromInventory(int slotIndex)
    {
        if(slotIndex >= gadgetManager.gadgetOwned.Length) return;
        if (gadgetManager.gadgetOwned[slotIndex].isEquipped)
        {
            gadgetManager.DetachGadgetbyID(gadgetManager.gadgetOwned[slotIndex].id);
        }
        for(int i = gadgetManager.gadgetOwned.Length - 1; i > slotIndex; i--)
        {
            gadgetManager.gadgetOwned[i-1] = gadgetManager.gadgetOwned[i];
        }
        gadgetManager.gadgetOwned[gadgetManager.gadgetOwned.Length - 1] = null;
        EmptyingGadgetButtons();
        SetupGadgetButtons();
    }

    public void RemoveBodyFromInventory(int bodyIdx)
    {
        if(bodyIdx >= bodyManager.bodyOwned.Length) return;
        if(bodyManager.currentEquippedBody.id == bodyManager.bodyOwned[bodyIdx].id)
        {
            // error message cannot remove because it is used
            Debug.LogWarning("Can't remove because body is equipped!");
            return;
        }
        for(int i = bodyManager.bodyOwned.Length - 1; i > bodyIdx; i--)
            bodyManager.bodyOwned[i-1] = bodyManager.bodyOwned[i];
        bodyManager.bodyOwned[bodyManager.bodyOwned.Length - 1] = null;
        EmptyingBodyButton();
        SetupBodyButtons();
    }

    public void InactivateConfirmBody()
    {
        confirmButtonBody.SetActive(false);
    }

    public void InactivateConfirmGadget()
    {
        confirmButtonGadget.SetActive(true);
    }
}
