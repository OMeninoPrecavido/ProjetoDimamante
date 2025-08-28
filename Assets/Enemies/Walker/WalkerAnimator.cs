using UnityEngine;

public class WalkerAnimator : MonoBehaviour
{
    const string IS_WALKING = "IsWalking";
    const string IS_RUNNING = "IsRunning";
    const string IS_HIT = "IsHit";

    Animator _animator;
    WalkerEnemy _walkerEnemyController;
    Rigidbody2D _rb2d;
    SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _walkerEnemyController = GetComponent<WalkerEnemy>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _rb2d = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (_walkerEnemyController.Orientation < 0)
            _spriteRenderer.flipX = true;
        else if (_walkerEnemyController.Orientation > 0)
            _spriteRenderer.flipX = false;


        if (!_walkerEnemyController.IsMoving)
        {
            _animator.SetBool(IS_WALKING, false);
            _animator.SetBool(IS_RUNNING, false);
        }
        else
        {
            if (_walkerEnemyController.CurrState == Enemy.EnemyState.Neutral)
            {
                _animator.SetBool(IS_WALKING, true);
                _animator.SetBool(IS_RUNNING, false);
            }
            else if (_walkerEnemyController.CurrState == Enemy.EnemyState.Hostile)
            {
                _animator.SetBool(IS_WALKING, false);
                _animator.SetBool(IS_RUNNING, true);
            }
        }

        if (_walkerEnemyController.CurrState == Enemy.EnemyState.Dead)
            _animator.SetBool(IS_HIT, true);

    }
}
