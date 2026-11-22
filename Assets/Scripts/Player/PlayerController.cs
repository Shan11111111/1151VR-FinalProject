using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ForestAdventure
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public Camera playerCamera;
        public float moveSpeed = 6f;
        public float mouseSensitivity = 0.12f;
        public float gamepadLookSensitivity = 120f;
        public float gravity = -18f;
        private CharacterController controller;
        private float pitch;
        private float verticalVelocity;
        public bool Locked { get; private set; }
        private void Awake() { controller = GetComponent<CharacterController>(); }
        private void Start()
        {
            if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        public void SetLocked(bool value)
        {
            Locked = value;
            Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = value;
        }
        private void Update()
        {
            if (Locked) return;
            Vector2 move = Vector2.zero;
            Vector2 look = Vector2.zero;
            bool usingPad = false;
#if ENABLE_INPUT_SYSTEM
            if (Gamepad.current != null && Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.015f)
            {
                move = Gamepad.current.leftStick.ReadValue();
                usingPad = true;
            }
            else if (Keyboard.current != null)
            {
                move = new Vector2((Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
                                   (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0));
            }
            if (Gamepad.current != null && Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.005f)
            {
                look = Gamepad.current.rightStick.ReadValue();
                usingPad = true;
            }
            else if (Mouse.current != null) look = Mouse.current.delta.ReadValue();
#else
            move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            look = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#endif
            transform.Rotate(Vector3.up, look.x * (usingPad ? gamepadLookSensitivity * Time.deltaTime : mouseSensitivity));
            pitch = Mathf.Clamp(pitch - look.y * (usingPad ? gamepadLookSensitivity * Time.deltaTime : mouseSensitivity), -75f, 75f);
            if (playerCamera != null) playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
            Vector3 direction = transform.right * move.x + transform.forward * move.y;
            direction = Vector3.ClampMagnitude(direction, 1f) * moveSpeed;
            if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            verticalVelocity += gravity * Time.deltaTime;
            controller.Move((direction + Vector3.up * verticalVelocity) * Time.deltaTime);
        }
    }
}
