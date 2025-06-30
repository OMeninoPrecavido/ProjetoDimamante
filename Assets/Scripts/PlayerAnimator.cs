using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimator : MonoBehaviour
{
    const string ORIENTATION = "orientation";
    const string IS_RUNNING = "isRunning";
    const string IS_CHARGING = "isCharging";
    const string IS_JUMPING = "isJumping";
    const string IS_FALLING = "isFalling";
    const string DASH_STARTED = "dashStarted";

    Animator animator;
    PlayerMovement playerMovement;
    DashController dashController;

    InputAction chargeDashAction;

    bool isChargingDash = false;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        dashController = GetComponent<DashController>();

        chargeDashAction = InputSystem.actions.FindAction("ChargeDash");
        chargeDashAction.started += OnChargeDashStart;
        chargeDashAction.canceled += OnChargeDashCancel;
    }

    private void Update()
    {
        animator.SetInteger(ORIENTATION, playerMovement.PlayerOrientation);

        if (playerMovement.HOrientation != 0)
            animator.SetBool(IS_RUNNING, true);
        else
            animator.SetBool(IS_RUNNING, false);

        if (playerMovement.IsGrounded)
        {
            animator.SetBool(IS_FALLING, false);
            animator.SetBool(IS_JUMPING, false);
        }
        else if (playerMovement.HasJumped && !playerMovement.IsFalling)
        {
            animator.SetBool(IS_JUMPING, true);
            animator.SetBool(IS_FALLING, false);
        }
        else if (playerMovement.IsFalling)
        {
            animator.SetBool(IS_FALLING, true);
            animator.SetBool(IS_JUMPING, false);
        }

        animator.SetBool(IS_CHARGING, isChargingDash);
        animator.SetBool(DASH_STARTED, dashController.IsDashing);
    }

    private void OnChargeDashStart(InputAction.CallbackContext context) => isChargingDash = true;
    private void OnChargeDashCancel(InputAction.CallbackContext context) => isChargingDash = false;
}
