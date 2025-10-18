using UnityEngine;
using UnityEngine.UI;

public class GalleryOption : MenuOptionBase
{
    [SerializeField] Color _onChooseColor;
    [SerializeField] Color _regularColor;
    [SerializeField] Image _outerBorder;
    [SerializeField] Image _drawingImage;
    [SerializeField] DrawingData _drawingData;
    [SerializeField] GameObject _infoElement;
    public DrawingData drawingData { get { return _drawingData; } }

    public void Initialize(DrawingData newDrawingData)
    {
        _drawingData = newDrawingData;
        _drawingImage.sprite = _drawingData.Drawing;
    }

    public void SetAsInfo()
    {
        _infoElement.SetActive(true);
    }

    public override void OnChoose()
    {
        _outerBorder.color = _onChooseColor;
    }

    public override void OnUnchoose()
    {
        _outerBorder.color = _regularColor;
    }
}
