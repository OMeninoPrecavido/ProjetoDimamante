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
    const string HAS_DASH_JUMPED = "hasDashJumped";
    const string IS_HIT = "isHit";

    Animator _animator;
    PlayerMovement _playerMovement;
    DashController _dashController;

    InputAction _chargeDashAction;

    bool _isChargingDash = false;

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _playerMovement = GetComponent<PlayerMovement>();
        _dashController = GetComponent<DashController>();

        _chargeDashAction = InputSystem.actions.FindAction("ChargeDash");
        _chargeDashAction.started += OnChargeDashStart;
        _chargeDashAction.canceled += OnChargeDashCancel;
    }

    private void Update()
    {
        _animator.SetInteger(ORIENTATION, _playerMovement.PlayerOrientation);

        if (_playerMovement.HOrientation != 0)
            _animator.SetBool(IS_RUNNING, true);
        else
            _animator.SetBool(IS_RUNNING, false);

        if (_playerMovement.IsGrounded)
        {
            _animator.SetBool(IS_FALLING, false);
            _animator.SetBool(IS_JUMPING, false);
        }
        else if (_playerMovement.HasJumped && !_playerMovement.IsFalling)
        {
            _animator.SetBool(IS_JUMPING, true);
            _animator.SetBool(IS_FALLING, false);
        }
        else if (_playerMovement.IsFalling)
        {
            _animator.SetBool(IS_FALLING, true);
            _animator.SetBool(IS_JUMPING, false);
        }

        _animator.SetBool(IS_HIT, _playerMovement.IsHit);
        _animator.SetBool(IS_CHARGING, _isChargingDash);
        _animator.SetBool(DASH_STARTED, _dashController.IsDashing);
        _animator.SetBool(HAS_DASH_JUMPED, _dashController.HasDashJumped);
    }

    private void OnChargeDashStart(InputAction.CallbackContext context) => _isChargingDash = true;
    private void OnChargeDashCancel(InputAction.CallbackContext context) => _isChargingDash = false;
}
