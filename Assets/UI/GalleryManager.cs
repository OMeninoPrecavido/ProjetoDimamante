using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static OldGalleryManager;

public class GalleryManager : MonoBehaviour
{
    [Header("-Base Gallery Menu-")]
    [SerializeField] GameObject _baseGallery;
    [SerializeField] GameObject _baseGalleryTitle;
    [SerializeField] GameObject _baseGalleryGLG;
    [SerializeField] GameObject _baseGalleryGoBackButton;

    [Header("-Drawing Menu-")]
    [SerializeField] GameObject _drawingMenu;
    [SerializeField] TextMeshProUGUI _drawingTitle;
    [SerializeField] TextMeshProUGUI _drawingText;
    [SerializeField] Image _drawingImage;
    [SerializeField] TextMeshProUGUI _leftArrow;
    [SerializeField] TextMeshProUGUI _rightArrow;

    [Header("-Info Menu-")]
    [SerializeField] GameObject _infoMenu;
    [SerializeField] TextMeshProUGUI _rightInfoArrow;
    [SerializeField] TextMeshProUGUI _leftInfoArrow;

    [Header("-Drawings List")]
    [SerializeField] List<DrawingData> _drawings;

    [Header("-Prefabs-")]
    [SerializeField] GameObject _galleryOptionPrefab;

    [Header("-Selection Colors-")]
    [SerializeField] Color _selectedColor;
    [SerializeField] Color _normalColor;
    [SerializeField] Color _lockedColor;

    //Admin. de Opções
    private MenuOptionBase[,] _galleryOptions;
    (MenuOptionBase opt, int column, int row) _currOption;

    //Estado atual do menu de galeria
    public enum GalleryMode { Base, Drawing, Transition, Closed }
    private GalleryMode _currGalleryMode = GalleryMode.Closed;

    //Variáveis aux. da transição
    private Vector3 _startingGalleryMenuPosition;

    //Referências de Input
    InputAction _upPressAction;
    InputAction _downPressAction;
    InputAction _leftPressAction;
    InputAction _rightPressAction;
    InputAction _leftReleaseAction;
    InputAction _rightReleaseAction;
    InputAction _confirmAction;

    private void Start()
    {
        //Setup dos inputs
        _upPressAction = InputSystem.actions.FindAction("UI/UpPress");
        _downPressAction = InputSystem.actions.FindAction("UI/DownPress");
        _leftPressAction = InputSystem.actions.FindAction("UI/LeftPress");
        _rightPressAction = InputSystem.actions.FindAction("UI/RightPress");
        _leftReleaseAction = InputSystem.actions.FindAction("UI/LeftRelease");
        _rightReleaseAction = InputSystem.actions.FindAction("UI/RightRelease");
        _confirmAction = InputSystem.actions.FindAction("UI/Confirm");

        _upPressAction.performed += UpInput;
        _downPressAction.performed += DownInput;
        _leftPressAction.performed += LeftInput;
        _rightPressAction.performed += RightInput;
        _leftReleaseAction.performed += LeftReleaseInput;
        _rightReleaseAction.performed += RightReleaseInput;
        _confirmAction.performed += ConfirmInput;

        _startingGalleryMenuPosition = transform.position;

        //Inicializa a matriz de opções
        int columnNumber = _baseGalleryGLG.GetComponent<GridLayoutGroup>().constraintCount;
        int rowNumber = Mathf.CeilToInt((_drawings.Count + 1) / (float)columnNumber) + 1; //+1 para a opção de info e +1 para a opção voltar

        _galleryOptions = new MenuOptionBase[rowNumber, columnNumber];

        //Cria o primeiro objeto da galeria, a página de informações
        GameObject infoContainer = new GameObject();
        infoContainer.name = "InfoContainer";
        infoContainer.AddComponent<RectTransform>();
        infoContainer.transform.SetParent(_baseGalleryGLG.transform, false);

        GameObject infoGalOpt = Instantiate(_galleryOptionPrefab, infoContainer.transform);
        GalleryOption infoGalOptComp = infoGalOpt.GetComponent<GalleryOption>();
        
        //Define a função do botão de informação
        infoGalOptComp.SetSelectedEvent(delegate
        {
            _currGalleryMode = GalleryMode.Drawing;
            _infoMenu.SetActive(true);
            _drawingMenu.SetActive(false);
            _baseGallery.SetActive(false);
        });

        //Ativa o visual de informação
        infoGalOptComp.SetAsInfo();

        _galleryOptions[0, 0] = infoGalOptComp; //Primeiro item da matriz de opções

        //Variáveis para popular a matriz de opções
        int r = 0;
        int c = 1;

        //Cria um objeto da galeria para cada desenho
        foreach (DrawingData drawing in _drawings)
        {
            //Cria o gameobject container
            GameObject container = new GameObject();
            container.name = "Container";
            container.AddComponent<RectTransform>();
            container.transform.SetParent(_baseGalleryGLG.transform, false);

            //Cria a gallery option dentro do container
            GameObject galOpt = Instantiate(_galleryOptionPrefab, container.transform);
            GalleryOption galOptComp = galOpt.GetComponent<GalleryOption>();
            galOptComp.Initialize(drawing);
            galOptComp.SetSelectedEvent(delegate { OpenDrawingMenu(galOptComp.drawingData); });

            //Popula a matriz de gallery options
            _galleryOptions[r, c] = galOptComp;

            c++;
            if (c >= columnNumber)
            {
                c = 0;
                r++;
            }
        }

        //Define opção inicial selecionada
        if (_currOption.opt != null)
            _currOption.opt.OnUnchoose();

        _currOption.opt = _galleryOptions[0, 0];
        _currOption.row = 0;
        _currOption.column = 0;

        _currOption.opt.OnChoose();

        //Botão de retorno adicionado à matriz de opções.
        //Sempre na última fileira e primeira coluna.
        _galleryOptions[_galleryOptions.GetLength(0) - 1, 0] = _baseGalleryGoBackButton.GetComponent<MenuTextOption>();
    }

    #region Gallery Methods

    //Inicialização a ser chamada para abrir o menu da galeria
    public IEnumerator Initialize()
    {
        _currGalleryMode = GalleryMode.Closed;

        //Coloca a opção selecionada como a primeira
        if (_currOption.opt != null)
            _currOption.opt.OnUnchoose();

        _currOption.opt = _galleryOptions[0, 0];
        _currOption.row = 0;
        _currOption.column = 0;

        _currOption.opt.OnChoose();

        //Esconde seus componentes
        _baseGalleryTitle.SetActive(false);
        _baseGalleryGLG.SetActive(false);
        _baseGalleryGoBackButton.SetActive(false);

        //Move o menu de galeria para a posição inicial
        transform.position = _startingGalleryMenuPosition;

        //Ativa obj base e título
        yield return new WaitForSeconds(0.3f);
        _baseGallery.SetActive(true);
        _baseGalleryTitle.SetActive(true);

        //Ativa GLG
        yield return new WaitForSeconds(0.3f);
        _baseGalleryGLG.SetActive(true);

        //Ativa botão de retorno
        yield return new WaitForSeconds(0.3f);
        _baseGalleryGoBackButton.SetActive(true);
        yield return null;

        _currGalleryMode = GalleryMode.Base;
    }

    //Muda a opção selecionada
    private void SetSelectedOption(int newColumn, int newRow)
    {
        //Não permite selecionar rows além das existentes
        if (newRow >= _galleryOptions.GetLength(0) || newRow < 0)
            return;

        //Se for pra última row, será sempre na coluna zero (onde está a opção voltar)
        if (newRow == _galleryOptions.GetLength(0) - 1)
            newColumn = 0;

        //Se estiver na opção voltar e for pra cima, seleciona a opção da coluna do meio
        if (_currOption.row == _galleryOptions.GetLength(0) - 1
            && newRow < _currOption.row)
            newColumn = Mathf.CeilToInt((_galleryOptions.GetLength(1) - 1) / 2);

        //Não permite selecionar colunas além das existentes
        if (newColumn >= _galleryOptions.GetLength(1) || newColumn < 0)
            return;

        //Impede a seleção de espaços nulos
        if (_galleryOptions[newRow, newColumn] == null)
            return;

        //Troca a opção selecionada
        _currOption.opt.OnUnchoose();

        _currOption.column = newColumn;
        _currOption.row = newRow;
        _currOption.opt = _galleryOptions[newRow, newColumn];

        _currOption.opt.OnChoose();
    }

    //Para outros scripts poderem marcar a galeria como fechada
    public void SetClosed() => _currGalleryMode = GalleryMode.Closed;

    //Abre o DrawingMenu e faz seu setup
    public void OpenDrawingMenu(DrawingData drawingData)
    {
        _currGalleryMode = GalleryMode.Drawing;
        _baseGallery.SetActive(false);
        _drawingMenu.SetActive(true);
        _infoMenu.SetActive(false);

        _drawingText.text = drawingData.Description;
        _drawingTitle.text = drawingData.Title;
        _drawingImage.sprite = drawingData.Drawing;
    }

    //Fecha o DrawingMenu
    public void CloseDrawingMenu()
    {
        _currGalleryMode = GalleryMode.Base;
        _baseGallery.SetActive(true);
        _drawingMenu.SetActive(false);
        _infoMenu.SetActive(false);
    }

    #endregion

    #region Input Methods

    private void LeftInput(InputAction.CallbackContext context)
    {
        //Nada ocorre se a galeria estiver em transição ou fechada
        if (_currGalleryMode == GalleryMode.Closed || _currGalleryMode == GalleryMode.Transition)
            return;

        //Modo base
        if (_currGalleryMode == GalleryMode.Base)
        {
            SetSelectedOption(_currOption.column - 1, _currOption.row);
        }
        //Modo drawing
        else if (_currGalleryMode == GalleryMode.Drawing)
        {
            if (_currOption.opt != _galleryOptions[0, 0])
            {
                _leftArrow.color = _selectedColor;
                _leftInfoArrow.color = _selectedColor;
                _rightArrow.color = _normalColor;
            }
            else
            {
                _leftInfoArrow.color = _lockedColor;
            }


            int newColumn = _currOption.column + -1;
            int newRow = _currOption.row;

            if (newColumn < 0)
            {
                newRow -= 1;
                newColumn = _galleryOptions.GetLength(1) - 1;
            }

            if (newRow < 0)
                return;

            SetSelectedOption(newColumn, newRow);

            if (_currOption.opt == _galleryOptions[0, 0])
            {
                _infoMenu.SetActive(true);
                _drawingMenu.SetActive(false);
                return;
            }

            GalleryOption galOpt = _currOption.opt as GalleryOption;
            OpenDrawingMenu(galOpt.drawingData);
        }
    }

    private void RightInput(InputAction.CallbackContext context)
    {
        //Nada ocorre se a galeria estiver em transição ou fechada
        if (_currGalleryMode == GalleryMode.Closed || _currGalleryMode == GalleryMode.Transition)
            return;

        //Modo base
        if (_currGalleryMode == GalleryMode.Base)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
                SetSelectedOption(_currOption.column + 1, _currOption.row);
        }
        //Modo drawing
        else if (_currGalleryMode == GalleryMode.Drawing)
        {
            if (_currOption.opt != _galleryOptions[_galleryOptions.GetLength(0) - 2, _galleryOptions.GetLength(1) - 1])
            {
                _rightArrow.color = _selectedColor;
                _rightInfoArrow.color = _selectedColor;
                _leftInfoArrow.color = _normalColor;
            }

            int newColumn = _currOption.column + 1;
            int newRow = _currOption.row;

            if (newColumn >= _galleryOptions.GetLength(1))
            {
                newRow += 1;
                newColumn = 0;
            }

            if (newRow >= _galleryOptions.GetLength(0) - 1)
                return;

            SetSelectedOption(newColumn, newRow);
            GalleryOption galOpt = _currOption.opt as GalleryOption;
            OpenDrawingMenu(galOpt.drawingData);
        }
    }

    private void UpInput(InputAction.CallbackContext context)
    {
        if (_currGalleryMode == GalleryMode.Closed || _currGalleryMode == GalleryMode.Transition)
            return;

        if (_currGalleryMode == GalleryMode.Base)
        {
            SetSelectedOption(_currOption.column, _currOption.row - 1);
        }
    }

    private void DownInput(InputAction.CallbackContext context)
    {
        if (_currGalleryMode == GalleryMode.Closed || _currGalleryMode == GalleryMode.Transition)
            return;

        if (_currGalleryMode == GalleryMode.Base)
        {
            SetSelectedOption(_currOption.column, _currOption.row + 1);
        }
    }

    private void LeftReleaseInput(InputAction.CallbackContext context)
    {
        if (_currGalleryMode == GalleryMode.Closed || _currGalleryMode == GalleryMode.Transition)
            return;

        if (_currGalleryMode == GalleryMode.Drawing)
        {
            if (_currOption.opt == _galleryOptions[_galleryOptions.GetLength(0) - 2, _galleryOptions.GetLength(1) - 1])
            {
                _rightArrow.color = _normalColor;
                _rightInfoArrow.color = _normalColor;
            }
            else
            {
                _leftArrow.color = _normalColor;
                _leftInfoArrow.color = _lockedColor;
            }
        }
    }

    private void RightReleaseInput(InputAction.CallbackContext context)
    {
        if (_currGalleryMode == GalleryMode.Closed || _currGalleryMode == GalleryMode.Transition)
            return;

        if (_currGalleryMode == GalleryMode.Drawing)
        {
            if (_currOption.opt != _galleryOptions[_galleryOptions.GetLength(0) - 2, _galleryOptions.GetLength(1) - 1])
            {
                _rightArrow.color = _normalColor;
                _rightInfoArrow.color = _normalColor;
            }
            else
            {
                _rightArrow.color = _lockedColor;
                _rightInfoArrow.color = _lockedColor;
            }
        }
    }

    private void ConfirmInput(InputAction.CallbackContext context)
    {
        if (_currGalleryMode == GalleryMode.Closed || _currGalleryMode == GalleryMode.Transition)
            return;

        if (_currGalleryMode == GalleryMode.Base)
        {
            _currOption.opt.Select();
            if (_currOption.opt == _galleryOptions[_galleryOptions.GetLength(0) - 2, _galleryOptions.GetLength(1) - 1])
            {
                _rightArrow.color = _lockedColor;
            }
        }
        else if (_currGalleryMode == GalleryMode.Drawing)
        {
            CloseDrawingMenu();
        }
    }

    #endregion
}
