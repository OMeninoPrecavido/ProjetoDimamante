using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class DashController : MonoBehaviour
{
    #region References

    [Header("-References-")] //References
    [SerializeField] CameraMovement _cameraMovement;
    PlayerMovement _playerMovement;
    Rigidbody2D _rb2d;
    Collider2D _collider2d;

    [Header("-Animations-")] //Animations
    [SerializeField] AnimationClip _readyAnim;
    [SerializeField] AnimationClip _disappearAnim;
    [SerializeField] AnimationClip _appearAnim;
    [SerializeField] AnimationClip _starAppearAnim;

    [Header("-Star Prefab-")] //Prefabs
    [SerializeField] Transform _starPrefab;
    Transform _starRef;

    //Actions
    InputAction _chargeDashAction;
    InputAction _releaseDashAction;
    InputAction _dashJumpAction;

    #endregion

    #region Attributes

    [Header("-Attributes-")] //Attributes
    [SerializeField] float _dashMaxDistance;
    [SerializeField] float _dashStarSpeed;
    [SerializeField] float _dashJumpImpulse;
    [SerializeField] float _starSpawnDistance;

    [Header("-Intervals-")] //Intervals
    [SerializeField, Range(0f, 1f)] float _delayUntilDashJumpStart;
    [SerializeField, Range(0f, 1f)] float _dashJumpInterval;
    [SerializeField, Range(0f, 1f)] float _movementCancellingDelay;

    //States
    public bool IsPreparing { get; private set; } = false;
    public bool IsCharging { get; private set; } = false;
    public bool IsDashing { get; private set; } = false;
    public bool HasDashJumped { get; private set; } = false;

    //Auxiliaries
    private Coroutine _dashCoroutine;

    #endregion

    #region Event Functions

    private void Start()
    {
        //Component references
        _playerMovement = GetComponent<PlayerMovement>();
        _rb2d = GetComponent<Rigidbody2D>();
        _collider2d = GetComponentInChildren<Collider2D>();

        //ChargeDash action setup
        _chargeDashAction = InputSystem.actions.FindAction("ChargeDash");
        _chargeDashAction.performed += OnChargeDashPerformed;

        //ReleaseDash action setup
        _releaseDashAction = InputSystem.actions.FindAction("ReleaseDash");
        _releaseDashAction.performed += OnReleaseDashPerformed;

        //DashJump action setup
        _dashJumpAction = InputSystem.actions.FindAction("Jump");
    }

    #endregion

    #region Methods

    //Called input actions
    private void OnChargeDashPerformed(InputAction.CallbackContext context) => _dashCoroutine = StartCoroutine(ChargeDash());
    private void OnReleaseDashPerformed(InputAction.CallbackContext context) => StartCoroutine(ReleaseDash());

    private IEnumerator ChargeDash()
    {
        //Preparing for charging the dash - time for animation to complete
        IsPreparing = true;
        IsCharging = false;

        //Instantiates star
        if (_starRef != null)
            Destroy(_starRef.gameObject);

        _starRef = Instantiate(_starPrefab,
                               transform.position + (Vector3.right * _starSpawnDistance * _playerMovement.PlayerOrientation),
                               Quaternion.identity);

        float waitLength = _readyAnim.length > _starAppearAnim.length ? _readyAnim.length : _starAppearAnim.length;
        float timeElapsed = 0f;
        while (timeElapsed < waitLength)
        {
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        //Actual charging of the dash
        IsPreparing = false;
        IsCharging = true;

        //Moves the star unitl it reaches the maximum dash distance
        float playerStartDist = Mathf.Abs(_starRef.position.x - transform.position.x);
        while (playerStartDist < _dashMaxDistance && _starRef != null)
        {
            int orientation = _playerMovement.PlayerOrientation;
            Vector3 newPos = new Vector3(_starRef.position.x + (_dashStarSpeed * Time.deltaTime * orientation),
                                         _starRef.position.y, _starRef.position.z);

            _starRef.position = newPos;
            playerStartDist = Mathf.Abs(_starRef.position.x - transform.position.x);

            yield return null;
        }
    }

    private IEnumerator ReleaseDash()
    {
        if (IsPreparing)
        {
            IsPreparing = false;
            IsCharging = false;
            
            if(_starRef != null)
                Destroy(_starRef.gameObject);

            StopCoroutine(_dashCoroutine);
        }
        else if (IsCharging)
        {
            IsPreparing = false;
            IsCharging = false;

            if (_starRef != null)
            {
                //STAGE 1 - Locks player & camera
                _cameraMovement.EnableHMovement(false); //Stop camera
                _cameraMovement.SetSmoothTimeX(0.4f); //Makes camera slower for when it moves
                _playerMovement.EnableMovement(false); //Disables player movement
                _playerMovement.EnableGravity(false); //Disables player gravity

                IsDashing = true;
                Vector3 newPlayerPos = _starRef.position;

                Destroy(_starRef.gameObject);

                yield return new WaitForSeconds(_disappearAnim.length); //Waits until disappearing animation is complete

                //STAGE 2 - Teleports player
                transform.position = newPlayerPos;
                IsDashing = false;

                yield return new WaitForSeconds(_delayUntilDashJumpStart); //Delimits the start of the dash jump interval

                //STAGE 3 - DASH JUMP INTERVAL
                bool hasDashJumped = false;
                //GetComponentInChildren<SpriteRenderer>().color = Color.red;

                float elapsedTime = 0;
                while (elapsedTime < _dashJumpInterval) //Dash jump interval
                {
                    if (_dashJumpAction.WasPressedThisFrame()) //Player dash jumps
                    {
                        hasDashJumped = true;
                        HasDashJumped = true;

                        //Camera, player physics and gravity are unlocked
                        _cameraMovement.EnableHMovement(true);
                        _playerMovement.EnableGravity(true);
                        _playerMovement.EnablePhysics(true);

                        //Player friction set high so they don't slide when landing
                        //SetFriction(100f);

                        //Applies dash jump velocity on player
                        Vector3 direction = new Vector3(_playerMovement.PlayerOrientation * 1, 1, 0).normalized;
                        _rb2d.linearVelocity = direction * _dashJumpImpulse;

                        break;
                    }
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                //GetComponentInChildren<SpriteRenderer>().color = Color.white;

                yield return new WaitForSeconds(_movementCancellingDelay); //Interval until player can move again

                //STAGE 4 - DASH END
                if (!hasDashJumped) //This will already be enabled if player has dash jumped
                {
                _playerMovement.EnableGravity(true);
                _cameraMovement.EnableHMovement(true);
                }

                HasDashJumped = false;

                //Disables physics and enables player control
                _playerMovement.EnablePhysics(false);
                _playerMovement.EnableMovement(true);

                yield return new WaitForSeconds(1f); //Interval until camera's smooth X returns to normal

                StartCoroutine(_cameraMovement.SmoothChangeSmoothX(0.1f, 2f));
            }
        }
    }

    public void CancelDash()
    {
        if (_dashCoroutine != null)
        {
            StopCoroutine(_dashCoroutine);

            if (_starRef != null)
                Destroy(_starRef.gameObject);

            IsPreparing = false;
            IsCharging = false;
        }
    }

    public void SetFriction(float frictionVal)
    {
        _collider2d.enabled = false;
        _rb2d.sharedMaterial.friction = frictionVal;
        _collider2d.enabled = true;
    }

    #endregion
}
