using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static DashControllerOLD;

[RequireComponent(typeof(PlayerMovement))]
public class DashController : MonoBehaviour
{
    [Header("-References-")] //References
    [SerializeField] CameraMovement _cameraMovement;
    PlayerMovement _playerMovement;

    [Header("-Animations-")] //Animations
    [SerializeField] AnimationClip _readyAnim;
    [SerializeField] AnimationClip _disappearAnim;
    [SerializeField] AnimationClip _appearAnim;

    [Header("-Star Prefab-")] //Prefabs
    [SerializeField] Transform _starPrefab;
    Transform _starRef;

    [Header("-Attributes-")] //Attributes
    [SerializeField] float _dashMaxDistance;
    [SerializeField] float _dashStarSpeed;
    [SerializeField] float _movementCancellingDelay;

    //Actions
    InputAction _chargeDashAction;
    InputAction _releaseDashAction;

    //States
    public bool IsPreparing { get; private set; } = false;
    public bool IsCharging { get; private set; } = false;
    public bool IsDashing { get; private set; } = false;

    //Auxiliaries
    private Coroutine _dashCoroutine;

    #region Event Functions

    private void Start()
    {
        _playerMovement = GetComponent<PlayerMovement>();

        //ChargeDash action setup
        _chargeDashAction = InputSystem.actions.FindAction("ChargeDash");
        _chargeDashAction.performed += OnChargeDashPerformed;

        //ReleaseDash action setup
        _releaseDashAction = InputSystem.actions.FindAction("ReleaseDash");
        _releaseDashAction.performed += OnReleaseDashPerformed;
    }

    #endregion

    private void OnChargeDashPerformed(InputAction.CallbackContext context) => _dashCoroutine = StartCoroutine(ChargeDash());
    private void OnReleaseDashPerformed(InputAction.CallbackContext context) => StartCoroutine(ReleaseDash());

    private IEnumerator ChargeDash()
    {
        //Preparing for charging the dash - time for animation to complete
        IsPreparing = true;
        IsCharging = false;

        float timeElapsed = 0f;
        while (timeElapsed < _readyAnim.length)
        {
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        //Actual charging of the dash
        IsPreparing = false;
        IsCharging = true;

        _starRef = Instantiate(_starPrefab, transform.position, Quaternion.identity);

        float playerStartDist = Mathf.Abs(_starRef.position.x - transform.position.x);
        while (playerStartDist < _dashMaxDistance)
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

            StopCoroutine(_dashCoroutine);
        }
        else if (IsCharging)
        {
            IsPreparing = false;
            IsCharging = false;

            if (_starRef != null)
            {
                _cameraMovement.EnableHMovement(false);
                _cameraMovement.SetSmoothTimeX(0.4f);
                _playerMovement.EnableMovement(false);
                _playerMovement.EnableGravity(false);
                IsDashing = true;
                Vector3 newPlayerPos = _starRef.position;
                Destroy(_starRef.gameObject);

                yield return new WaitForSeconds(_disappearAnim.length);

                transform.position = newPlayerPos;
                IsDashing = false;

                yield return new WaitForSeconds(_appearAnim.length);


                yield return new WaitForSeconds(_movementCancellingDelay);
                _playerMovement.EnableGravity(true);
                _cameraMovement.EnableHMovement(true);
                _playerMovement.EnableMovement(true);

                yield return new WaitForSeconds(1f);
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
}
