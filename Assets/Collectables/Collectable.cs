using System.Collections;
using UnityEngine;

//Component must be in the same GameObject as the collider

public abstract class Collectable : MonoBehaviour, IDashable
{
    [SerializeField] AnimationClip _collectClip;
    Animator _animator;
    BoxCollider2D _bc2d;
    protected DashController _dashControllerRef;

    protected virtual void Start()
    {
        _animator = GetComponent<Animator>();
        _bc2d = GetComponent<BoxCollider2D>();
        if (_bc2d == null || _bc2d.isTrigger == false)
            Debug.LogError("Every collectable must have a trigger collider");
    }

    protected virtual void CollectEffect() { }

    protected IEnumerator Collect()
    {
        CollectEffect();
        _animator.SetTrigger("Destroy");
        yield return new WaitForSeconds(_collectClip.length);
        Destroy(gameObject);
    }

    public void OnDashedThrough(DashController dashControllerRef)
    {
        _dashControllerRef = dashControllerRef;
        StartCoroutine(Collect());
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if ((_dashControllerRef = collision.GetComponentInParent<DashController>()) != null)
        {
            StartCoroutine(Collect());
        }
    }
}
