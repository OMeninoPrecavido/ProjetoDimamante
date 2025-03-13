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

    //Serialized references
    [SerializeField] Transform floorCheck;
    [SerializeField] LayerMask floorMask;

    //Component References
    Rigidbody2D rb2d;
    BoxCollider2D bc2d;

    //Input Actions
    InputAction moveAction;
    InputAction lookAction;
    InputAction jumpAction;

    #endregion

    #region Properties

    //Serialized Properties
    [SerializeField] float speed;
    [SerializeField] float acceleration;
    [SerializeField] float deceleration;
    [SerializeField] float jumpForce;
    [SerializeField] float apexTime;
    [SerializeField] float apexSpeed;
    [SerializeField] float jumpEndEarlyDivisor;
    [SerializeField] float jumpBufferingTime;
    [SerializeField] float coyoteTime;

    //Player states
    bool isGrounded;
    bool wasGrounded;
    bool hasJumped;
    bool isFalling;
    bool isAffectedByPhysics;
    bool hasJumpBuffered;
    bool shouldCoyoteJump;
    bool shouldCheckGrounding = true;


    //Value holders
    float hOrientation;
    float previousHOrientation;
    float previousVOrientation;

    //Auxiliaries
    Coroutine jumpBufferingCoroutine;

    #endregion

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        bc2d = GetComponentInChildren<BoxCollider2D>();

        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    void Update()
    {
        ReadHorizontalMovement();
        HandleJump();
    }

    void FixedUpdate()
    {
        HandleHorizontalMovement();  
    }

    #region Horizontal Movement

    private void ReadHorizontalMovement()
    {
        hOrientation = moveAction.ReadValue<float>();
    }

    private void HandleHorizontalMovement()
    {
        if (!isAffectedByPhysics)
        {
            float velX = rb2d.linearVelocityX; //Current Rigidbody2D horizontal velocity

            if (hOrientation != 0f) //Player wants to move
            {
                if (Mathf.Abs(velX) < speed) //Player hasn't reached full speed or is still
                    velX = Accelerate(velX, hOrientation); //Accelerate

                if (Mathf.Abs(velX) > speed) //Player's velocity is over its expected speed
                    velX = Decelerate(velX, speed); //Decelerate back to current speed

                if (hOrientation != previousHOrientation) //Player is moving and wants to change direction
                    velX = 0;

                previousHOrientation = hOrientation;
            }
            else if (hOrientation == 0f) //Player wants to stay still
            {
                if (Mathf.Abs(velX) > 0f) //Player hasn't yet stopped
                    velX = Decelerate(velX, 0); //Decelerate to 0 speed
            }

            rb2d.linearVelocityX = velX;
        }
    }

    private float Accelerate(float currSpeed, float hOrientation)
    {
        float velX = currSpeed;

        float addToVelX = Time.deltaTime * acceleration * hOrientation;

        if (Mathf.Abs(addToVelX) < SPEED_ADD_THRESHOLD)
            addToVelX = SPEED_ADD_THRESHOLD * hOrientation;

        velX += addToVelX;

        if (Mathf.Abs(velX) > speed)
            velX = speed * hOrientation;

        return velX;
    }

    private float Decelerate(float currSpeed, float positiveTargetSpeed)
    {
        float velX = currSpeed;

        float subtractFromVelX = Time.deltaTime * acceleration * previousHOrientation;

        if (Mathf.Abs(subtractFromVelX) < SPEED_ADD_THRESHOLD)
            subtractFromVelX = SPEED_ADD_THRESHOLD * previousHOrientation;

        velX -= subtractFromVelX;

        if (positiveTargetSpeed > 0)
        {
            if (Mathf.Abs(velX) < speed)
                velX = speed * previousHOrientation;
        }
        else
        {
            if (Mathf.Sign(velX) != MathF.Sign(previousHOrientation))
                velX = 0;
        }

        return velX;
    }

    #endregion

    #region Vertical Movement

    private void HandleJump()
    {
        if (shouldCheckGrounding)
            isGrounded = CheckGrounding(); //Checks if player is grounded

        if (isGrounded)
        {
            hasJumped = false; //If player is grounded, obviously hasJumped == false
            isFalling = false; //Same for isFalling
        }

        if (!hasJumped && wasGrounded && !isGrounded && !shouldCoyoteJump) //Player dropped from a platform
        {
            StartCoroutine(CoyoteTime(coyoteTime));
        }

        if (!isGrounded && jumpAction.WasPressedThisFrame()) //Jump buffering. Player pressed jump but still isn't grounded
        {
            if (jumpBufferingCoroutine != null)
                StopCoroutine(jumpBufferingCoroutine);

            jumpBufferingCoroutine = StartCoroutine(JumpBuffering(jumpBufferingTime));
        }

        if ((isGrounded && jumpAction.WasPressedThisFrame()) || hasJumpBuffered || shouldCoyoteJump) //Player pressed for jump while grounded
        {
            hasJumped = true;
            isGrounded = shouldCheckGrounding = false;
            hasJumpBuffered = false;
            shouldCoyoteJump = false;

            rb2d.linearVelocityY = jumpForce; //Adds vertical velocity to the player
        }

        if (hasJumped && !isFalling && jumpAction.WasReleasedThisFrame()) //Player has released the jump button early
        {
            rb2d.linearVelocityY = rb2d.linearVelocityY / jumpEndEarlyDivisor;
        }

        if (hasJumped && !isFalling && Mathf.Sign(rb2d.linearVelocityY) < previousVOrientation) //Player has reached the peak of a jump
        {
            shouldCheckGrounding = true; //Allows the ground check only after mid-jump so there's no risk of hasJumped becoming true prematurely
            isFalling = true;
            StartCoroutine(ApexTime(apexTime));
        }

        previousVOrientation = Mathf.Sign(rb2d.linearVelocityY);
        wasGrounded = isGrounded;
    }

    private IEnumerator ApexTime(float secondsInZeroGravity)
    {
        float normalSpeed = speed;
        float normalAcceleration = acceleration;

        rb2d.linearVelocityY = 0;
        rb2d.gravityScale = 0;

        speed = apexSpeed;
        acceleration = 50000;

        yield return new WaitForSeconds(secondsInZeroGravity);

        rb2d.gravityScale = GRAVITY_SCALE;
        speed = normalSpeed;
        acceleration = normalAcceleration;
    }

    private IEnumerator JumpBuffering(float bufferSeconds)
    {
        for (float i = 0; i < bufferSeconds; i += Time.deltaTime)
        {
            if (isGrounded)
            {
                hasJumpBuffered = true;
                break;
            }
                
            yield return null;
        }   
    }

    private IEnumerator CoyoteTime(float seconds)
    {
        for (float i = 0; i < seconds; i += Time.deltaTime)
        {
            if (jumpAction.WasPressedThisFrame())
            {
                shouldCoyoteJump = true;
                break;
            }

            yield return null;
        }
    }

    private bool CheckGrounding()
    {
        RaycastHit2D hit; //RaycastHit2D may also operate as a boolean value. True = It hit something

        hit = Physics2D.BoxCast(floorCheck.position, new Vector2(bc2d.size.x, 0.01f), 0, Vector2.down, 0.1f, floorMask);

        //Debug.DrawRay(new Vector3(floorCheck.position.x - bc2d.size.x/2, floorCheck.position.y, floorCheck.position.z), Vector3.down * 0.1f, UnityEngine.Color.red, 0f, false);
        //Debug.DrawRay(new Vector3(floorCheck.position.x + bc2d.size.x/2, floorCheck.position.y, floorCheck.position.z), Vector3.down * 0.1f, UnityEngine.Color.red, 0f, false);

        return hit;
    }

    #endregion
}
