using UnityEngine;

public class SmoothDampingTest : MonoBehaviour
{
    [SerializeField] Transform startingPos;
    [SerializeField] Transform endingPos;

    [SerializeField] float smoothTime;

    [SerializeField] Vector3 currentVelocity = Vector3.zero;

    private void Update()
    {
        float st = smoothTime;
        /*if (endingPos.position.x > 9)
        {
            st = smoothTime / 3;
        }*/
        transform.position = Vector3.SmoothDamp(transform.position, endingPos.position, ref currentVelocity, st);
        if (transform.position == endingPos.position)
        {
            Transform aux = startingPos;
            startingPos = endingPos;
            endingPos = aux;
        }
    }
}
