using UnityEngine;

public class FlyerAnimator : MonoBehaviour
{
    const string IS_NEUTRAL = "isNeutral";
    const string IS_DESCENDING = "isDescending";
    const string IS_SWOOPING = "isSwooping";
    const string IS_DEAD = "isDead";

    Animator _animator;
    FlyerEnemy _flyerEnemyController;
    SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _flyerEnemyController = GetComponent<FlyerEnemy>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (_flyerEnemyController.Orientation < 0)
            _spriteRenderer.flipX = false;
        else if (_flyerEnemyController.Orientation > 0)
            _spriteRenderer.flipX = true;

        if (_flyerEnemyController.CurrState == Enemy.EnemyState.Dead)
        {
            FlyerSetBool(IS_DEAD);
        }
        else
        {
            switch (_flyerEnemyController.CurrFlyerState)
            {
                case FlyerEnemy.FlyerState.Patrolling:
                case FlyerEnemy.FlyerState.Rising:
                    FlyerSetBool(IS_NEUTRAL);
                    break;
                case FlyerEnemy.FlyerState.Descending:
                    FlyerSetBool(IS_DESCENDING);
                    break;
                case FlyerEnemy.FlyerState.Swooping:
                    FlyerSetBool(IS_SWOOPING);
                    break;
            }
        }
    }

    private void FlyerSetBool(string booleanName)
    {
        bool isNeutral = false;
        bool isDead = false;
        bool isSwooping = false;
        bool isDescending = false;

        if (booleanName == IS_NEUTRAL)
            isNeutral = true;
        if (booleanName == IS_DEAD)
            isDead = true;
        if (booleanName == IS_SWOOPING)
            isSwooping = true;
        if (booleanName == IS_DESCENDING)
            isDescending = true;

        _animator.SetBool(IS_NEUTRAL, isNeutral);
        _animator.SetBool(IS_DEAD, isDead);
        _animator.SetBool(IS_SWOOPING, isSwooping);
        _animator.SetBool(IS_DESCENDING, isDescending);
    }
}
