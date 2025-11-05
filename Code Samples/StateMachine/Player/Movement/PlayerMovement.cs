using Input;
using UnityEngine;

namespace StateMachine.Player.Movement
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : StateMachine<PlayerMovement>
    {
        #region Member Fields

        [Header("References")]
        private UserInput _input;
        private CharacterController _controller;
        private Animator _animator;

        [Header("Movement Settings")]
        [SerializeField] private float _walkSpeed = 1.5f;
        [SerializeField] private float _runSpeed = 3f;
        [SerializeField] private float _crouchSpeed = 1.0f;
        [SerializeField] private float _accelerationRate = 10f;
        [SerializeField] private float _decelerationRate = 5f;
        [SerializeField] private float _turnTiltAngle = 5f;
        [SerializeField] private float _dodgeTime = 0.35f;

        [Header("Interpolation Settings")]
        [SerializeField] private AnimationCurve _accelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float _directionChangeThreshold = 90f;
        [SerializeField] private float _directionChangePenalty = 0.5f;

        [Header("Jump & Gravity Settings")]
        [SerializeField] private float _jumpHeight = 0.9f;
        [SerializeField] private float _groundedGravity = -2f;
        [SerializeField] private float _fallMultiplier = 2f;
        [SerializeField] private float _coyoteTime = 0.05f;
        [SerializeField] private float _landTime = 0.35f;

        [Header("Collision & Edge Detection")]
        [SerializeField] private float _maxEdgeStickAngle = 45f;
        [SerializeField] private float _edgePushForce = 2f;

        private readonly float _gravity = -9.81f;
        private Vector3 _currentVelocity = Vector3.zero;
        private Vector3 _targetVelocity = Vector3.zero;
        private Vector3 _lastInputDirection = Vector3.zero;
        private bool _isGrounded;
        private readonly bool _isFalling;
        private bool _isSliding;

        private Vector3 _currentMovement;
        private Vector3 _appliedMovement;

        private ControllerHit _lastGroundHit;
        private ControllerHit _currentGroundHit;
        private CollisionFlags _collisionFlags;

        #endregion

        #region Properties

        // References
        public UserInput Input => _input;
        public CharacterController Controller => _controller;
        public Animator Animator => _animator;

        // Speed control
        public float WalkSpeed => _walkSpeed;
        public float RunSpeed => _runSpeed;
        public float CrouchSpeed => _crouchSpeed;
        public float AccelerationRate => _accelerationRate;
        public float DecelerationRate => _decelerationRate;
        public float TargetSpeed { get; set; }
        public float CurrentSpeed => _currentVelocity.magnitude;
        public float DodgeTime => _dodgeTime;

        // Velocity
        public float AppliedVerticalVelocity
        {
            get => _appliedMovement.y;
            set => _appliedMovement.y = value;
        }
        public float CurrentVerticalVelocity
        {
            get => _currentMovement.y;
            set => _currentMovement.y = value;
        }

        // Jump & Gravity control
        public float JumpHeight => _jumpHeight;
        public float Gravity => _gravity;
        public float GroundedGravity => _groundedGravity;
        public float FallMultiplier => _fallMultiplier;
        public float LandTime => _landTime;
        public float TimeSinceLastGrounded { get; set; }

        // Input control
        public Vector2 MoveInput => _input.Move;
        public bool JumpPressed => _input.JumpPressed;
        public bool RunPressed => _input.RunPressed;

        // States control
        public bool HasControl { get; set; } = true;

        // Standing state control
        public bool IsMoving => _input.Move.magnitude > 0.1f;
        public bool IsRunning => _input.RunPressed && _input.Move.y > 0;
        public bool IsSliding => IsRunning && IsCrouching || _isSliding;
        public bool IsCrouching => _input.Crouch;
        public bool IsDodging => _input.Move.y <= 0f && _input.JumpPressed;

        // Root state control
        public bool IsGrounded => _isGrounded && !IsJumping;
        public bool IsFalling => !_isGrounded && _appliedMovement.y <= 1f;
        public bool IsJumping => _isGrounded && _input.JumpPressed && _input.Move.y > 0;

        // Collider control
        public float ControllerHeight { get; set; }
        public Vector3 ControllerCenter { get; set; }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            _input = UserInput.Instance;
            InitializeStateMachine(new GroundedState(this));
        }

        private void Update()
        {
            UpdateStateMachine();
            CalculateVelocity();
            ApplyMovement();
            ApplyGravity();
            UpdateGroundedStatus();
        }

        private void LateUpdate()
        {
            ApplyAnimations();
            UpdateCapsuleCollider();
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if ((_collisionFlags & CollisionFlags.Below) != 0)
            {
                _currentGroundHit = new(hit);
            }
        }

        #endregion

        #region Velocity Control

        /// <summary>
        /// Calculates and updates the character's current movement velocity based on input, acceleration,
        /// deceleration, and direction changes.
        /// </summary>
        /// <remarks>
        /// This method blends the current and target velocity smoothly using dynamic acceleration/deceleration rates.
        /// It ensures responsive movement while maintaining natural transitions between motion states.
        /// </remarks>
        private void CalculateVelocity()
        {
            // Compute normalized input direction
            Vector3 inputDirection = Vector3.zero;
            if (MoveInput.magnitude > 0.01f)
            {
                inputDirection = new Vector3(MoveInput.x, 0f, MoveInput.y).normalized;
            }

            // Determine target velocity based on control state
            if (HasControl)
                _targetVelocity = inputDirection * TargetSpeed;
            else
                _targetVelocity = Vector3.zero;

            float rate = CalculateDynamicRate(inputDirection);

            _currentVelocity = InterpolateVelocity(_currentVelocity, _targetVelocity, rate);

            // Store last non-zero input direction for use in directional change detection
            if (inputDirection != Vector3.zero)
            {
                _lastInputDirection = inputDirection;
            }
        }

        /// <summary>
        /// Calculates a dynamic interpolation rate for velocity blending based on current input and movement direction.
        /// </summary>
        /// <param name="inputDirection">The current normalized input direction.</param>
        /// <returns>
        /// A rate value that determines how quickly velocity should interpolate toward the target.
        /// </returns>
        /// <remarks>
        /// The returned rate adjusts automatically depending on whether the player is accelerating,
        /// decelerating, or changing direction.
        /// </remarks>
        private float CalculateDynamicRate(Vector3 inputDirection)
        {
            float currentSpeed = _currentVelocity.magnitude;
            float targetSpeed = _targetVelocity.magnitude;

            // Case 1: No input — apply deceleration
            if (inputDirection == Vector3.zero)
            {
                return _decelerationRate;
            }

            // Case 2: Direction change detected
            if (_lastInputDirection != Vector3.zero && inputDirection != Vector3.zero)
            {
                float angle = Vector3.Angle(_lastInputDirection, inputDirection);

                // If turning beyond a threshold (e.g. >90°), apply a temporary slowdown
                if (angle > _directionChangeThreshold)
                {
                    float directionChangeFactor = Mathf.InverseLerp(_directionChangeThreshold, 180f, angle);
                    // Higher angle -> stronger deceleration penalty
                    return Mathf.Lerp(_accelerationRate, _decelerationRate, directionChangeFactor * _directionChangePenalty);
                }
            }

            // Case 3: Classic acceleration/deceleration
            if (currentSpeed < targetSpeed)
            {
                return _accelerationRate;
            }
            else
            {
                return _decelerationRate;
            }
        }

        /// <summary>
        /// Smoothly interpolates velocity between current and target values using a dynamic rate and acceleration curve.
        /// </summary>
        /// <param name="current">Current velocity vector.</param>
        /// <param name="target">Target velocity vector.</param>
        /// <param name="rate">Interpolation rate, typically from <see cref="CalculateDynamicRate"/>.</param>
        /// <returns>New interpolated velocity vector.</returns>
        /// <remarks>
        /// Applies a custom acceleration curve during speed-up phases for smoother motion response.
        /// </remarks>
        private Vector3 InterpolateVelocity(Vector3 current, Vector3 target, float rate)
        {
            // Base linear interpolation toward the target velocity
            Vector3 newVelocity = Vector3.MoveTowards(current, target, rate * Time.deltaTime);

            // Apply acceleration curve only when speeding up
            float currentMagnitude = current.magnitude;
            float targetMagnitude = target.magnitude;

            if (targetMagnitude > 0.01f && currentMagnitude < targetMagnitude)
            {
                float progress = Mathf.Clamp01(currentMagnitude / Mathf.Max(TargetSpeed, 0.01f));
                float curveValue = _accelerationCurve.Evaluate(progress);
                float adjustedRate = rate * (0.5f + curveValue * 1.5f);

                // Apply the adjusted rate for smoother acceleration
                newVelocity = Vector3.MoveTowards(current, target, adjustedRate * Time.deltaTime);
            }

            return newVelocity;
        }

        #endregion

        #region Movement

        /// <summary>
        /// Applies the current movement vector to the character controller, moving the object in the scene.
        /// </summary>
        private void ApplyMovement()
        {
            Vector3 movement = transform.TransformDirection(_currentVelocity) * Time.deltaTime;
            _collisionFlags = _controller.Move(movement);

            ApplyTilt();
        }

        /// <summary>
        /// Applies gravity to the current movement vector and updates the character's position accordingly.
        /// </summary>
        private void ApplyGravity()
        {
            _collisionFlags |= _controller.Move(_appliedMovement * Time.deltaTime);
        }

        #endregion

        #region Edge Detection & Ground Check

        /// <summary>
        /// Determines whether the character is grounded or sliding based on the current collision state and ground
        /// conditions.
        /// </summary>
        /// <remarks>
        /// This method is <b>experimental</b> and does not handle all cases correctly yet
        /// </remarks>
        private void GroundCheck()
        {
            // If the character is currently jumping, it cannot be grounded
            if (IsJumping)
            {
                _isGrounded = false;
                return;
            }

            // Check if the controller reports contact with the ground
            bool isOverGround = (_collisionFlags & CollisionFlags.Below) != 0;

            // Process only if we are above valid ground and have a current ground hit
            if (isOverGround && _currentGroundHit != null)
            {
                Vector3 groundCheckOrigin = transform.position + Vector3.up * _controller.radius;
                Vector3 directionToHit = _currentGroundHit.Point - groundCheckOrigin;

                float angle = GetAngle(_lastGroundHit);

                // Case 1: we were not grounded in the last frame
                if (_lastGroundHit == null)
                {
                    // If the ground angle is too steep, start sliding
                    if (angle > _maxEdgeStickAngle)
                    {
                        _isSliding = true;
                        _isGrounded = false;

                        Vector3 pushDirection = -directionToHit;
                        pushDirection.y = 0f;
                        _controller.Move(_edgePushForce * Time.deltaTime * pushDirection);
                        _lastGroundHit = null;
                    }
                    // Otherwise, the ground is valid -- mark controller as grounded
                    else
                    {
                        _isGrounded = true;
                        _isSliding = false;
                        TimeSinceLastGrounded = 0f;
                        _lastGroundHit = new(_currentGroundHit);
                    }
                }
                // Case 2: we were grounded in the last frame
                else
                {
                    // If we stepped onto a different collider, reset ground state
                    if (_lastGroundHit.Collider != _currentGroundHit.Collider)
                    {
                        _isGrounded = true;
                        _isSliding = false;
                        TimeSinceLastGrounded = 0f;
                    }
                    else
                    {
                        // Check if the surface angle exceeds the maximum stick angle
                        if (angle > _maxEdgeStickAngle)
                        {
                            _isSliding = true;
                            _isGrounded = false;

                            // Apply a small push to prevent sticking to steep edges
                            Vector3 pushDirection = -directionToHit;
                            pushDirection.y = 0f;
                            _controller.Move(_edgePushForce * Time.deltaTime * pushDirection);

                            _lastGroundHit = null;
                        }
                        else
                        {
                            // Surface is valid -- maintain grounded state
                            _isGrounded = true;
                            _isSliding = false;
                            TimeSinceLastGrounded = 0f;
                            _lastGroundHit = new(_currentGroundHit);
                        }
                    }
                }
            }
            // Case 3: No valid ground below
            else
            {
                _isGrounded = false;
                TimeSinceLastGrounded += Time.deltaTime;

                // Clear ground reference if the coyote time has elapsed
                if (TimeSinceLastGrounded > _coyoteTime)
                {
                    _lastGroundHit = null;
                }
            }
        }

        private float GetAngle(ControllerHit hit)
        {
            if (hit == null)
                return -1f;

            Vector3 groundCheckOrigin = transform.position + Vector3.up * _controller.radius;
            Vector3 directionToHit = hit.Point - groundCheckOrigin;

            if (directionToHit.sqrMagnitude < 1E-6f)
            {
                return 0f;
            }

            return Vector3.Angle(Vector3.down, directionToHit);
        }

        /// <summary>
        /// Updates the grounded state of the character using the built-in CharacterController information.
        /// </summary>
        /// <remarks>
        /// This is the <b>current stable implementation</b> for grounded checks.
        /// A more detailed but <b>experimental</b> version exists in <see cref="GroundCheck"/> and may replace this method in the future.
        /// </remarks>
        private void UpdateGroundedStatus()
        {
            // CharacterController reports grounded status directly
            if (_controller.isGrounded)
            {
                TimeSinceLastGrounded = 0f;
                _isGrounded = true;
            }
            else
            {
                // When airborne, increment grounded timer and apply coyote time logic
                TimeSinceLastGrounded += Time.deltaTime;
                _isGrounded = TimeSinceLastGrounded <= _coyoteTime;
            }
        }

        /// <summary>
        /// Updates the capsule collider's height and center to match the target height.
        /// </summary>
        /// <remarks>This method adjusts the height of the capsule collider over time using a smooth
        /// transition and recalculates the center position to ensure it remains aligned with the updated
        /// height.</remarks>
        private void UpdateCapsuleCollider()
        {
            _controller.height = Mathf.MoveTowards(_controller.height, ControllerHeight, 2f * Time.deltaTime);
            _controller.center = Vector3.up * _controller.height / 2f;
        }

        #endregion

        #region Animations

        /// <summary>
        /// Applies a visual tilt to the character based on movement input and running state.
        /// </summary>
        /// <remarks>
        /// Adds a subtle roll (Z-axis rotation) during movement to make character motion feel more dynamic.
        /// The tilt only applies while the character is grounded.
        /// </remarks>
        private void ApplyTilt()
        {
            // Skip tilt when airborne
            if (!_isGrounded)
                return;

            // Use raw input for responsive tilt — stronger when running
            float targetTilt = -MoveInput.x * (IsRunning ? 1f : 0.25f) * _turnTiltAngle;

            Quaternion targetRotation = Quaternion.Euler(0f, transform.eulerAngles.y, targetTilt);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        /// <summary>
        /// Updates animator parameters based on the current movement state and velocity.
        /// </summary>
        /// <remarks>
        /// This method ensures smooth transitions between movement animations (idle, walk, run, crouch, slide).
        /// It uses velocity-based blending for natural motion.
        /// </remarks>
        private void ApplyAnimations()
        {
            float dampTime = CurrentSpeed > TargetSpeed ? 0.15f : 0.2f;
            float moveFactor = TargetSpeed > 0 ? CurrentSpeed / TargetSpeed : 0f;
            float runMultiplier = IsRunning ? 2f : 1f;

            // Normalize velocity relative to target speed for consistent animation scaling
            float targetX = _currentVelocity.x / Mathf.Max(TargetSpeed, 0.01f) * moveFactor;
            float targetY = _currentVelocity.z / Mathf.Max(TargetSpeed, 0.01f) * moveFactor * runMultiplier;

            _animator.SetFloat("moveX", targetX, dampTime, Time.deltaTime);
            _animator.SetFloat("moveY", targetY, dampTime, Time.deltaTime);

            _animator.SetBool("isCrouching", IsCrouching);
            _animator.SetBool("isSliding", IsSliding);
        }

        #endregion
    }
}