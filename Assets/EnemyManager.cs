using System;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public event Action<bool> EnableEnemyMovementEvent;
    public void EnableEnemyMovement(bool b) => EnableEnemyMovementEvent?.Invoke(b);
}
