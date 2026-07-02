using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

public class ForceButtonHold : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    [Header("Components")]
    // public Image button;

    public ForceTest forceTest;
    [SerializeField] private StickThrowTest stickThrow;

    private bool isPressed;
    public event System.Action OnForcePressed;
    public event System.Action OnForceReleased;
    // Update is called once per frame

    void Start()
    {
        isPressed = false;
    }

    void Update()
    {
        if (isPressed)
        {
            forceTest.ChangeForce();
            Debug.Log("Button is being held down");
        }
    }

    public void ButtonForce()
    {
        if(isPressed) isPressed = false;
        else isPressed = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        forceTest.ChangeForce();
        OnForcePressed?.Invoke();

        if (AudioManager.Instance != null)
        {
        AudioManager.Instance.PlayLoopingSFX("PowerNaikTurun");
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        stickThrow.Throw();

        AudioManager.Instance.StopLoopingSFX();

        AudioManager.Instance.PlayThrowSFXSequence();
    }
}
