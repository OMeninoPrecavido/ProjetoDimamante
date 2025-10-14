using UnityEngine;
using UnityEngine.UI;

public class GalleryOption : MenuOptionBase
{
    [SerializeField] Color _onChooseColor;
    [SerializeField] Color _regularColor;
    [SerializeField] Image _outerBorder;
    [SerializeField] DrawingData _drawingData;
    public DrawingData drawingData { get { return _drawingData; } }

    public override void OnChoose()
    {
        _outerBorder.color = _onChooseColor;
    }

    public override void OnUnchoose()
    {
        _outerBorder.color = _regularColor;
    }
}
