using System;
using UnityEngine;

public class PurpleDiamond : Collectable
{
    public event Action OnCollected;

    protected override void CollectEffect()
    {
        OnCollected?.Invoke();
    }
}
