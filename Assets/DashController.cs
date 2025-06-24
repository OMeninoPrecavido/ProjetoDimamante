using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static DashControllerOLD;

[RequireComponent(typeof(PlayerMovement))]
public class DashController : MonoBehaviour
{
    [SerializeField] AnimationClip _readyAnim;

    [SerializeField] Transform _starPrefab;
    Transform _starRef;

    PlayerMovement _playerMovement;

    InputAction _chargeDashAction;
    InputAction _releaseDashAction;

    [SerializeField] float _dashMaxDistance;
    [SerializeField] float _dashStarSpeed;

    public bool IsPreparing { get; private set; }
    public bool IsCharging { get; private set; }

    private Coroutine _dashCoroutine;

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

    private void OnChargeDashPerformed(InputAction.CallbackContext context)
    {
        _dashCoroutine = StartCoroutine(ChargeDash());
    }

    private void OnReleaseDashPerformed(InputAction.CallbackContext context)
    {
        ReleaseDash();
    }

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

    private void ReleaseDash()
    {
        if (IsPreparing)
        {
            StopCoroutine(_dashCoroutine);
        }
        else if (IsCharging)
        {
            if (_starRef != null)
            {
                Vector3 newPlayerPos = _starRef.position;
                transform.position = newPlayerPos;

                Destroy(_starRef.gameObject);
            }
        }

        IsPreparing = false;
        IsCharging = false;
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
