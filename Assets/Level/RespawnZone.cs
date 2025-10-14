using UnityEngine;

public class RespawnZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerMovement playerMovement = collision.GetComponentInParent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.SetRespawnZone(this);
        }
    }
}
