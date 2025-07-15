using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    #region Constants

    const float SPEED_ADD_THRESHOLD = 0.2f;
    const float GRAVITY_SCALE = 4.5f;

    #endregion

    #region References

    //Floor Check
    [Header("-Floor Check-")]
    [SerializeField] Transform _floorCheck;
    [SerializeField] LayerMask _floorMask;

    //Component References
    Rigidbody2D _rb2d;
    BoxCollider2D _bc2d;
    DashController _dashController;

    //Input Actions
    InputAction _moveAction;
    InputAction _jumpAction;

    #endregion

    #region Attributes

    //Serialized Attributes
    [Header("-Horizontal Attributes-")]
    [SerializeField] float _speed;
    [SerializeField] float _acceleration;
    [SerializeField] float _deceleration;

    [Header("-Vertical Attributes")]
    [SerializeField] float _jumpForce;
    [SerializeField] float _apexTime;
    [SerializeField] float _apexSpeed;
    [SerializeField] float _jumpEndEarlyDivisor;
    [SerializeField] float _jumpBufferingTime;
    [SerializeField] float _coyoteTime;

    //Player states
    public bool IsGrounded { get; private set; }
    public bool WasGrounded { get; private set; }
    public bool HasJumped { get; private set; }
    public bool IsFalling { get; private set; }
    public bool IsAffectedByPhysics { get; private set; }
        public void EnablePhysics(bool b) => IsAffectedByPhysics = b;
    public bool HasJumpBuffered { get; private set; }
    public bool ShouldCoyoteJump { get; private set; }
    public bool ShouldCheckGrounding { get; private set; } = true;
    public bool IsMovementEnabled { get; private set; } = true;
        public void EnableMovement(bool b) => IsMovementEnabled = b;

    //Value holders
    public float HOrientation { get; private set; } //Movement orientation -> can be 0
    float _previousHOrientation;
    float _previousVOrientation;

    public int PlayerOrientation { get; private set; } = 1; //-1 = Left, 1 = Right

    //Auxiliaries
    Coroutine _jumpBufferingCoroutine;

    #endregion

    void Start()
    {
        //Components setup
        _rb2d = GetComponent<Rigidbody2D>();
        _bc2d = GetComponentInChildren<BoxCollider2D>();
        _dashController = GetComponent<DashController>();

        //Actions setup
        _moveAction = InputSystem.actions.FindAction("Move");
        _jumpAction = InputSystem.actions.FindAction("Jump");
    }

    void Update()
    {
        if (IsMovementEnabled)
        {
            ReadHorizontalMovement();
            HandleJump();
        }
    }

    void FixedUpdate()
    {
        if (IsMovementEnabled)
            HandleHorizontalMovement();  
    }

    #region Horizontal Movement

    private void ReadHorizontalMovement()
    {
        HOrientation = _moveAction.ReadValue<float>();
    }

    private void HandleHorizontalMovement()
    {
        if (!IsAffectedByPhysics)
        {
            float velX = _rb2d.linearVelocityX; //Current Rigidbody2D horizontal velocity

            if (HOrientation != 0f) //Player wants to move
            {
                if (Mathf.Abs(velX) < _speed) //Player hasn't reached full speed or is still
                    velX = Accelerate(velX, HOrientation); //Accelerate

                if (Mathf.Abs(velX) > _speed) //Player's velocity is over its expected speed
                    velX = Decelerate(velX, _speed); //Decelerate back to current speed

                if (HOrientation != _previousHOrientation) //Player is moving and wants to change direction
                    velX = 0;

                _previousHOrientation = HOrientation;
                PlayerOrientation = (int)HOrientation;

                _dashController.CancelDash();
            }
            else if (HOrientation == 0f) //Player wants to stay still
            {
                if (Mathf.Abs(velX) > 0f) //Player hasn't yet stopped
                    velX = Decelerate(velX, 0); //Decelerate to 0 speed
            }

            _rb2d.linearVelocityX = velX;
        }
    }

    private float Accelerate(float currSpeed, float hOrientation)
    {
        float velX = currSpeed;

        float addToVelX = Time.deltaTime * _acceleration * hOrientation;

        if (Mathf.Abs(addToVelX) < SPEED_ADD_THRESHOLD)
            addToVelX = SPEED_ADD_THRESHOLD * hOrientation;

        velX += addToVelX;

        if (Mathf.Abs(velX) > _speed)
            velX = _speed * hOrientation;

        return velX;
    }

    private float Decelerate(float currSpeed, float positiveTargetSpeed)
    {
        float velX = currSpeed;

        float subtractFromVelX = Time.deltaTime * _deceleration * _previousHOrientation;

        if (Mathf.Abs(subtractFromVelX) < SPEED_ADD_THRESHOLD)
            subtractFromVelX = SPEED_ADD_THRESHOLD * _previousHOrientation;

        velX -= subtractFromVelX;

        if (positiveTargetSpeed > 0)
        {
            if (Mathf.Abs(velX) < positiveTargetSpeed)
                velX = positiveTargetSpeed * _previousHOrientation;
        }
        else
        {
            if (Mathf.Sign(velX) != MathF.Sign(_previousHOrientation))
                velX = 0;
        }

        return velX;
    }

    #endregion

    #region Vertical Movement

    private void HandleJump()
    {
        if (ShouldCheckGrounding)
            IsGrounded = CheckGrounding(); //Checks if player is grounded

        if (IsGrounded)
        {
            HasJumped = false; //If player is grounded, obviously hasJumped == false
            IsFalling = false; //Same for isFalling
        }
        else
        {
            _dashController.CancelDash();
        }

        if (!HasJumped && WasGrounded && !IsGrounded && !ShouldCoyoteJump) //Player dropped from a platform
        {
            IsFalling = true;
            StartCoroutine(CoyoteTime(_coyoteTime));
        }

        if (!IsGrounded && _jumpAction.WasPressedThisFrame()) //Jump buffering. Player pressed jump but still isn't grounded
        {
            if (_jumpBufferingCoroutine != null)
                StopCoroutine(_jumpBufferingCoroutine);

            _jumpBufferingCoroutine = StartCoroutine(JumpBuffering(_jumpBufferingTime));
        }

        if ((IsGrounded && _jumpAction.WasPressedThisFrame()) || HasJumpBuffered || ShouldCoyoteJump) //Player pressed for jump while grounded
        {
            HasJumped = true;
            IsGrounded = ShouldCheckGrounding = false;
            HasJumpBuffered = false;
            ShouldCoyoteJump = false;

            _rb2d.linearVelocityY = _jumpForce; //Adds vertical velocity to the player
        }

        if (HasJumped && !IsFalling && _jumpAction.WasReleasedThisFrame()) //Player has released the jump button early
        {
            _rb2d.linearVelocityY = _rb2d.linearVelocityY / _jumpEndEarlyDivisor;
        }

        if (HasJumped && !IsFalling && Mathf.Sign(_rb2d.linearVelocityY) < _previousVOrientation) //Player has reached the peak of a jump
        {
            ShouldCheckGrounding = true; //Allows the ground check only after mid-jump so there's no risk of hasJumped becoming true prematurely
            IsFalling = true;
            StartCoroutine(ApexTime(_apexTime));
        }

        _previousVOrientation = Mathf.Sign(_rb2d.linearVelocityY);
        WasGrounded = IsGrounded;
    }

    private IEnumerator ApexTime(float secondsInZeroGravity)
    {
        float normalSpeed = _speed;
        float normalAcceleration = _acceleration;

        _rb2d.linearVelocityY = 0;
        _rb2d.gravityScale = 0;

        _speed = _apexSpeed;
        _acceleration = 50000;

        yield return new WaitForSeconds(secondsInZeroGravity);

        _rb2d.gravityScale = GRAVITY_SCALE;
        _speed = normalSpeed;
        _acceleration = normalAcceleration;
    }

    private IEnumerator JumpBuffering(float bufferSeconds)
    {
        for (float i = 0; i < bufferSeconds; i += Time.deltaTime)
        {
            if (IsGrounded)
            {
                HasJumpBuffered = true;
                break;
            }
                
            yield return null;
        }   
    }

    private IEnumerator CoyoteTime(float seconds)
    {
        for (float i = 0; i < seconds; i += Time.deltaTime)
        {
            if (_jumpAction.WasPressedThisFrame())
            {
                ShouldCoyoteJump = true;
                break;
            }

            yield return null;
        }
    }

    private bool CheckGrounding()
    {
        RaycastHit2D hit; //RaycastHit2D may also operate as a boolean value. True = It hit something

        hit = Physics2D.BoxCast(_floorCheck.position, new Vector2(_bc2d.size.x, 0.01f), 0, Vector2.down, 0.1f, _floorMask);

        //Debug.DrawRay(new Vector3(floorCheck.position.x - bc2d.size.x/2, floorCheck.position.y, floorCheck.position.z), Vector3.down * 0.1f, UnityEngine.Color.red, 0f, false);
        //Debug.DrawRay(new Vector3(floorCheck.position.x + bc2d.size.x/2, floorCheck.position.y, floorCheck.position.z), Vector3.down * 0.1f, UnityEngine.Color.red, 0f, false);

        return hit;
    }

    public void EnableGravity(bool b)
    {
        if (!b)
            _rb2d.constraints |= RigidbodyConstraints2D.FreezePositionY;
        else
        {
            _rb2d.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
            _rb2d.linearVelocityY = -0.01f;
        }
            
    }

    #endregion

}
