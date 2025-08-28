using UnityEngine;

public class WallerAnimator : MonoBehaviour
{
    const string IS_MOVING = "isMoving";
    const string IS_WALL = "isWall";
    const string IS_HIT = "isHit";

    Animator _animator;
    WallerEnemy _wallerEnemyController;
    SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _wallerEnemyController = GetComponent<WallerEnemy>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (_wallerEnemyController.Orientation < 0)
            _spriteRenderer.flipX = true;
        else if (_wallerEnemyController.Orientation > 0)
            _spriteRenderer.flipX = false;

        if (_wallerEnemyController.CurrState == Enemy.EnemyState.Neutral)
        {
            _animator.SetBool(IS_MOVING, _wallerEnemyController.IsMoving);
            _animator.SetBool(IS_WALL, _wallerEnemyController.IsWall);
        }
        else if (_wallerEnemyController.CurrState == Enemy.EnemyState.Dead)
        {
            _animator.SetBool(IS_HIT, true);
        }
    }
}
