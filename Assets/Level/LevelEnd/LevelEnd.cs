using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEnd : MonoBehaviour
{
    [SerializeField] int _levelNumber;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GlobalVariables.LevelsUnlocked <= _levelNumber)
            GlobalVariables.IncrementLevelsUnlocked();

        AudioManager.Instance.Play("Win");
        LevelManager.Instance.ChangeToScene("Menu");
    }
}
