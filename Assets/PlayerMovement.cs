using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    #region Constants

    const float SPEED_ADD_THRESHOLD = 0.2f;

    #endregion

    #region References

    //Serialized references
    [SerializeField] Transform floorCheck;
    [SerializeField] LayerMask floorMask;

    //Component References
    Rigidbody2D rb2d;

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

    //Auxiliaries
    bool isGrounded;
    bool hasJumped;
    bool shouldCheckGrounding = true;

    float previousHOrientation;
    float previousVOrientation;

    #endregion

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();

        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    void Update()
    {
        HandleHorizontalMovement();
        HandleJump();
    }

    private bool CheckGrounding()
    {
        RaycastHit2D hit; //RaycastHit2D may also operate as a boolean value. True = It hit something
        hit = Physics2D.Raycast(floorCheck.position, Vector2.down, 0.1f, floorMask);

        Debug.DrawRay(floorCheck.position, Vector3.down * 0.5f, UnityEngine.Color.red, 0f, false);

        return hit;
    }

    private void HandleHorizontalMovement()
    {
        float hOrientation = moveAction.ReadValue<float>(); //Orientation. Left = -1, Still = 0, Right = 1
        float velX = rb2d.linearVelocityX; //Current Rigidbody2D horizontal velocity

        if (hOrientation != 0f) //Player wants to move
        {
            if (Mathf.Abs(velX) < speed) //Player hasn't reached full speed or is still
            {
                //Accelerate
                float addToVelX = Time.deltaTime * acceleration * hOrientation;

                if (Mathf.Abs(addToVelX) < SPEED_ADD_THRESHOLD)
                    addToVelX = SPEED_ADD_THRESHOLD * hOrientation;

                velX += addToVelX;

                if (Mathf.Abs(velX) > speed)
                    velX = speed * hOrientation;
            }

            if (hOrientation != previousHOrientation) //Player is moving and wants to change direction
            {
                velX = 0;
            }

            previousHOrientation = hOrientation;
        }
        else if (hOrientation == 0f) //Player wants to stay still
        {
            if (Mathf.Abs(velX) > 0f) //Player hasn't yet stopped
            {
                //Decelerate
                float subtractFromVelX = Time.deltaTime * deceleration * previousHOrientation;

                if (Mathf.Abs(subtractFromVelX) < SPEED_ADD_THRESHOLD)
                    subtractFromVelX = SPEED_ADD_THRESHOLD * previousHOrientation;

                velX -= subtractFromVelX;

                if (Mathf.Sign(velX) != MathF.Sign(previousHOrientation))
                    velX = 0;
            }
        }

        rb2d.linearVelocityX = velX;
    }

    private void HandleJump()
    {
        if (shouldCheckGrounding)
            isGrounded = CheckGrounding(); //Checks if player is grounded

        if (isGrounded)
            hasJumped = false; //If player is grounded, obviously hasJumped == false

        if (isGrounded && jumpAction.WasPressedThisFrame()) //Player pressed for jump
        {
            hasJumped = true;
            isGrounded = shouldCheckGrounding = false;
            rb2d.linearVelocityY = jumpForce;
        }

        if (hasJumped && Mathf.Sign(rb2d.linearVelocityY) < previousVOrientation) //Player has reached the peak of a jump
        {
            shouldCheckGrounding = true; //Allows the ground check only after mid-jump so there's no risk of hasJumped becoming true prematurely
            Debug.Log("Peak!");
        }

        previousVOrientation = Mathf.Sign(rb2d.linearVelocityY);
    }
}
