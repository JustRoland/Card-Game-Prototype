using System;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Movement
{
    public class PlayerMovement : MonoBehaviour, ICharacterController
    {
        [Header("References")] [SerializeField]
        private Transform cameraTarget;

        [Space] [Header("Other Settings")] 
        [SerializeField] private float gravity = -9.81f;

        [SerializeField] private float standHeight = 2f;
        [SerializeField] private float crouchHeight = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float standCameraTargetHeight = 0.9f;
        [Range(0f, 1f)]
        [SerializeField] private float crouchCameraTargetHeight = 0.7f;
        [SerializeField] private LayerMask slowedLayer;

        private CharacterState _state;
        private Stance _stance;

        private KinematicCharacterMotor _motor;
        private PlayerController _player;
        private Collider[] _overlapColliders;

        private Quaternion _requestedRotation;
        private Vector3 _requestedMovement;
        private bool _requestedSprint;
        private bool _requestedJump;
        private bool _requestedDash;
        private bool _requestedCrouch;

        private Vector3 _startPosition;
        private bool _doubleJumpAvailable;
        private bool _canDoubleJump;
        private bool _dashing;
        private bool _dashAvailable;
        private Vector3 _dashStartPosition;
        private Vector3 _appliedDashVelocity;
        private TimerTime _dashCooldownEndTime;
        private bool _grounded;


        public void Initialize()
        {
            _motor = GetComponent<KinematicCharacterMotor>();
            _motor.CharacterController = this;
            _player = GetComponent<PlayerController>();
            _stance = Stance.Stand;
            _overlapColliders = new Collider[8];

            //without this, the cooldown end times are always under the threshold 
            _dashCooldownEndTime = new TimerTime(100, 0, 0);

            _startPosition = transform.position;
        }

        public void ResetPlayerCharacter()
        {
            _dashCooldownEndTime = new TimerTime(100, 0, 0);

            _requestedMovement = Vector3.zero;
            _requestedRotation = Quaternion.identity;
            _requestedSprint = false;
            _requestedJump = false;
            _requestedDash = false;
            _requestedCrouch = false;

            _motor.SetPosition(_startPosition);
        }


        public void UpdateInput(CharacterInput input)
        {
            _requestedRotation = input.Rotation;
            _requestedMovement = new Vector3(input.Move.x, 0, input.Move.y).normalized;
            _requestedMovement = input.Rotation * _requestedMovement;
            _requestedSprint = input.Sprint;
            _requestedJump = _requestedJump || input.Jump;
            _requestedDash = _requestedDash || input.Dash;
            _requestedCrouch = input.Crouch switch
                {
                    CrouchInput.None => _requestedCrouch,
                    CrouchInput.Toggle => !_requestedCrouch,
                    CrouchInput.Hold => true,
                    CrouchInput.Uncrouch => false,
                    _ => _requestedCrouch
                };
            _canDoubleJump = input.DoubleJump;
        }

        public void UpdateBody()
        {
            var currentHeight = _motor.Capsule.height;
            var cameraTargetHeight = currentHeight * (_stance is Stance.Stand ? standCameraTargetHeight : crouchCameraTargetHeight);
            cameraTarget.localPosition = new Vector3(0f, cameraTargetHeight, 0f);
        }
        
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            var projectedRotation = Vector3.ProjectOnPlane(_requestedRotation * Vector3.forward, _motor.CharacterUp);

            //if statement can be removed with restricted vertical look angle.
            if (projectedRotation != Vector3.zero)
                currentRotation = Quaternion.LookRotation(projectedRotation, _motor.CharacterUp);
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            //if grounded ...
            if (_motor.GroundingStatus.IsStableOnGround)
            {
                _doubleJumpAvailable = true;
                _dashAvailable = true;

                var projectedMovement =
                    _motor.GetDirectionTangentToSurface(_requestedMovement, _motor.GroundingStatus.GroundNormal) *
                    _requestedMovement.magnitude;


                _state = slowedLayer == (slowedLayer | 1 << _motor.GroundingStatus.GroundCollider.gameObject.layer) ? CharacterState.Slowed : CharacterState.Normal;


                // lazy override
                 var speed = _requestedSprint ? _player.Stats.SprintSpeed : _player.Stats.WalkSpeed;
                 speed = _requestedCrouch ? _player.Stats.CrouchSpeed : speed;
                 speed = _state is CharacterState.Slowed ? _player.Stats.SlowedSpeed : speed;

                var targetVelocity = projectedMovement * speed;
                currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity,
                    1f - Mathf.Exp(-_player.Stats.Acceleration / speed * deltaTime));
            }
            //or in the air...
            else
            {
                //in air movement
                var horizontalInAirVelocity = Vector3.ProjectOnPlane(currentVelocity, Vector3.up);

                var projectedMovement =
                    _motor.GetDirectionTangentToSurface(_requestedMovement, _motor.GroundingStatus.GroundNormal) *
                    _requestedMovement.magnitude;

                var horizontalSpeed = horizontalInAirVelocity.magnitude;
                var speed = Mathf.Max(horizontalSpeed, _player.Stats.WalkSpeed);
                var targetHorizontalVelocity = projectedMovement * speed;

                var targetVelocity = targetHorizontalVelocity + new Vector3(0, currentVelocity.y, 0);

                currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity,
                    1f - Mathf.Exp(-_player.Stats.AirAcceleration * deltaTime));


                //gravity
                currentVelocity += Vector3.up * (gravity * deltaTime);
            }

            if (_requestedJump)
            {
                _requestedJump = false;
                
                //TODO: Consider caching the button press to execute when available.
                //dont allow jump while dashing
                if (_dashing) return;
                
                if (_motor.GroundingStatus.IsStableOnGround)
                {
                    _motor.ForceUnground(0f);

                    currentVelocity.y = Mathf.Max(currentVelocity.y, _player.Stats.JumpForce);
                }
                else if (_doubleJumpAvailable && _canDoubleJump)
                {
                    _doubleJumpAvailable = false;
                    currentVelocity.y = Mathf.Max(currentVelocity.y, _player.Stats.JumpForce);
                }
            }

            if (_requestedDash)
            {
                _requestedDash = false;

                if (_dashing) return;
                if (!_dashAvailable) return;
                if (GameManager.Instance.CurrentTime >= _dashCooldownEndTime) return;

                _dashing = true;
                _dashAvailable = false;

                _dashCooldownEndTime = GameManager.Instance.CurrentTime + new TimerTime(0, 0, seconds: _player.Stats.DashCooldown);

                var projectedMovement =
                    _motor.GetDirectionTangentToSurface(_requestedMovement, _motor.GroundingStatus.GroundNormal) *
                    _requestedMovement.magnitude;

                _dashStartPosition = _motor.TransientPosition;

                _appliedDashVelocity = projectedMovement.magnitude != 0
                    ? projectedMovement * _player.Stats.DashForce
                    : _motor.CharacterForward * _player.Stats.DashForce;

                currentVelocity += _appliedDashVelocity;
            }

            //this is to limit the dash in the air
            if (_dashing)
            {
                var distance = Vector3.Distance(_dashStartPosition, _motor.TransientPosition);

                //if on the ground
                if (_motor.GroundingStatus.IsStableOnGround)
                {
                    //wait until distance is reached and ...
                    if (distance < _player.Stats.DashAirDistance / 1.5f) return;
                }
                //else in the air
                else
                {
                    //wait until distance is reached, then subtract initial dash velocity and...
                    if (distance < _player.Stats.DashAirDistance) return;
                    currentVelocity -= _appliedDashVelocity * .9f;
                }

                //...disable dashing.
                _dashing = false;
            }
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
            _grounded = _motor.GroundingStatus.IsStableOnGround;

            if (_requestedCrouch && _stance is Stance.Stand)
            {
                print("Crouch");
                _stance = Stance.Crouch;
                _motor.SetCapsuleDimensions(radius: _motor.Capsule.radius, height: crouchHeight, yOffset: crouchHeight * 0.5f);
            }
        }

        public void PostGroundingUpdate(float deltaTime)
        {
            if (_grounded) return;

            if (_motor.GroundingStatus.IsStableOnGround)
            {
                //Play landing sound...
            }
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
            if (!_requestedCrouch && _stance is not Stance.Stand)
            {
                _motor.SetCapsuleDimensions(radius: _motor.Capsule.radius, height: standHeight,
                    yOffset: standHeight * 0.5f);
            }
            else return;

            if (_motor.CharacterOverlap(position: _motor.TransientPosition,
                    rotation: _motor.TransientRotation,
                    overlappedColliders: _overlapColliders,
                    layers: _motor.CollidableLayers,
                    triggerInteraction: QueryTriggerInteraction.Ignore) > 0)
            {
                print("Could not stand up");
                _requestedCrouch = true;
                _motor.SetCapsuleDimensions(radius: _motor.Capsule.radius, height: crouchHeight, yOffset: crouchHeight * 0.5f);
            }
            else
            {
                print("Stand up");
                _stance = Stance.Stand;
            }
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            Vector3 atCharacterPosition,
            Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }

        public Transform GetCameraTarget() => cameraTarget;
        public void ChangePlayerState(CharacterState newState) => _state = newState;

        public void RefreshAbility(Abilities abilities, int count)
        {
            switch (abilities)
            {
                case Abilities.DoubleJump:
                    _doubleJumpAvailable = true;
                    break;
                case Abilities.Dash:
                    _dashAvailable = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(abilities), abilities, null);
            }
        }
    }
}