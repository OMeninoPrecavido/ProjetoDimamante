using UnityEngine;

public class FollowAuxSD : MonoBehaviour
{
    public Transform player;
    public CameraMovement scenecamera;
    float velocity = 0;

    private void Update()
    {
        float newX = Mathf.SmoothDamp(transform.position.x, player.position.x, ref velocity, scenecamera._smoothTimeX);
        transform.position = new Vector3 (newX, transform.position.y, transform.position.z);
    }
}
