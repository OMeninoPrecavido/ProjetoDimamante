using UnityEngine;

public class FollowAuxSD : MonoBehaviour
{
    public Transform player;
    public float smoothTime; 
    float velocity = 0;

    private void Update()
    {
        float newX = Mathf.SmoothDamp(transform.position.x, player.position.x, ref velocity, smoothTime);
        transform.position = new Vector3 (newX, transform.position.y, transform.position.z);
    }
}
