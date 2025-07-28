using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour, IDashable
{
    //References
    protected Rigidbody2D _rb2d;

    //Attributes
    [SerializeField] float _hitStrenght = 10f;
    [SerializeField] float _deathDelay = 0.5f;
    protected int _orientation = -1;

    private void Start()
    {
        _rb2d = GetComponent<Rigidbody2D>();
    }

    public void OnDashedThrough()
    {
        GetComponent<SpriteRenderer>().color = Color.red;
        StartCoroutine(Die());
    }

    IEnumerator Die()
    {
        _rb2d.linearVelocity = new Vector3(_orientation * 1, 2, 0).normalized * _hitStrenght;
        yield return new WaitForSeconds(_deathDelay);
        Destroy(gameObject);
    }
}
