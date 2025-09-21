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
    Coroutine _showUpCoroutine;
    public void UpdateLives(int lives)
    {
        _livesText.text = "X" + lives.ToString();
        if (_showUpCoroutine != null)
            StopCoroutine(_showUpCoroutine);
        _showUpCoroutine = StartCoroutine(ShowUpLives());
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

}
