using TMPro;
using UnityEngine;

public class MenuTextOption : MenuOptionBase
{
    [SerializeField] TextMeshProUGUI _text;
    [SerializeField] Color _selectedColor;
    [SerializeField] Color _regularColor;

    public override void OnChoose()
    {
        _text.color = _selectedColor;
    }

    public override void OnUnchoose()
    {
        _text.color= _regularColor;
    }
}
