using UnityEngine;

//Component must be in the same GameObject as the collider

public abstract class Collectable : MonoBehaviour, IDashable
{
    BoxCollider2D _bc2d;
    protected DashController _dashControllerRef;

    protected virtual void Start()
    {
        _bc2d = GetComponent<BoxCollider2D>();
        if (_bc2d == null || _bc2d.isTrigger == false)
            Debug.LogError("Every collectable must have a trigger collider");
    }

    protected virtual void Collect() { }

    public void OnDashedThrough(DashController dashControllerRef)
    {
        _dashControllerRef = dashControllerRef;
        Collect();
        Destroy(gameObject);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if ((_dashControllerRef = collision.GetComponentInParent<DashController>()) != null)
        {
            Collect();
            Destroy(gameObject);
        }
    }
}
