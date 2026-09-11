using UnityEngine;
using UnityEngine.InputSystem;

namespace Poolcore
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        public MovementSettings settings;
        public Camera view;
        private CharacterController body;
        private InputAction move, look, fast, capture, release;
        private Vector3 spawn;
        private Quaternion spawnRotation;
        private float verticalSpeed, pitch;
        private float wadingMultiplier=1;
        public bool menuControlsCapture;
        public bool IsCaptured => Cursor.lockState == CursorLockMode.Locked;
        public float MouseSensitivity { get; set; }
        public float FieldOfView { get => view.fieldOfView; set => view.fieldOfView=value; }
        public void Capture() { Cursor.lockState=CursorLockMode.Locked; Cursor.visible=false; }
        public void Release() { Unlock(); }

        private void Awake()
        {
            body = GetComponent<CharacterController>();
            spawn = transform.position;
            spawnRotation = transform.rotation;
            view.fieldOfView = settings.fieldOfView;
            MouseSensitivity=settings.sensitivity;
            move = new InputAction("Move", InputActionType.Value);
            move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            look = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");
            fast = new InputAction("Fast", InputActionType.Button, "<Keyboard>/leftShift");
            capture = new InputAction("Capture", InputActionType.Button, "<Mouse>/leftButton");
            release = new InputAction("Release", InputActionType.Button, "<Keyboard>/escape");
        }

        private void OnEnable() { move.Enable(); look.Enable(); fast.Enable(); capture.Enable(); release.Enable(); }
        private void OnDisable()
        {
            move.Disable(); look.Disable(); fast.Disable(); capture.Disable(); release.Disable();
            Unlock();
        }
        private void OnDestroy() { move.Dispose(); look.Dispose(); fast.Dispose(); capture.Dispose(); release.Dispose(); }
        private void OnApplicationFocus(bool focused) { if (!focused) Unlock(); }
        private void OnApplicationPause(bool paused) { if (paused) Unlock(); }
        private static void Unlock() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }

        private void Update()
        {
            if (release.WasPressedThisFrame()) { Unlock(); return; }
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                if (!menuControlsCapture && Application.isFocused && capture.WasPressedThisFrame())
                { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
                return;
            }
            var delta = look.ReadValue<Vector2>() * MouseSensitivity;
            transform.Rotate(0, delta.x, 0);
            pitch = Mathf.Clamp(pitch - delta.y, -85, 85);
            view.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
            SimulateMovement(move.ReadValue<Vector2>(), fast.IsPressed(), Mathf.Min(Time.deltaTime, 0.05f));
        }

        // Shared by input and integration validation: drives the real CharacterController.
        public void SimulateMovement(Vector2 input, bool faster, float dt)
        {
            if (transform.position.y < settings.respawnBelow) { ResetToSpawn(); return; }
            input = Vector2.ClampMagnitude(input, 1);
            float targetWading=WaterZone.DepthAt(transform.position)>0.08f ? 0.65f : 1;
            wadingMultiplier=Mathf.MoveTowards(wadingMultiplier,targetWading,dt*2);
            var velocity = (transform.right * input.x + transform.forward * input.y) * (faster ? settings.fastSpeed : settings.walkSpeed) * wadingMultiplier;
            if (body.isGrounded && verticalSpeed < 0) verticalSpeed = -2;
            verticalSpeed += settings.gravity * dt;
            velocity.y = verticalSpeed;
            var flags = body.Move(velocity * dt);
            if ((flags & CollisionFlags.Above) != 0 && verticalSpeed > 0) verticalSpeed = 0;
        }

        public void Teleport(Vector3 position)
        {
            body.enabled = false;
            transform.position = position;
            verticalSpeed = 0;
            wadingMultiplier=1;
            body.enabled = true;
            Physics.SyncTransforms();
        }
        public void ResetToSpawn()
        {
            Teleport(spawn);
            transform.rotation = spawnRotation;
            pitch = 0;
            view.transform.localRotation = Quaternion.identity;
        }
    }
}
