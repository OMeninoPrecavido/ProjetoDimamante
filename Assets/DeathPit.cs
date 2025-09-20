using UnityEngine;

public class DeathPit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerDamage playerDamage = collision.GetComponentInParent<PlayerDamage>();
        if (playerDamage != null){
            StartCoroutine(playerDamage.OnPitFall());
        }
    }
}
