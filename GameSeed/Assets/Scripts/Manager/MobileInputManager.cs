// using UnityEngine;
// using UnityEngine.InputSystem;

// public class MobileInputManager : MonoBehaviour
// {
//     [SerializeField] private StickThrowTest stickThrowTest;
//     [SerializeField] private float dragSensitivity = 0.002f; // Adjust to fit your screen resolution scaling

//     private PlayerControls controls;
//     private Vector2 touchStartPos;
//     private bool isAiming = false;

//     void Awake()
//     {
//         controls = new PlayerControls();
//     }

//     void OnEnable()
//     {
//         controls.Player.Enable();
        
//         controls.Player.TouchPress.started += OnTouchStarted;
//         controls.Player.TouchPress.canceled += OnTouchReleased;
//     }

//     void OnDisable()
//     {
//         controls.Player.TouchPress.started -= OnTouchStarted;
//         controls.Player.TouchPress.canceled -= OnTouchReleased;
        
//         controls.Player.Disable();
//     }

//     private void OnTouchStarted(InputAction.CallbackContext context)
//     {
//         touchStartPos = controls.Player.Aim.ReadValue<Vector2>();
//         isAiming = true;
//     }

//     void Update()
//     {
//         if (!isAiming || stickThrowTest == null) return;
//         Vector2 currentTouchPos = controls.Player.Aim.ReadValue<Vector2>();
//         float deltaY = currentTouchPos.y - touchStartPos.y;
//         float currentPower = Mathf.Clamp(deltaY * dragSensitivity, -1f, 1f);
//     }

//     private void OnTouchReleased(InputAction.CallbackContext context)
//     {
//         if (!isAiming) return;
        
//         isAiming = false;

//         if (stickThrowTest != null)
//         {
//             stickThrowTest.Throw();
//         }
//     }
// }