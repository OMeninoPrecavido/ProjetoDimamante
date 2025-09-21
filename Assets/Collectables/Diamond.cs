using UnityEngine;

public class Diamond : Collectable
{
    protected override void CollectEffect()
    {
        PlayerDamage playerDamageRef = _dashControllerRef.GetComponent<PlayerDamage>();
        if (playerDamageRef != null)
        {
            playerDamageRef.AddDiamonds(1);
        }
    }

}
