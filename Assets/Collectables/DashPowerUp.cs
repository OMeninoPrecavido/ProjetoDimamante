using Unity.VisualScripting;
using UnityEngine;

public class DashPowerUp : Collectable
{
    [SerializeField] float _dashIncrement;

    protected override void CollectEffect()
    {
        base.CollectEffect();
        _dashControllerRef.IncrementDashDistance(_dashIncrement);
    }
}
