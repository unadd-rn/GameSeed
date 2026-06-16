using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

public class ForceButtonHold : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    [Header("Components")]
    public Image button;

    public ForceTest forceTest;

    private bool isPressed;
    // Update is called once per frame

    void Update()
    {
        if (isPressed)
        {
            forceTest.ChangeForce();
            Debug.Log("Button is being held down");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }
}
