using System.Collections;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IDashable
{
    //References
    protected BoxCollider2D _boxCollider2d;
    protected Rigidbody2D _rb2d;
    protected EnemyManager _enemyManager;
    protected Animator _animator;

    //Attributes
    [Header("-OnDash Attributes-")]
    [SerializeField] float _hitStrenght = 10f;
    [SerializeField] float _deathDelay = 0.5f;

    [Header("-Sound Attributes-")]
    [SerializeField] float _soundRange = 20f;

    //Orientation values
    public int Orientation { get; protected set; } = -1;
    protected int _previousOrientation = -1;

    protected Coroutine _currBehaviour; //References current behaviour enemy has, as a coroutine

    //Possible enemy states
    public enum EnemyState { Neutral, Hostile, Dead }
    public EnemyState CurrState; //Current state

    protected virtual void Start()
    {
        //Component references
        _rb2d = GetComponent<Rigidbody2D>();
        _boxCollider2d = GetComponentInChildren<BoxCollider2D>();
        _animator = GetComponentInChildren<Animator>();

        //Events
        _enemyManager = EnemyManager.Instance;
        _enemyManager.EnableEnemyMovementEvent += OnEnableEnemyMovement;
    }

    //IDashable - Called when dashed through
    public virtual void OnDashedThrough(DashController _dashControllerRef)
    {
        StartCoroutine(Die());
    }

    //Freezes enemy movement as well as their animation
    private void OnEnableEnemyMovement(bool b)
    {
        if (_rb2d != null)
            _rb2d.constraints = b? RigidbodyConstraints2D.FreezeRotation : RigidbodyConstraints2D.FreezeAll;

        if (_animator != null)
            _animator.speed = b ? 1f : 0f;
    }

    //Called when enemy is dashed through
    IEnumerator Die()
    {
        CurrState = EnemyState.Dead;
        StopCoroutine(_currBehaviour);
        _boxCollider2d.enabled = false;

        _rb2d.linearVelocity = new Vector3(Orientation * 1, 2, 0).normalized * _hitStrenght;
        yield return new WaitForSeconds(_deathDelay);
        Destroy(gameObject);
    }

    protected bool CheckIfWithinSoundRange(float range)
    {
        Vector3 camPos = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0);
        Vector3 enemyPos = new Vector3(transform.position.x, transform.position.y, 0);

        if (Vector3.Distance(camPos, enemyPos) < range)
            return true;

        return false;
    }

    protected IEnumerator StepSounds(string soundName, float interval)
    {
        while (true)
        {
            if (CheckIfWithinSoundRange(15f))
                AudioManager.Instance.Play(soundName);
            yield return new WaitForSeconds(interval);
        }
    }
}
