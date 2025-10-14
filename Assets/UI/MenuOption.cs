using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MenuOption : MonoBehaviour
{
    RectTransform _rectTransform;
    TextMeshProUGUI _tmpro;

    [SerializeField] UnityEvent OnSelect;

    public bool IsLocked { get; set; } = false;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _tmpro = GetComponent<TextMeshProUGUI>();
    }

    public IEnumerator Highlight(float hValue, float time, Color highlightColor)
    {
        _tmpro.color = highlightColor;
        float startVal = _rectTransform.anchoredPosition.x;
        float finalVal = _rectTransform.anchoredPosition.x - hValue;
        float timeElapsed = 0;
        while (timeElapsed < time)
        {
            float ratio = timeElapsed / time;
            float newX = Mathf.Lerp(startVal, finalVal, ratio);
            _rectTransform.anchoredPosition = new Vector2(newX, _rectTransform.anchoredPosition.y);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        _rectTransform.anchoredPosition = new Vector2(finalVal, _rectTransform.anchoredPosition.y);
    }

    public IEnumerator UnHighlight(float hValue, float time, Color normalColor)
    {
        _tmpro.color = normalColor;
        float startVal = _rectTransform.anchoredPosition.x;
        float finalVal = _rectTransform.anchoredPosition.x + hValue;
        float timeElapsed = 0;
        while (timeElapsed < time)
        {
            float ratio = timeElapsed / time;
            float newX = Mathf.Lerp(startVal, finalVal, ratio);
            _rectTransform.anchoredPosition = new Vector2(newX, _rectTransform.anchoredPosition.y);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        _rectTransform.anchoredPosition = new Vector2(finalVal, _rectTransform.anchoredPosition.y);
    }

    public void Select()
    {
        OnSelect?.Invoke();
    }
}
