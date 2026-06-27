using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GadgetButtonHandler : MonoBehaviour
{
    [Header("References")]
    public RadialGadgetController radialController; 
    public GameObject playerTarget; 

    public void OnUseGadgetClicked()
    {
        if (radialController == null) return;
        GadgetInstance selectedGadget = radialController.GetSelectedGadget();
        if (selectedGadget != null && selectedGadget.data != null)
        {
            selectedGadget.data.Apply(playerTarget != null ? playerTarget : gameObject);
            Debug.Log(selectedGadget.data.gadgetName);
        }
    }
}
