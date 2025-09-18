using Unity.VisualScripting;
using UnityEngine;

public class DashPowerUp : Collectable
{
    [SerializeField] float _dashIncrement;

    protected override void Collect()
    {
        base.Collect();
        _dashControllerRef.IncrementDashDistance(_dashIncrement);
    }
}
