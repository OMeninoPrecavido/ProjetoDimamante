using System;
using System.Collections;
using System.Collections.Generic;
/* Manages Main Menu UI, which icludes the main menu options and the level options */

using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    //Other menu references
    [SerializeField] RectTransform _galleryMenu;
    [SerializeField] float _galleryMenuTransitionTime;
    Vector2 _originalGalleryPos;


    //MenuOption references
    [SerializeField] List<MenuOption> _menuOptions;
    [SerializeField] List<MenuOption> _levelOptions;

    (MenuOption opt, int index) _currMenuOption; //Currently selected option

    Coroutine _menuMovingOp; //Current UI movement operation, so one can wait for another to end

    //Selection/Unselection colors
    [SerializeField] Color _selectedOptionColor;
    [SerializeField] Color _regularOptionColor;
    [SerializeField] Color _lockedOptionColor;

    //Main menu modes
    enum MainMenuMode { Menu, Level, Transition, Closed }
    MainMenuMode _currMenuMode = MainMenuMode.Menu;

    private void Start()
    {
        for (int i = 0; i < _levelOptions.Count; i++)
        {
            if (i >= GlobalVariables.LevelsUnlocked)
                _levelOptions[i].IsLocked = true;
        }

        _currMenuOption.opt = _menuOptions[0];
        _currMenuOption.index = 0;
        StartCoroutine(SetStartingOption());
    }

    private void Update()
    {
        //Provisory input system for UI navigation
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (_currMenuMode == MainMenuMode.Menu)
                StartCoroutine(ChangeSelectedOption(false, _menuOptions));
            else if (_currMenuMode == MainMenuMode.Level)
                StartCoroutine(ChangeSelectedOption(false, _levelOptions));
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (_currMenuMode == MainMenuMode.Menu)
                StartCoroutine(ChangeSelectedOption(true, _menuOptions));
            else if (_currMenuMode == MainMenuMode.Level)
                StartCoroutine(ChangeSelectedOption(true, _levelOptions));
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            if (_currMenuMode != MainMenuMode.Closed && _currMenuMode != MainMenuMode.Transition)
                _currMenuOption.opt.Select();
        }
    }

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

        if ((this._currMenuOption.index < menuList.Count - 1 && !up) || (_currMenuOption.index > 0) && up)
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

    private IEnumerator OpenGallery()
    {
        _originalGalleryPos = _galleryMenu.anchoredPosition;
        _currMenuMode = MainMenuMode.Closed;

        float timeElapsed = 0;
        Vector2 startingPosition = _galleryMenu.anchoredPosition;
        Vector2 finalPosition = Vector2.zero;
        while (timeElapsed <= _galleryMenuTransitionTime)
        {
            float ratio = timeElapsed / _galleryMenuTransitionTime;
            _galleryMenu.anchoredPosition = Vector2.Lerp(startingPosition, finalPosition, ratio);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        _galleryMenu.anchoredPosition = finalPosition;
        _galleryMenu.GetComponent<GalleryManager>().GalleryIsOpen = true;
    }

    private IEnumerator CloseGallery()
    {
        _galleryMenu.GetComponent<GalleryManager>().GalleryIsOpen = false;

        _currMenuMode = MainMenuMode.Transition;

        float timeElapsed = 0;
        Vector2 startingPosition = Vector2.zero;
        Vector2 finalPosition = _originalGalleryPos;

        while (timeElapsed <= _galleryMenuTransitionTime)
        {
            float ratio = timeElapsed / _galleryMenuTransitionTime;
            _galleryMenu.anchoredPosition = Vector2.Lerp(startingPosition, finalPosition, ratio);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        _galleryMenu.anchoredPosition = finalPosition;

        _currMenuMode = MainMenuMode.Menu;
    }

    #region UnityEvent Editor Methods

    public void SetLevelMenuED() => StartCoroutine(SetLevelMenu());
    public void OpenGalleryED() => StartCoroutine(OpenGallery());
    public void CloseGalleryED() => StartCoroutine(CloseGallery());

    #endregion
}
