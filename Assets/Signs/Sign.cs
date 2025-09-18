using UnityEngine;

public class Sign : MonoBehaviour
{
    [TextArea]
    [SerializeField] string _text;

    [SerializeField] Transform _textPos;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponentInParent<PlayerMovement>() != null)
            SignUIManager.Instance.ActivateSignView(_text, _textPos.position);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponentInParent<PlayerMovement>() != null)
            SignUIManager.Instance.DeactivateSignView();
    }
}
