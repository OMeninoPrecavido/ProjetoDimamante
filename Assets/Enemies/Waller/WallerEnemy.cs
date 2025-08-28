using System;
using System.Collections;
using UnityEngine;

public class WallerEnemy : Enemy
{
    [Header("-Waiting Times-")]
    [SerializeField] float _lowerWaitingTime;
    [SerializeField] float _upperWaitingTime;
    [SerializeField] float _lowerWalkingTime;
    [SerializeField] float _upperWalkingTime;
    [SerializeField] float _lowerWallTime;
    [SerializeField] float _upperWallTime;

    [Header("-Movement-")]
    [SerializeField] float _speed;
    [SerializeField] float _acceleration;
    [SerializeField] float _deceleration;
    [SerializeField] LayerMask _pitCheckLayerMask;

    public bool IsWall { get; private set; } = false;
    public bool IsMoving { get; private set; } = false;

    protected override void Start()
    {
        base.Start();
        CurrState = EnemyState.Neutral;
        _currBehaviour = StartCoroutine(NeutralBehaviour());
    }

    private IEnumerator NeutralBehaviour()
    {
        while (CurrState == EnemyState.Neutral)
        {
            //Waiting
            _rb2d.linearVelocityX = 0;
            _rb2d.constraints = RigidbodyConstraints2D.FreezePositionX;

            float waitingTime = UnityEngine.Random.Range(_lowerWaitingTime, _upperWaitingTime);
            yield return new WaitForSeconds(waitingTime);

            //Chooses between transforming into wall and walking
            int choice = UnityEngine.Random.Range(0, 2);

            //Walking
            if (choice == 0)
            {
                _rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;

                //Chooses random orientation and walks in that direction for random amount of time
                int o = UnityEngine.Random.Range(0, 2);
                Orientation = o == 0 ? 1 : -1;
                float walkSeconds = UnityEngine.Random.Range(_lowerWalkingTime, _upperWalkingTime);
                float elapsedTime = 0;
                IsMoving = true;
                
                while (elapsedTime < walkSeconds)
                {
                    CheckForPit();

                    _rb2d.linearVelocityX = Accelerate(_rb2d.linearVelocityX, Orientation, _speed);
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
            }
            //Turns into wall
            else if (choice == 1)
            {
                float wallTime = UnityEngine.Random.Range(_lowerWallTime, _upperWallTime);
                IsWall = true;

                yield return new WaitForSeconds(wallTime);

                //Turns back
                IsWall = false;
            }

            yield return null;
        }
    }

    //Movement methods
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

    private void CheckForPit()
    {
        float pitCheckY = _boxCollider2d.bounds.center.y - (_boxCollider2d.size.y / 2) * _boxCollider2d.transform.localScale.y;
        float pitCheckX = _boxCollider2d.bounds.center.x + (_boxCollider2d.size.x / 2) * Orientation;
        Vector3 _pitCheckPos = new Vector3(pitCheckX, pitCheckY, 0);

        bool hit = Physics2D.Raycast(_pitCheckPos, Vector3.down, 0.3f, _pitCheckLayerMask);
        Debug.DrawRay(_pitCheckPos, Vector3.down * 0.3f, Color.red, 0f, false);

        if (!hit)
            Orientation = -Orientation;
    }
}
