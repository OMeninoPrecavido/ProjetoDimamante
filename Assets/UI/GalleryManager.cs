using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GalleryManager : MonoBehaviour
{
    public bool GalleryIsOpen = false;

    public enum GalleryMode { Base, Drawing }
    private GalleryMode _currGalleryMode = GalleryMode.Base;

    [SerializeField] GridLayoutGroup _galleryGLG;
    [SerializeField] MenuOptionBase _goBackOption;

    [SerializeField] GameObject _galleryBase;

    [SerializeField] GameObject _drawingBase;
    [SerializeField] TextMeshProUGUI _drawingBaseTitle;
    [SerializeField] TextMeshProUGUI _drawingBaseDescription;
    [SerializeField] Image _drawingBaseImage;

    private MenuOptionBase[,] _galleryOptions;
    (MenuOptionBase opt, int column, int row) _currOption;

    private void Start()
    {
        //Populates _galleryOptions matrix
        int columnNumber = _galleryGLG.constraintCount;
        int rowNumber = _galleryGLG.transform.childCount / columnNumber;

        _galleryOptions = new MenuOptionBase[rowNumber + 1, columnNumber]; //+1 for "go back" button

        int i = 0;
        int j = 0;
        foreach (Transform container in _galleryGLG.transform)
        {
            _galleryOptions[i, j] = container.GetComponentInChildren<MenuOptionBase>();

            if (_galleryOptions[i, j] is GalleryOption galOpt)
            {
                if (galOpt.drawingData != null)
                    galOpt.SetSelectedEvent(delegate { OpenDrawingBase(galOpt.drawingData); });
            }

            j++;
            if (j >= columnNumber)
            {
                i++;
                j = 0;
            }
        }

        _galleryOptions[rowNumber, 0] = _goBackOption;

        //Sets starting chosen option
        _currOption.column = 0;
        _currOption.row = 0;
        _currOption.opt = _galleryOptions[0, 0];

        _currOption.opt.OnChoose();
    }

    private void Update()
    {
        if (!GalleryIsOpen)
            return;

        if (_currGalleryMode == GalleryMode.Base)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
                ChangeSelectedOption(_currOption.column + 1, _currOption.row);

            if (Input.GetKeyDown(KeyCode.LeftArrow))
                ChangeSelectedOption(_currOption.column - 1, _currOption.row);

            if (Input.GetKeyDown(KeyCode.DownArrow))
                ChangeSelectedOption(_currOption.column, _currOption.row + 1);

            if (Input.GetKeyDown(KeyCode.UpArrow))
                ChangeSelectedOption(_currOption.column, _currOption.row - 1);

            if (Input.GetKeyDown(KeyCode.Z))
                _currOption.opt.Select();
        }
        else if (_currGalleryMode == GalleryMode.Drawing)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                int row = _currOption.row;
                int column = _currOption.column + 1;

                if (column > _galleryOptions.GetLength(1) - 1)
                {
                    column = 0;
                    row++;
                }

                if (row > _galleryOptions.GetLength(0) - 2)
                    return;

                ChangeDrawingPage(column, row);
            }

            //if (Input.GetKeyDown(KeyCode.LeftArrow))
                // ChangeSelectedOption(_currOption.column - 1, _currOption.row);
        }

    }

    private void ChangeSelectedOption(int newColumn, int newRow)
    {
        if (newRow >= _galleryOptions.GetLength(0) || newRow < 0)
            return;

        if (newRow == _galleryOptions.GetLength(0) - 1)
            newColumn = 0;

        if (_currOption.row == _galleryOptions.GetLength(0) - 1
            && newRow < _currOption.row)
            newColumn = Mathf.CeilToInt((_galleryOptions.GetLength(1) - 1) / 2);

        if (newColumn >= _galleryOptions.GetLength(1) || newColumn < 0)
            return;

        _currOption.opt.OnUnchoose();

        _currOption.column = newColumn;
        _currOption.row = newRow;
        _currOption.opt = _galleryOptions[newRow, newColumn];

        _currOption.opt.OnChoose();

    }

    public void OpenDrawingBase(DrawingData drawingData)
    {
        _galleryBase.SetActive(false);
        _drawingBase.SetActive(true);
        _drawingBaseTitle.text = drawingData.Title;
        _drawingBaseDescription.text = drawingData.Description;
        _drawingBaseImage.sprite = drawingData.Drawing;
        _currGalleryMode = GalleryMode.Drawing;
    }

    public void UpdateDrawingBase(DrawingData drawingData)
    {
        _drawingBaseTitle.text = drawingData.Title;
        _drawingBaseDescription.text = drawingData.Description;
        _drawingBaseImage.sprite = drawingData.Drawing;
    }

    private void ChangeDrawingPage(int newColumn, int newRow)
    {
        _currOption.column = newColumn;
        _currOption.row = newRow;
        _currOption.opt = _galleryOptions[newRow, newColumn];

        if (_currOption.opt is GalleryOption galOpt)
        {
            UpdateDrawingBase(galOpt.drawingData);
        }
    }
}
