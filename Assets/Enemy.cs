using System.Collections;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IDashable
{
    //References
    protected BoxCollider2D _boxCollider2d;
    protected Rigidbody2D _rb2d;
    protected EnemyManager _enemyManager;

    //Attributes
    [Header("-OnDash Attributes-")]
    [SerializeField] float _hitStrenght = 10f;
    [SerializeField] float _deathDelay = 0.5f;

    protected int _orientation = -1;
    protected int _previousOrientation = -1;

    protected Coroutine _currBehaviour;

    public enum EnemyState { Neutral, Hostile, Dead }
    protected EnemyState _currState;

    protected virtual void Start()
    {
        _rb2d = GetComponent<Rigidbody2D>();
        _boxCollider2d = GetComponent<BoxCollider2D>();

        _enemyManager = EnemyManager.Instance;
        _enemyManager.EnableEnemyMovementEvent += OnEnableEnemyMovement;
    }

    //IDashable - Called when dashed through
    public void OnDashedThrough()
    {
        GetComponent<SpriteRenderer>().color = Color.red;
        StartCoroutine(Die());
    }

    private void OnEnableEnemyMovement(bool b)
    {
        if (_rb2d != null)
            _rb2d.constraints = b? RigidbodyConstraints2D.FreezeRotation : RigidbodyConstraints2D.FreezeAll;
    }

    IEnumerator Die()
    {
        _currState = EnemyState.Dead;
        StopCoroutine(_currBehaviour);

        _rb2d.linearVelocity = new Vector3(_orientation * 1, 2, 0).normalized * _hitStrenght;
        yield return new WaitForSeconds(_deathDelay);
        Destroy(gameObject);
    }
}
