using UnityEngine;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{
    void Awake()
    {
        InputSystem.actions.FindActionMap("UI").Disable();
        InputSystem.actions.FindActionMap("Player").Enable();
    }
}
