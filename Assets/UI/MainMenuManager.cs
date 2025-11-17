using System;
using System.Collections;
using System.Collections.Generic;

/* Manages Main Menu UI, which icludes the main menu options and the level options */

using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class MainMenuManager : MonoBehaviour
{
    [Header("-Main Menu References-")]
    [SerializeField] List<MenuOption> _menuOptions;
    [SerializeField] List<MenuOption> _levelOptions;
    [SerializeField] Transform _menuOptionsVLG;
    [SerializeField] GameObject _gameTitle;

    [Header("-Other Menu References-")]
    [SerializeField] RectTransform _galleryMenu;
    [SerializeField] float _galleryMenuTransitionTime;
    [SerializeField] RectTransform _creditsMenu;
    Vector2 _originalGalleryPos;
    Vector2 _originalCreditsPos;

    [Header("-Main Camera & Tilemap Refs-")]
    [SerializeField] Camera _camera;
    [SerializeField] Tilemap _solidsTilemap;

    [Header("-Option Selection Colors-")]
    [SerializeField] Color _selectedOptionColor;
    [SerializeField] Color _regularOptionColor;
    [SerializeField] Color _lockedOptionColor;

    //Option admin. variables
    (MenuOption opt, int index) _currMenuOption; //Currently selected option
    Coroutine _menuMovingOp; //Current UI movement operation, so one can wait for another to end

    //Main menu modes
    enum MainMenuMode { Menu, Level, Credits, Transition, Closed }
    MainMenuMode _currMenuMode = MainMenuMode.Menu;

    //Menu transitions aux. variables
    Vector3 _startingCameraPos;
    Vector3 _startingMainMenuPos;

    //Input action references
    InputAction _upPressAction;
    InputAction _downPressAction;
    InputAction _confirmAction;

    #region Event Functions

    private void Start()
    {
        InputSystem.actions.FindActionMap("Player").Disable();
        InputSystem.actions.FindActionMap("UI").Enable();

        _upPressAction = InputSystem.actions.FindAction("UI/UpPress");
        _downPressAction = InputSystem.actions.FindAction("UI/DownPress");
        _confirmAction = InputSystem.actions.FindAction("UI/Confirm");

        _upPressAction.performed += UpInput;
        _downPressAction.performed += DownInput;
        _confirmAction.performed += ConfirmInput;

        _startingCameraPos = _camera.transform.position;
        _startingMainMenuPos = transform.position;
        _originalCreditsPos = _creditsMenu.transform.position;

        for (int i = 0; i < _levelOptions.Count; i++)
        {
            if (i >= GlobalVariables.LevelsUnlocked && i < _levelOptions.Count - 1) //Last one is "Back"
                _levelOptions[i].IsLocked = true;
        }

        _currMenuOption.opt = _menuOptions[0];
        _currMenuOption.index = 0;
        StartCoroutine(SetStartingOption());
    }

    private void OnDestroy()
    {
        _upPressAction.performed -= UpInput;
        _downPressAction.performed -= DownInput;
        _confirmAction.performed -= ConfirmInput;
    }

    #endregion

    #region Main Menu Methods

    //Called at Start(). Needs to be a coroutine so it can be waitable
    private IEnumerator SetStartingOption()
    {
        yield return _menuMovingOp = StartCoroutine(_currMenuOption.opt.Highlight(200, 0.1f, _selectedOptionColor));
        _menuMovingOp = null;
    }

    //Changes the selected menu option and updates _currMenuOption variable.
    private IEnumerator ChangeSelectedOption(bool up, List<MenuOption> menuList)
    {
        if (_menuMovingOp != null)
            yield break;

        if ((_currMenuOption.index < menuList.Count - 1 && !up) || (_currMenuOption.index > 0) && up)
        {
            Color retreatColor = _regularOptionColor;
            Color advanceColor = _selectedOptionColor;

            if (_currMenuOption.opt.IsLocked)
                retreatColor = _lockedOptionColor;

            StartCoroutine(_currMenuOption.opt.UnHighlight(200, 0.1f, retreatColor));
            int newIndex = 0;

            if (!up)
                newIndex = _currMenuOption.index + 1;
            else
                newIndex = _currMenuOption.index - 1;

            _currMenuOption.opt = menuList[newIndex];
            _currMenuOption.index = newIndex;

            if (_currMenuOption.opt.IsLocked)
                advanceColor = _lockedOptionColor;

            yield return _menuMovingOp = StartCoroutine(_currMenuOption.opt.Highlight(200, 0.1f, advanceColor));
        }

        _menuMovingOp = null;
    }

    //Changes Main Menu into Level Menu
    private IEnumerator SetLevelMenu()
    {
        _currMenuMode = MainMenuMode.Transition;

        yield return StartCoroutine(TakeAwayOptions(_menuOptions, _currMenuOption.index));
        
        _currMenuOption.opt = _levelOptions[0];
        _currMenuOption.index = 0;

        yield return StartCoroutine(BringInOptions(_levelOptions, _currMenuOption.index));

        _currMenuMode = MainMenuMode.Level;
    }
    
    //Goes from level menu into main menu
    private IEnumerator LevelToMainMenu()
    {
        _currMenuMode = MainMenuMode.Transition;

        yield return StartCoroutine(TakeAwayOptions(_levelOptions, _currMenuOption.index));

        _currMenuOption.opt = _menuOptions[0];
        _currMenuOption.index = 0;

        yield return StartCoroutine(BringInOptions(_menuOptions, _currMenuOption.index));

        _currMenuMode = MainMenuMode.Menu;
    }

    //Makes options of a menu list gradually leave screen
    private IEnumerator TakeAwayOptions(List<MenuOption> options, int selectedOptionIndex)
    {
        for (int i = options.Count - 1; i >= 0; i--)
        {
            float retreatVal = 600;
            float retreatTime = 0.4f;
            Color retreatColor = _regularOptionColor;
                
            if (selectedOptionIndex == i)
            {
                retreatVal = 800;
                retreatTime = 0.5333f;
            }

            if (options[i].IsLocked)
            {
                retreatColor = _lockedOptionColor;
            }

            if (i == 0)
                yield return StartCoroutine(options[i].UnHighlight(retreatVal, retreatTime, retreatColor));
            else
                StartCoroutine(options[i].UnHighlight(retreatVal, retreatTime, retreatColor));

            yield return new WaitForSeconds(0.05f);
        }
    }

    //Makes options of a menu list gradually enter screen
    private IEnumerator BringInOptions(List<MenuOption> options, int selectedOptionIndex)
    {
        for (int i = options.Count - 1; i >= 0; i--)
        {
            float advanceVal = 600;
            float advanceTime = 0.4f;
            Color advanceColor = _regularOptionColor;

            if (selectedOptionIndex == i)
            {
                advanceVal = 800;
                advanceTime = 0.5333f;
                advanceColor = _selectedOptionColor;
            }

            if (options[i].IsLocked)
            {
                advanceColor = _lockedOptionColor;
            }

            if (i == 0)
                yield return StartCoroutine(options[i].Highlight(advanceVal, advanceTime, advanceColor));
            else
                StartCoroutine(options[i].Highlight(advanceVal, advanceTime, advanceColor));

            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator Initialize()
    {
        _currMenuMode = MainMenuMode.Transition;

        //Coloca a opção jogar como selecionada
        StartCoroutine(ChangeSelectedOption(true, _menuOptions));

        //Reposiciona menu na posição original
        transform.position = _startingMainMenuPos;

        //Ativa título
        yield return new WaitForSeconds(0.3f);
        _gameTitle.SetActive(true);

        //Ativa opções uma por uma
        for (int i = _menuOptionsVLG.childCount - 1; i >= 0; i--)
        {
            yield return new WaitForSeconds(0.1f);
            _menuOptionsVLG.GetChild(i).gameObject.SetActive(true);
        }

        _currMenuMode = MainMenuMode.Menu;
    }

    #endregion

    #region Credits Menu Methods

    private IEnumerator SetCreditsMenu()
    {
        _currMenuMode = MainMenuMode.Transition;

        float scaleFactor = GetComponentInParent<Canvas>().scaleFactor;

        float camHeight = _camera.orthographicSize * 2f;
        float newCamY = _camera.transform.position.y + camHeight;

        //Tira o menu principal de tela e move a câmera
        float elapsedTime = 0;
        Vector3 startingCamPos = _camera.transform.position;
        Vector3 startingMainMenuPos = GetComponent<RectTransform>().anchoredPosition;
        while (elapsedTime <= _galleryMenuTransitionTime)
        {
            //Move a câmera
            float ratio = elapsedTime / _galleryMenuTransitionTime;
            float currY = Mathf.Lerp(startingCamPos.y, newCamY, ratio);
            _camera.transform.position = new Vector3(_camera.transform.position.x, currY, _camera.transform.position.z);

            //Move a UI
            float canvasScreenOffset = (_camera.WorldToScreenPoint(_camera.transform.position).y - _camera.WorldToScreenPoint(startingCamPos).y) / scaleFactor; // Quanto a câmera já moveu
            GetComponent<RectTransform>().anchoredPosition = startingMainMenuPos - new Vector3(0, canvasScreenOffset, 0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _creditsMenu.transform.position = _originalCreditsPos;
        _creditsMenu.gameObject.SetActive(true);

        //Esconde os elementos do menu principal
        _gameTitle.SetActive(false);
        foreach (Transform child in _menuOptionsVLG)
        {
            child.gameObject.SetActive(false);
        }

        _currMenuMode = MainMenuMode.Credits;
    }

    private IEnumerator CloseCredits()
    {
        _currMenuMode = MainMenuMode.Transition;

        //Move a câmera de volta ao menu principal
        Vector3 startingPos = _camera.transform.position;
        Vector3 startingCreditsPos = _creditsMenu.anchoredPosition;
        Vector3 finalPos = _startingCameraPos;
        float elapsedTime = 0;

        float scaleFactor = GetComponentInParent<Canvas>().scaleFactor;

        while (elapsedTime <= _galleryMenuTransitionTime)
        {
            //Move a câmera
            float ratio = elapsedTime / _galleryMenuTransitionTime;
            float currY = Mathf.Lerp(startingPos.y, finalPos.y, ratio);
            _camera.transform.position = new Vector3(_camera.transform.position.x, currY, _camera.transform.position.z);

            //Move a UI dos creditos
            float canvasScreenOffset = (_camera.WorldToScreenPoint(_camera.transform.position).y - _camera.WorldToScreenPoint(startingPos).y) / scaleFactor; // Quanto a câmera já moveu
            _creditsMenu.anchoredPosition = startingCreditsPos - new Vector3(0, canvasScreenOffset, 0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        //Inicializa o menu inicial
        yield return StartCoroutine(Initialize());
    }

    #endregion

    #region Gallery Menu Methods

    private IEnumerator OpenGallery()
    {
        _currMenuMode = MainMenuMode.Transition;

        Bounds tilemapBounds = _solidsTilemap.localBounds;
        tilemapBounds.center += _solidsTilemap.transform.position; //Ajuste para coordenadas de mundo

        float scaleFactor = GetComponentInParent<Canvas>().scaleFactor;

        float camWidth = _camera.orthographicSize * 2f * _camera.aspect;
        float newCamX = tilemapBounds.min.x - camWidth / 2 - 1f;

        //Tira o menu principal de tela e move a câmera
        float elapsedTime = 0;
        Vector3 startingCamPos = _camera.transform.position;
        Vector3 startingMainMenuPos = GetComponent<RectTransform>().anchoredPosition;
        while (elapsedTime <= _galleryMenuTransitionTime)
        {
            //Move a câmera
            float ratio = elapsedTime / _galleryMenuTransitionTime;
            float currX = Mathf.Lerp(startingCamPos.x, newCamX, ratio);
            _camera.transform.position = new Vector3(currX, _camera.transform.position.y, _camera.transform.position.z);

            //Move a UI
            float canvasScreenOffset = (_camera.WorldToScreenPoint(_camera.transform.position).x - _camera.WorldToScreenPoint(startingCamPos).x) / scaleFactor; // Quanto a câmera já moveu
            GetComponent<RectTransform>().anchoredPosition = startingMainMenuPos - new Vector3(canvasScreenOffset, 0, 0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        //Ativa o menu da galeria
        StartCoroutine(_galleryMenu.GetComponent<GalleryManager>().Initialize());

        //Esconde os elementos do menu principal
        _gameTitle.SetActive(false);
        foreach (Transform child in _menuOptionsVLG)
        {
            child.gameObject.SetActive(false);
        }

        _currMenuMode = MainMenuMode.Closed;
    }

    private IEnumerator CloseGallery()
    {
        //Move a câmera de volta ao menu principal
        Vector3 startingPos = _camera.transform.position;
        Vector3 startingGalleryPos = _galleryMenu.anchoredPosition;
        Vector3 finalPos = _startingCameraPos;
        float elapsedTime = 0;

        float scaleFactor = GetComponentInParent<Canvas>().scaleFactor;

        //Coloca a galeria no modo fechado
        _galleryMenu.GetComponent<GalleryManager>().SetClosed();

        while (elapsedTime <= _galleryMenuTransitionTime)
        {
            //Move a câmera
            float ratio = elapsedTime / _galleryMenuTransitionTime;
            float currX = Mathf.Lerp(startingPos.x, finalPos.x, ratio);
            _camera.transform.position = new Vector3(currX, _camera.transform.position.y, _camera.transform.position.z);

            //Move a UI da galeria
            float canvasScreenOffset = (_camera.WorldToScreenPoint(_camera.transform.position).x - _camera.WorldToScreenPoint(startingPos).x) / scaleFactor; // Quanto a câmera já moveu
            _galleryMenu.anchoredPosition = startingGalleryPos - new Vector3(canvasScreenOffset, 0, 0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        //Inicializa o menu inicial
        yield return StartCoroutine(Initialize());

    }

    #endregion

    #region UnityEvent Editor Methods

    public void SetLevelMenuED() => StartCoroutine(SetLevelMenu());
    public void OpenGalleryED() => StartCoroutine(OpenGallery());
    public void CloseGalleryED() => StartCoroutine(CloseGallery());
    public void LevelToMainMenuED() => StartCoroutine(LevelToMainMenu());
    public void OpenCreditsED() => StartCoroutine(SetCreditsMenu());
    public void StartLevel1(){
        StopAllCoroutines();
        SceneManager.LoadScene("Level1");
    }
    public void StartLevel2(){
        StopAllCoroutines();
        SceneManager.LoadScene("Level2");
    }
    public void StartLevel3()
    {
        StopAllCoroutines();
        SceneManager.LoadScene("Level3");
    }
    public void StartLevel4()
    {
        StopAllCoroutines();
        SceneManager.LoadScene("Level4");
    }
    public void StartLevel5()
    {
        StopAllCoroutines();
        SceneManager.LoadScene("Level5");
    }

    public void StartLevel6()
    {
        StopAllCoroutines();
        SceneManager.LoadScene("Level6");
    }

    #endregion

    #region Input Methods

    private void UpInput(InputAction.CallbackContext context)
    {
        if (_currMenuMode == MainMenuMode.Menu)
            StartCoroutine(ChangeSelectedOption(true, _menuOptions));
        else if (_currMenuMode == MainMenuMode.Level)
            StartCoroutine(ChangeSelectedOption(true, _levelOptions));
    }

    private void DownInput(InputAction.CallbackContext context)
    {
        if (_currMenuMode == MainMenuMode.Menu)
            StartCoroutine(ChangeSelectedOption(false, _menuOptions));
        else if (_currMenuMode == MainMenuMode.Level)
            StartCoroutine(ChangeSelectedOption(false, _levelOptions));
    }

    private void ConfirmInput(InputAction.CallbackContext context)
    {
        if (_currMenuMode == MainMenuMode.Credits)
        {
            StartCoroutine(CloseCredits());
            return;
        }

        if (_currMenuMode != MainMenuMode.Closed && _currMenuMode != MainMenuMode.Transition
            && !_currMenuOption.opt.IsLocked)
            _currMenuOption.opt.Select();
    }

    #endregion
}
