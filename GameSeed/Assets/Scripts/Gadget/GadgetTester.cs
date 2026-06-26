using UnityEngine;

public class GadgetTester : MonoBehaviour
{
    public GadgetManager gadgetManager;
    public RadialGadgetController radialController;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (gadgetManager.gadgetOwned[0] != null)
            {
                gadgetManager.StartPreviewGadget(gadgetManager.gadgetOwned[0], 0);
                Debug.Log("Previewing Gadget 0 in Slot 0");
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            gadgetManager.ConfirmPlacement();
            radialController.PopulateRadialMenu(); // Crucial: Refresh the UI!
            Debug.Log("Gadget Equipped and Menu Updated!");
        }
    }
}