using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    [SerializeField] Animator _transitionAnimator;
    [SerializeField] AnimationClip _transitionInClip;
    [SerializeField] AnimationClip _transitionOutClip;

    void Awake()
    {
        AudioManager.Instance.SetMusic("LevelMusic");

        InputSystem.actions.FindActionMap("UI").Disable();
        InputSystem.actions.FindActionMap("Player").Enable();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        _transitionAnimator.SetTrigger("ExitTransition");
    }

    public void ChangeToScene(string sceneName)
    {
        StartCoroutine(ChangeToSceneCoroutine(sceneName));
    }

    private IEnumerator ChangeToSceneCoroutine(string sceneName)
    {
        GlobalVariables.shouldMenuTransition = true;
        _transitionAnimator.SetTrigger("EnterTransition");
        yield return new WaitForSeconds(_transitionOutClip.length);
        SceneManager.LoadScene(sceneName);
    }
}
