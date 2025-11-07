using System.Collections;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    [SerializeField] RectTransform _lives;
    [SerializeField] TextMeshProUGUI _livesText;
    Coroutine _livesShowUpCoroutine;
    public void UpdateLives(int lives)
    {
        _livesText.text = "X" + lives.ToString();
        if (_livesShowUpCoroutine != null)
            StopCoroutine(_livesShowUpCoroutine);
        _livesShowUpCoroutine = StartCoroutine(ShowUpLives());
    }
    private IEnumerator ShowUpLives()
    {
        Vector2 startingPos = _lives.anchoredPosition;
        Vector2 newPos = new Vector2(startingPos.x, 75);
        _lives.gameObject.SetActive(true);

        yield return new WaitForSeconds(1);

        float elapsedTime = 0;
        float time = 0.6f;
        while (elapsedTime < time)
        {
            float ratio = elapsedTime / time;
            _lives.anchoredPosition = Vector2.Lerp(startingPos, newPos, ratio);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _lives.gameObject.SetActive(false);
        _lives.anchoredPosition = startingPos;

    }

    [SerializeField] RectTransform _pDiamonds;
    [SerializeField] TextMeshProUGUI _pDiamondsText;
    Coroutine _pDiamondsShowUpCoroutine;

    public void UpdatePurpleDiamonds(int pDiamonds)
    {
        _pDiamondsText.text = "X" + pDiamonds.ToString();
        if (_pDiamondsShowUpCoroutine != null)
            StopCoroutine(_pDiamondsShowUpCoroutine);
        _pDiamondsShowUpCoroutine = StartCoroutine(ShowUpPurpleDiamonds());
    }

    private IEnumerator ShowUpPurpleDiamonds()
    {
        Vector2 pDiamondsPos = _pDiamonds.anchoredPosition;
        Vector2 newPos = new Vector2(pDiamondsPos.x, 75);
        _pDiamonds.gameObject.SetActive(true);

        yield return new WaitForSeconds(1);

        float elapsedTime = 0;
        float time = 0.6f;
        while (elapsedTime < time)
        {
            float ratio = elapsedTime / time;
            _pDiamonds.anchoredPosition = Vector2.Lerp(pDiamondsPos, newPos, ratio);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _pDiamonds.gameObject.SetActive(false);
        _pDiamonds.anchoredPosition = pDiamondsPos;

    }
}
