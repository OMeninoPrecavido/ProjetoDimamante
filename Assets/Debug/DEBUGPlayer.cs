using TMPro;
using UnityEngine;

public class DEBUGPlayerMovement : MonoBehaviour
{
    [Header("-Player Reference-")]
    [SerializeField] GameObject player;

    PlayerMovement playerMovement;
    Rigidbody2D rb2d;

    [Header("-Canvas References-")]
    [SerializeField] TextMeshProUGUI isGrounded;
    [SerializeField] TextMeshProUGUI hasJumped;
    [SerializeField] TextMeshProUGUI isFalling;
    [SerializeField] TextMeshProUGUI isMovementEnabled;
    [SerializeField] TextMeshProUGUI hOrientation;
    [SerializeField] TextMeshProUGUI playerOrientation;
    [SerializeField] TextMeshProUGUI xVelocity;
    [SerializeField] TextMeshProUGUI yVelocity;

    private void Start()
    {
        playerMovement = player.GetComponent<PlayerMovement>();
        rb2d = player.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        SetStatText(ref isGrounded, playerMovement.IsGrounded);
        SetStatText(ref hasJumped, playerMovement.HasJumped);
        SetStatText(ref isFalling, playerMovement.IsFalling);
        SetStatText(ref isMovementEnabled, playerMovement.IsMovementEnabled);
        SetStatText(ref hOrientation, playerMovement.HOrientation);
        SetStatText(ref playerOrientation, playerMovement.PlayerOrientation);
        SetStatText(ref xVelocity, rb2d.linearVelocityX);
        SetStatText(ref yVelocity, rb2d.linearVelocityY);
    }

    void SetStatText(ref TextMeshProUGUI statText, bool stat)
    {
        if (stat == true)
        {
            statText.color = Color.green;
            statText.text = "TRUE";
        }
        else
        {
            statText.color = Color.red;
            statText.text = "FALSE";
        }
    }

    void SetStatText(ref TextMeshProUGUI statText, float stat)
    {
        statText.text = stat.ToString("F2");
    }
}
