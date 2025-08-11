using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class WalkerEnemy : Enemy
{
    [Header("-Neutral State-")]
    [SerializeField] float _walkingSpeed;
    [SerializeField] float _lowerWaitingLimit;
    [SerializeField] float _upperWaitingLimit;
    [SerializeField] float _lowerWalkingLimit;
    [SerializeField] float _upperWalkingLimit;

    [Header("-Hostile State-")]
    [SerializeField] float _runningSpeed;
    [SerializeField] float _noPlayerFoundTolerance;

    [Header("-Movement-")]
    [SerializeField] float _acceleration;
    [SerializeField] float _deceleration;

    [Header("-Checks-")]
    [SerializeField] float _sightRange;
    [SerializeField] LayerMask _pitCheckLayerMask;
    [SerializeField] LayerMask _playerCheckLayerMask;

    public bool IsSearching { get; private set; } = false;
    public bool SeesPlayer { get; private set; } = false;
    public bool IsMoving { get; private set; } = false;

    protected override void Start()
    {
        base.Start();
        CurrState = EnemyState.Neutral;
        StartCoroutine(CheckForPlayer());
        _currBehaviour = StartCoroutine(NeutralBehaviour());
    }

    private IEnumerator NeutralBehaviour()
    {
        while (CurrState == EnemyState.Neutral)
        {
            //Stays in place for random amount of time
            _rb2d.linearVelocityX = 0;
            IsMoving = false;
            float waitSeconds = UnityEngine.Random.Range(_lowerWaitingLimit, _upperWaitingLimit);
            yield return new WaitForSeconds(waitSeconds);

            //Chooses random orientation and walks in that direction for random amount of time
            int o = UnityEngine.Random.Range(0, 2);
            Orientation = o == 0 ? 1 : -1;
            float walkSeconds = UnityEngine.Random.Range(_lowerWalkingLimit, _upperWalkingLimit);
            float elapsedTime = 0;
            IsMoving = true;
            while (elapsedTime < walkSeconds)
            {
                CheckForPit();

                _rb2d.linearVelocityX = Accelerate(_rb2d.linearVelocityX, Orientation, _walkingSpeed);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            //Decelerates after it's done walking
            while (_rb2d.linearVelocityX > 0)
            {
                CheckForPit();
                _rb2d.linearVelocityX = Decelerate(_rb2d.linearVelocityX, 0);
                _previousOrientation = Orientation;
                yield return null;
            }
            IsMoving = false;

            yield return null;
        }
    }

    private IEnumerator HostileBehaviour()
    {
        IsMoving = true;
        while (CurrState == EnemyState.Hostile)
        {
            //While hostile, just keeps running
            CheckForPit();
            _rb2d.linearVelocityX = _runningSpeed * Orientation;
            yield return null;
        }
    }

    private IEnumerator CheckForPlayer()
    {
        while (true)
        {
            //Constantly checks if sees player
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector3.right * Orientation, _sightRange, _playerCheckLayerMask);
            Debug.DrawRay(transform.position, Vector3.right * Orientation * _sightRange, Color.red, 0f);

            PlayerMovement player = null;

            if (hit)
                player = hit.collider.GetComponentInParent<PlayerMovement>();

            SeesPlayer = player != null ? true : false;

            //Sees player and is neutral
            if (SeesPlayer && CurrState == EnemyState.Neutral)
            {
                CurrState = EnemyState.Hostile;
                StopCoroutine(_currBehaviour);
                _currBehaviour = StartCoroutine(HostileBehaviour());
            }
            //Doesn't see player, is hostile and isn't currently searching for player
            else if (!SeesPlayer && CurrState == EnemyState.Hostile && !IsSearching)
            {
                StartCoroutine(Search());
            }

            yield return null;
        }
    }

    private IEnumerator Search()
    {
        //Searches for the player for a few seconds before reverting to neutral state
        IsSearching = true;
        float elapsedTime = 0;
        while (elapsedTime < _noPlayerFoundTolerance)
        {
            if (SeesPlayer)
            {
                IsSearching = false;
                break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (IsSearching)
        {
            CurrState = EnemyState.Neutral;
            StopCoroutine(_currBehaviour);
            _currBehaviour = StartCoroutine(NeutralBehaviour());
        }
        IsSearching = false;
    }

    private void CheckForPit()
    {
        float pitCheckY = _boxCollider2d.bounds.center.y - _boxCollider2d.size.y / 2;
        float pitCheckX = _boxCollider2d.bounds.center.x + (_boxCollider2d.size.x / 2) * Orientation;
        Vector3 _pitCheckPos = new Vector3(pitCheckX, pitCheckY, 0);

        bool hit = Physics2D.Raycast(_pitCheckPos, Vector3.down, 0.3f, _pitCheckLayerMask);
        Debug.DrawRay(_pitCheckPos, Vector3.down * 0.3f, Color.red, 0f, false);

        if (!hit)
            Orientation = -Orientation;
    }

    private float Accelerate(float currSpeed, float hOrientation, float speed)
    {
        float velX = currSpeed;

        float addToVelX = Time.deltaTime * _acceleration * hOrientation;

        velX += addToVelX;

        if (Mathf.Abs(velX) > speed)
            velX = speed * hOrientation;

        return velX;
    }

    private float Decelerate(float currSpeed, float positiveTargetSpeed)
    {
        float velX = currSpeed;

        float subtractFromVelX = Time.deltaTime * _deceleration * _previousOrientation;

        velX -= subtractFromVelX;

        if (positiveTargetSpeed > 0)
        {
            if (Mathf.Abs(velX) < positiveTargetSpeed)
                velX = positiveTargetSpeed * _previousOrientation;
        }
        else
        {
            if (Mathf.Sign(velX) != MathF.Sign(_previousOrientation))
                velX = 0;
        }

        return velX;
    }
}
