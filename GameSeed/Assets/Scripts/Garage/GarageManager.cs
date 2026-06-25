using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GarageManager : MonoBehaviour
{
    public GameObject gadgetPanel;
    public GameObject bodyPanel;
    public StickData data;

    [Header("Kebutuhan button")]
    [SerializeField] Transform gadgetPanelTransform;
    [SerializeField] Transform bodyPanelTransform;
    [SerializeField] GadgetManager gadgetManager;

    private Button[] buttonsG;
    private Button[] buttonsB;

    void Start()
    {
        gadgetPanel.SetActive(false);
        bodyPanel.SetActive(true);
        SetupButtons();

        if(data != null && data.stickBody != null)
        {
            GameObject spawnedBody = Instantiate(data.stickBody, transform);

            spawnedBody.transform.localPosition = Vector3.zero;
            spawnedBody.transform.localRotation = Quaternion.identity;

            GetComponent<GadgetManager>().stickBodyTransform = spawnedBody.transform;
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
        SceneManager.LoadScene("Cetta");
    }

    public void SetupButtons()
    {
        buttonsG = gadgetPanelTransform.GetComponentsInChildren<Button>(true);
        for(int i = 0; i < buttonsG.Length; i++)
        {
            if(i >= gadgetManager.gadgetOwned.Length)
            {
                // kasih aset button kosong
                continue;
            }
            GadgetInstance currentG = gadgetManager.gadgetOwned[i];
            Button currentButton = buttonsG[i];
            // kasih aset button

            currentButton.onClick.RemoveAllListeners();
            currentButton.onClick.AddListener(() =>
            {
               gadgetManager.StartPreviewGadget(currentG, 0);
               // masukin teks stat gadget ke text di bawah(?)
               // set active teksnya
            });
        }
    }
}
