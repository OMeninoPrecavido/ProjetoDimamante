using UnityEngine;

public class TestingBoost : MonoBehaviour
{
    Rigidbody2D rb2d;
    [SerializeField] float power;

    private void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 v3 = new Vector3(1, 1, 0).normalized;
            rb2d.linearVelocity = v3 * power;
        }
    }
}
