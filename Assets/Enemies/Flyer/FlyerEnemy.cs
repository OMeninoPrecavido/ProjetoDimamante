using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class FlyerEnemy : Enemy
{
    [Header("-Neutral State-")]
    [SerializeField] Transform _pointLeft;
    [SerializeField] Transform _pointRight;
    [SerializeField] float _flyingSpeed;
    [SerializeField] float _fovAngle;
    [SerializeField] LayerMask _checkForPlayerLayerMask;
    [SerializeField] float _neutralCooldown;

    [Header("-Hostile State-")]
    [SerializeField] float _turnTime;
    [SerializeField] float _pauseTime;
    [SerializeField] Transform _floorCheck;
    [SerializeField] float _floorCheckDistance;
    [SerializeField] float _wallCheckDistance;
    [SerializeField] LayerMask _floorCheckLayerMask;
    [SerializeField] float _descentSpeed;
    [SerializeField] float _swoopingSpeed;
    [SerializeField] float _risingSpeed;
    [SerializeField] AnimationClip _downImpulseClip;
    [SerializeField] AnimationClip _forwardImpulseClip;

    public FlyerState CurrFlyerState { get; private set; } = FlyerState.Patrolling;
    public Coroutine _flapSoundRef;

    public enum FlyerState
    {
        Patrolling,
        Descending,
        Swooping,
        Rising
    }

    protected override void Start()
    {
        base.Start(); //Base class Start()

        //Starts on neutral behaviour
        CurrState = EnemyState.Neutral;
        _currBehaviour = StartCoroutine(NeutralBehaviour());
    }

    //Neutral behaviour loop
    private IEnumerator NeutralBehaviour()
    {        
        if (_flapSoundRef == null)
            _flapSoundRef = StartCoroutine(StepSounds("Flap", 0.65f));

        SetFlyerState(FlyerState.Patrolling);
        float cooldownTimer = 0;

        while (CurrState == EnemyState.Neutral)
        {
            //Cooldown timer
            if (cooldownTimer <= _neutralCooldown)
                cooldownTimer += Time.deltaTime;

            Vector2 playerPosition;
            //If player is seen and is not on cooldown
            if (CheckForPlayer(out playerPosition) && cooldownTimer > _neutralCooldown)
            {
                //Starts hostile behaviour
                _rb2d.linearVelocityX = 0;
                CurrState = EnemyState.Hostile;
                StopCoroutine(_currBehaviour);
                _currBehaviour = StartCoroutine(HostileBehaviour(playerPosition));
                break;
            }

            //Changes direction if reached one of the reference points
            if (transform.position.x >= _pointRight.position.x)
                Orientation = -1;
            if (transform.position.x <= _pointLeft.position.x)
                Orientation = 1;

            //Moves
            float velX = Orientation * _flyingSpeed;
            _rb2d.linearVelocityX = velX;
            yield return null;
        }
    }

    //Hostile behaviour loop
    private IEnumerator HostileBehaviour(Vector2 playerPosition)
    {
        if (_flapSoundRef != null)
            StopCoroutine(_flapSoundRef);

        //Diving movement
        SetFlyerState(FlyerState.Descending);
        yield return new WaitForSeconds(_downImpulseClip.length);

        AudioManager.Instance.Play("FlyerDescent");
        AudioManager.Instance.Play("FlyerScreech");

        bool hitFloor = false;
        while (transform.position.y > playerPosition.y + _floorCheckDistance)
        {
            Debug.DrawLine(_floorCheck.position, new Vector2(_floorCheck.position.x, _floorCheck.position.y - _floorCheckDistance), Color.red);
            if (Physics2D.Raycast(_floorCheck.position, Vector2.down, _floorCheckDistance, _floorCheckLayerMask))
            {
                hitFloor = true;
                break;
            }

            _rb2d.linearVelocityY = -_descentSpeed;
            yield return null;
        }

        if (!hitFloor)
        {
            //Curve - Not used anymore
            /*float elapsedTime = 0;
            float ratio = 0;
            while (elapsedTime < _turnTime)
            {
                ratio = elapsedTime / _turnTime;
                _rb2d.linearVelocity = Vector2.Lerp(Vector2.down, new Vector2(Orientation, 0), ratio).normalized * _descentSpeed;

                elapsedTime += Time.deltaTime;
                yield return null;
            }*/

            _rb2d.linearVelocityY = 0;

            //Swooping movement
            SetFlyerState(FlyerState.Swooping);
            yield return new WaitForSeconds(_forwardImpulseClip.length);

            AudioManager.Instance.Play("FlyerImpulse");

            while ((Orientation == 1 && transform.position.x <= playerPosition.x) ||
                   (Orientation == -1 && transform.position.x >= playerPosition.x))
            {
                float halfWidth = _boxCollider2d.bounds.size.x / 2;
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x + Orientation * halfWidth, transform.position.y), Vector3.right * Orientation, _wallCheckDistance, _floorCheckLayerMask);
                Debug.DrawRay(new Vector2(transform.position.x + Orientation * halfWidth, transform.position.y), Vector3.right * Orientation, Color.red, _wallCheckDistance);

                if (hit)
                {
                    Debug.Log(hit.collider.gameObject.name);
                    break;
                }

                _rb2d.linearVelocityX = Orientation * _swoopingSpeed;
                yield return null;
            }
        }

        //Rising movement        
        SetFlyerState(FlyerState.Rising);

        _flapSoundRef = StartCoroutine(StepSounds("Flap", 0.65f));

        while (transform.position.y < _pointLeft.position.y ||
               transform.position.x < _pointLeft.position.x ||
               transform.position.x > _pointRight.position.x)
        {
            Vector2 vel = new Vector2();
            vel.x = Orientation;

            if (transform.position.y < _pointLeft.position.y)
                vel.y = 1;

            if (transform.position.x < _pointLeft.position.x)
                vel.x = Orientation = 1;
            else if (transform.position.x > _pointRight.position.x)
                vel.x = Orientation = -1;

            _rb2d.linearVelocity = vel.normalized * _risingSpeed;

            yield return null;
        }

        _rb2d.linearVelocityY = 0;
        _rb2d.linearVelocityX = 0;

        CurrState = EnemyState.Neutral;
        StopCoroutine(_currBehaviour);
        _currBehaviour = StartCoroutine(NeutralBehaviour());

    }

    //Checks if player is within line of sight
    private bool CheckForPlayer(out Vector2 playerPosition)
    {
        BoxCollider2D playerCollider2D = Physics2D.OverlapCircle(transform.position, 10f, _checkForPlayerLayerMask) as BoxCollider2D;
        DebugDrawCircle(20, transform.position, 10f, _fovAngle, Orientation);

        if (playerCollider2D != null)
        {
            Vector2 halfSize = playerCollider2D.size * 0.5f;
            Vector2[] vertices = new Vector2[4];
            vertices[0] = (Vector2)playerCollider2D.transform.position + new Vector2(-halfSize.x, halfSize.y); //Top left
            vertices[1] = (Vector2)playerCollider2D.transform.position + new Vector2(-halfSize.x, -halfSize.y); //Bottom left
            vertices[2] = (Vector2)playerCollider2D.transform.position + new Vector2(halfSize.x, halfSize.y); //Top right
            vertices[3] = (Vector2)playerCollider2D.transform.position + new Vector2(halfSize.x, -halfSize.y); //Bottom right

            Debug.DrawLine(vertices[0], vertices[1], Color.red);
            Debug.DrawLine(vertices[1], vertices[3], Color.red);
            Debug.DrawLine(vertices[3], vertices[2], Color.red);
            Debug.DrawLine(vertices[2], vertices[0], Color.red);
            Debug.DrawLine(vertices[0], vertices[3], Color.red);
            Debug.DrawLine(vertices[2], vertices[1], Color.red);

            foreach (Vector2 vertex in vertices)
            {
                float vertexAngle = -Orientation * Vector2.SignedAngle((vertex - (Vector2)transform.position).normalized, Vector2.down);
                if (vertexAngle <= _fovAngle && vertexAngle >= 0)
                {
                    playerPosition = playerCollider2D.transform.position;
                    RaycastHit2D hit = Physics2D.Linecast(transform.position, vertex, _floorCheckLayerMask);

                    if (hit.collider == null)
                        return true;
                }
            }
        }

        playerPosition = Vector2.zero;
        return false;
    }

    private void DebugDrawCircle(int segmentNumber, Vector2 center, float radius, float fovAngle, float orientation)
    {
        float segmentAngle = (2 * Mathf.PI) / segmentNumber;

        for (int i = 0; i < segmentNumber; i++)
        {
            Vector2 pointA;
            pointA.x = radius * Mathf.Cos(segmentAngle * i);
            pointA.y = radius * Mathf.Sin(segmentAngle * i);
            pointA += center;

            Vector2 pointB;
            pointB.x = radius * Mathf.Cos(segmentAngle * (i + 1));
            pointB.y = radius * Mathf.Sin(segmentAngle * (i + 1));
            pointB += center;

            Debug.DrawLine(pointA, pointB, Color.red);
        }

        Vector2 pointC;
        pointC.x = orientation * radius * Mathf.Cos(-(Mathf.PI / 2) + (Mathf.Deg2Rad * fovAngle));
        pointC.y = radius * Mathf.Sin(-(Mathf.PI / 2) + (Mathf.Deg2Rad * fovAngle));
        pointC += center;

        Vector2 pointD = center;
        pointD.y -= radius;
        Debug.DrawLine(transform.position, pointC, Color.blue);
        Debug.DrawLine(transform.position, pointD, Color.blue);
    }

    private void SetFlyerState(FlyerState flyerState)
    {
        CurrFlyerState = flyerState;
        
        switch (flyerState)
        {
            case FlyerState.Patrolling:
            case FlyerState.Descending:
            case FlyerState.Swooping:
                _boxCollider2d.enabled = true;
                break;
            case FlyerState.Rising:
                _boxCollider2d.enabled = false;
                break;
        }
    }

    public override void OnDashedThrough(DashController dashControllerRef)
    {
        _rb2d.linearVelocity = Vector2.zero;
        _rb2d.bodyType = RigidbodyType2D.Dynamic;
        base.OnDashedThrough(dashControllerRef);
    }
}
