using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeTest : MonoBehaviour
{
    private PlayerControls _controls;

    [SerializeField] private float minimumSwipeMagnitude = 10f;
    private Vector2 _swipeDirection;
    // Start is called before the first frame update
    void Start()
    {
        _controls = new PlayerControls();

        _controls.Player.Enable();

        _controls.Player.Touch.canceled += ProcessTouchComplete;
        _controls.Player.Swipe.performed += ProcessSwipeDelta;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ProcessSwipeDelta(InputAction.CallbackContext context)
    {
        _swipeDirection = context.ReadValue<Vector2>();
    }

    private void ProcessTouchComplete(InputAction.CallbackContext context)
    {
        //Check if the magnitude is high enough
        Debug.Log("Touch complete");
        if (Mathf.Abs(_swipeDirection.magnitude) < minimumSwipeMagnitude) return;
        Debug.Log("Swipe detected");

        var position = Vector3.zero;

        //check horizontal direction
        if (_swipeDirection.x > 0)
        {
            Debug.Log("Swiping Right");
            position.x = 1;
        }
        else if (_swipeDirection.x < 0)
        {
            Debug.Log("Swiping Left");
            position.x = -1;
        }

        //check vertical direction
        if (_swipeDirection.y > 0)
        {
            Debug.Log("Swiping Up");
            position.y = 1;
        }
        else if (_swipeDirection.y < 0)
        {
            Debug.Log("Swiping Down");
            position.y = -1;
        }

        //apply these changes to the current object
        transform.position = position;
    }
}
