using System;
using UnityEngine;


namespace Movement
{
    public struct CharacterInput
    {
        public Quaternion Rotation;
        public Vector2 Move;
        public bool Sprint;
        public bool Jump;
        public bool DoubleJump;
        public bool Dash;
        public CrouchInput Crouch;

        public bool Attack;
        public bool AttackButtonDown;
        public bool Reload;
    }

    public enum CrouchInput
    {
        None, 
        Toggle,
        Hold,
        Uncrouch,
    }

    public enum Stance
    {
        Stand,
        Crouch,
    }
    public enum CharacterState //Potentially redundant.
    {
        Normal,
        Slowed,
    }
    public enum Abilities
    {
        DoubleJump,
        Dash,
    }


    public class PlayerController : MonoBehaviour
    {
        private InputSystem_Actions _inputAction;
        
        [SerializeField] private BaseStats baseStats;
        public CharacterStats Stats { get; private set; }

        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerCombat playerCombat;
        [SerializeField] private PlayerCamera playerCamera;
        [SerializeField] private CameraSpring cameraSpring;

        [Space] [SerializeField] private float raycastMaxDistance;
        [SerializeField] private GameObject interactVisual;
        [SerializeField] private bool canDash;
        [SerializeField] private bool canDoubleJump;


        private Camera _camera;

        private void Awake()
        {
            Stats = new CharacterStats(new StatsMediator(), baseStats);
        }


        private void Start()
        {
            _camera = GetComponentInChildren<Camera>();

            _inputAction = new InputSystem_Actions();
            _inputAction.Enable();

            playerMovement.Initialize();
            playerCombat.Initialize(this);
            playerCamera.Initialize(playerMovement.GetCameraTarget());
            cameraSpring.Initialize();
        }

        private void OnDestroy()
        {
            _inputAction.Disable();
        }


        private void ResetPlayer()
        {
            playerMovement.ResetPlayerCharacter();
            playerCamera.Initialize(playerMovement.GetCameraTarget());
            cameraSpring.Initialize();
        }

        private void Update()
        {
            if (_inputAction.Player.Reset.WasPressedThisFrame())
            {
                ResetPlayer();
            }


            else
            {
                //camera rotation
                playerCamera.UpdateRotation(_inputAction.Player.Look.ReadValue<Vector2>());

                InterpretRaycast(CenterScreenRaycast(raycastMaxDistance));

                //character input
                var input = new CharacterInput
                {
                    Rotation = playerCamera.transform.rotation,
                    Move = _inputAction.Player.Move.ReadValue<Vector2>(),
                    Sprint = _inputAction.Player.Sprint.IsPressed(),
                    Jump = _inputAction.Player.Jump.WasPressedThisFrame(),
                    DoubleJump = canDoubleJump,
                    Dash = _inputAction.Player.Dash.WasPressedThisFrame() && canDash,
                    Crouch = _inputAction.Player.Crouch.WasPressedThisFrame() ? CrouchInput.Toggle : CrouchInput.None,
                    Attack = _inputAction.Player.Attack.IsPressed(),
                    AttackButtonDown = _inputAction.Player.Attack.WasPressedThisFrame(),
                    Reload = _inputAction.Player.Reload.WasPressedThisFrame()
                };
                playerMovement.UpdateInput(input);
                playerMovement.UpdateBody();
                playerCombat.UpdateInput(input);
            }
            
            Stats.Mediator.Update(Time.deltaTime);
            // print($"Attack: {Stats.Damage}, Defense: {Stats.Defense}");
        }

        private void LateUpdate()
        {
            var cameraTarget = playerMovement.GetCameraTarget();

            playerCamera.UpdatePosition(cameraTarget);
        }

        public (RaycastHit?, Ray) CenterScreenRaycast(float distance)
        {
            var ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            return Physics.Raycast(ray, out RaycastHit hit, distance) ? (hit, ray) : (null, ray);
        }


        private void InterpretRaycast((RaycastHit? hit, Ray ray) raycast)
        {
            if (raycast.hit.HasValue && raycast.hit.Value.transform.TryGetComponent<IInteractable>(out var interactable))
            {
                if (interactVisual) interactVisual.SetActive(true);
                if (!_inputAction.Player.Interact.WasPressedThisFrame()) return;
                interactable.Interact();
            }
            else
            {
                if (interactVisual) interactVisual.SetActive(false);
            }


            // For interacting with objects in the world.
        }

        public void EnableDash(bool value) => canDash = value;
        public void EnableDoubleJump(bool value) => canDoubleJump = value;
    }
}