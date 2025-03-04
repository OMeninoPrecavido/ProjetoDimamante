using UnityEngine;

public class MoveTowardsTest : MonoBehaviour
{
    public Transform startPos;
    public Transform endPos;

    public float speed;

    private void Update()
    {
        if (transform.position.x != endPos.position.x)
        {
            float newPosX = Mathf.MoveTowards(transform.position.x, endPos.position.x, speed * Time.deltaTime);
            transform.position = new Vector3(newPosX, transform.position.y, transform.position.z);
        }
        else
        {
            Transform aux = startPos;
            startPos = endPos;
            endPos = aux;
        }
    }
}
