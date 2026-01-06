using System.Collections;
using System.Net;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class DashController : MonoBehaviour
{
    #region References

    [Header("-References-")] //References
    [SerializeField] CameraMovement _cameraMovement;
    [SerializeField] LayerMask _solidGround;
    PlayerMovement _playerMovement;
    Rigidbody2D _rb2d;
    Collider2D _collider2d;
    EnemyManager _enemyManager;

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

    public bool DashHold { get; private set; } = false; //Prevents sudden animation change when dash is over in air

    bool _dashIsHappening = false;

    //Auxiliaries
    private Coroutine _dashCoroutine;
    private Coroutine _releaseDashCoroutine;

    #endregion

    #region Event Functions

    private void Start()
    {
        //Component references
        _playerMovement = GetComponent<PlayerMovement>();
        _rb2d = GetComponent<Rigidbody2D>();
        _collider2d = GetComponentInChildren<Collider2D>();
        _enemyManager = EnemyManager.Instance;

        //ChargeDash action setup
        _chargeDashAction = InputSystem.actions.FindAction("ChargeDash");
        _chargeDashAction.performed += OnChargeDashPerformed;

        //ReleaseDash action setup
        _releaseDashAction = InputSystem.actions.FindAction("ReleaseDash");
        _releaseDashAction.performed += OnReleaseDashPerformed;

        //DashJump action setup
        _dashJumpAction = InputSystem.actions.FindAction("Jump");
    }

    private void OnDestroy()
    {
        _chargeDashAction.performed -= OnChargeDashPerformed;
        _releaseDashAction.performed -= OnReleaseDashPerformed;
    }

    private void OnDrawGizmos()
    {
        if (_playerMovement == null) return; // evita erro no editor

        // mesma posição usada no OverlapCircle
        Vector3 checkPos = transform.position +
                           (Vector3.right * _starSpawnDistance * _playerMovement.PlayerOrientation);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(checkPos, 0.5f);
    }

    #endregion

    #region Methods

    //Called input actions
    private void OnChargeDashPerformed(InputAction.CallbackContext context) => _dashCoroutine = StartCoroutine(ChargeDash());
    private void OnReleaseDashPerformed(InputAction.CallbackContext context) => _releaseDashCoroutine = StartCoroutine(ReleaseDash());

    private IEnumerator ChargeDash()
    {
        //Check if there's space to spawn star
        bool hitWall = Physics2D.OverlapCircle(transform.position + (Vector3.right * _starSpawnDistance * _playerMovement.PlayerOrientation), 0.5f, _solidGround);
        if (hitWall)
            yield break;

        //Can't start dash if mid air
        if (!_playerMovement.IsGrounded)
            yield break;

        //Can't start dash if already dashing
        if (IsDashing)
            yield break;

        if (_dashIsHappening)
            yield break;

        //Preparing for charging the dash - time for animation to complete
        IsPreparing = true;
        IsCharging = false;

        //Instantiates star
        if (_starRef != null)
            Destroy(_starRef.gameObject);

        _starRef = Instantiate(_starPrefab,
                               transform.position + (Vector3.right * _starSpawnDistance * _playerMovement.PlayerOrientation),
                               Quaternion.identity);

        AudioManager.Instance.Play("Star");

        //Positions camera according to side of charge
        if (_playerMovement.PlayerOrientation == -1 && _cameraMovement.CurrFocus == Side.Left)
        {
            _cameraMovement.StartShiftCoroutine(Side.Right);
        }
        else if (_playerMovement.PlayerOrientation == 1 && _cameraMovement.CurrFocus == Side.Right)
        {
            _cameraMovement.StartShiftCoroutine(Side.Left);
        }

        //Time for start to spawn
        float waitLength = _readyAnim.length > _starAppearAnim.length ? _readyAnim.length : _starAppearAnim.length;
        float timeElapsed = 0f;
        while (timeElapsed < waitLength)
        {
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        AudioManager.Instance.Play("Charge");

        //Actual charging of the dash
        IsPreparing = false;
        IsCharging = true;

        //Moves the star until it reaches the maximum dash distance
        float playerStartDist = Mathf.Abs(_starRef.position.x - transform.position.x);
        bool starHitWall = false;
        while (!starHitWall && playerStartDist < _dashMaxDistance && _starRef != null)
        {
            int orientation = _playerMovement.PlayerOrientation;

            //Checks for wall
            starHitWall = Physics2D.Raycast(_starRef.position, Vector2.right * orientation, 0.5f, _solidGround);

            Vector3 newPos = new Vector3(_starRef.position.x + (_dashStarSpeed * Time.deltaTime * orientation),
                                         _starRef.position.y, _starRef.position.z);

            _starRef.position = newPos;
            playerStartDist = Mathf.Abs(_starRef.position.x - transform.position.x);

            yield return null;
        }

        _dashCoroutine = null;
    }

    private IEnumerator ReleaseDash()
    {
        AudioManager.Instance.StopPlaying("Charge");

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

            Vector3 startingPoint = transform.position;

            _dashIsHappening = true;

            if (_starRef != null)
            {
                //STAGE 1 - Locks player & camera
                _cameraMovement.EnableHMovement(false); //Stop camera
                _cameraMovement.SetSmoothTimeX(0.4f); //Makes camera slower for when it moves
                _playerMovement.EnableMovement(false); //Disables player movement
                _playerMovement.EnableGravity(false); //Disables player gravity
                _enemyManager.EnableEnemyMovement(false); //Disables enemy movement

                IsDashing = true;
                DashHold = true;
                Vector3 newPlayerPos = _starRef.position;

                Destroy(_starRef.gameObject);

                AudioManager.Instance.Play("Disappear");

                yield return new WaitForSeconds(_disappearAnim.length); //Waits until disappearing animation is complete

                //STAGE 2 - Teleports player
                Vector3 posIfWaller;
                if (CheckForWaller(transform.position, newPlayerPos, out posIfWaller))
                {
                    newPlayerPos = posIfWaller - _playerMovement.PlayerOrientation * Vector3.right * GetComponentInChildren<BoxCollider2D>().size.x / 4;
                }

                transform.position = newPlayerPos;
                Physics2D.SyncTransforms();

                //Checks if player is inside a collider
                RaycastHit2D hit = Physics2D.BoxCast(_collider2d.bounds.center, new Vector2(_collider2d.bounds.size.x, 0.01f), 0f, Vector2.down, 10f, _solidGround);

                Debug.DrawRay(_collider2d.bounds.center, Vector2.down * 10f, Color.blue, 3f, false);

                if (hit)
                {
                    float distance = Mathf.Abs(hit.point.y - _collider2d.bounds.center.y);
                    float dif = distance - _collider2d.bounds.size.y / 2;

                    if (dif < 0)
                    {
                        transform.position = new Vector2(transform.position.x, transform.position.y + Mathf.Abs(dif));
                    }
                }


                IsDashing = false;

                AudioManager.Instance.Play("Cut");

                yield return new WaitForSeconds(_delayUntilDashJumpStart); //Delimits the start of the dash jump interval

                AudioManager.Instance.Play("Appear");

                //STAGE 3 - DASH JUMP INTERVAL
                HasDashJumped = false;
                //GetComponentInChildren<SpriteRenderer>().color = Color.red;

                float elapsedTime = 0;
                while (elapsedTime < _dashJumpInterval) //Dash jump interval
                {
                    if (_dashJumpAction.WasPressedThisFrame() && _playerMovement.IsGrounded) //Player dash jumps
                    {
                        DashHold = false;
                        HasDashJumped = true;

                        //Camera, player physics and gravity are unlocked
                        _cameraMovement.EnableHMovement(true);
                        _playerMovement.EnableGravity(true);
                        _playerMovement.EnablePhysics(true);

                        _enemyManager.EnableEnemyMovement(true);
                        HitDashables(startingPoint, newPlayerPos);

                        //Applies dash jump velocity on player
                        Vector3 direction = new Vector3(_playerMovement.PlayerOrientation * 1, 1, 0).normalized;
                        _rb2d.linearVelocity = direction * _dashJumpImpulse;

                        _dashIsHappening = false;

                        break;
                    }
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                //GetComponentInChildren<SpriteRenderer>().color = Color.white;

                yield return new WaitForSeconds(_movementCancellingDelay); //Interval until player can move again

                //STAGE 4 - DASH END
                if (!HasDashJumped) //This will already be enabled if player has dash jumped
                {
                    DashHold = false;
                    _playerMovement.EnableGravity(true);
                    _cameraMovement.EnableHMovement(true);

                    _enemyManager.EnableEnemyMovement(true);
                    HitDashables(startingPoint, newPlayerPos);

                    _dashIsHappening = false;
                }

                HasDashJumped = false;

                //Disables physics and enables player control
                _playerMovement.EnablePhysics(false);
                _playerMovement.EnableMovement(true);

                yield return new WaitForSeconds(1f); //Interval until camera's smooth X returns to normal

                StartCoroutine(_cameraMovement.SmoothChangeSmoothX(0.1f, 2f));
            }
        }

        _releaseDashCoroutine = null;
    }

    public void CancelDash()
    {
        AudioManager.Instance.StopPlaying("Charge");

        _dashIsHappening = false;

        if (_starRef != null)
            Destroy(_starRef.gameObject);

        if (_dashCoroutine != null)
        {
            StopCoroutine(_dashCoroutine);

            IsPreparing = false;
            IsCharging = false;

            _dashCoroutine = null;
        }
        if (_releaseDashCoroutine != null && !HasDashJumped)
        {
            StopCoroutine(_releaseDashCoroutine);

            if (_starRef != null)
                Destroy(_starRef.gameObject);

            IsPreparing = false;
            IsCharging = false;
            IsDashing = false;

            _cameraMovement.SetSmoothTimeX(0.1f);
            _playerMovement.EnableGravity(true);
            _cameraMovement.EnableHMovement(true);
            _enemyManager.EnableEnemyMovement(true);
            _playerMovement.EnablePhysics(false);
            _playerMovement.EnableMovement(true);

            _releaseDashCoroutine = null;
        }
    }

    public void SetFriction(float frictionVal)
    {
        _collider2d.enabled = false;
        _rb2d.sharedMaterial.friction = frictionVal;
        _collider2d.enabled = true;
    }

    public void HitDashables(Vector3 startingPoint, Vector3 endPoint)
    {
        RaycastHit2D[] hitColliders = Physics2D.LinecastAll(startingPoint, endPoint);
        foreach (RaycastHit2D hit in hitColliders)
        {
            IDashable dashable = hit.collider.gameObject.GetComponent<IDashable>();
            if (dashable == null)
                dashable = hit.collider.gameObject.GetComponentInParent<IDashable>();
            if (dashable != null)
            {
                dashable.OnDashedThrough(this);
                if (dashable is Enemy)
                {
                    AudioManager.Instance.Play("SwordHit");
                    AudioManager.Instance.Play("EnemyHit");
                }
            }
                
        }
    }

    private bool CheckForWaller(Vector3 startingPoint, Vector3 endPoint, out Vector3 wallerPosition)
    {
        wallerPosition = Vector3.zero;
        RaycastHit2D[] hitColliders = Physics2D.LinecastAll(startingPoint, endPoint);
        foreach (RaycastHit2D hit in hitColliders)
        {
            Enemy enemy = hit.collider.gameObject.GetComponentInParent<Enemy>();
            if (enemy != null && enemy is WallerEnemy waller && waller.IsWall)
            {
                wallerPosition = enemy.transform.position;
                return true;
            }
        }
        return false;
    }

    public void IncrementDashDistance(float increment)
    {
        _dashMaxDistance += increment;
    }

    #endregion
}
