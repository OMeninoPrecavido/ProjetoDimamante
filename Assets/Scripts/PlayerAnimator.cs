using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] Animator animator;
    PlayerMovement playerMovement;
    DashController dashController;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        dashController = GetComponent<DashController>();
    }
}
