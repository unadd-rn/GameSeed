using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GarageManager : MonoBehaviour
{
    public GameObject gadgetPanel;
    public GameObject bodyPanel;
    // Start is called before the first frame update
    void Start()
    {
        gadgetPanel.SetActive(false);
        bodyPanel.SetActive(true);
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
}
