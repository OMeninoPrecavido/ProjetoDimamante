using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MenuOptionBase : MonoBehaviour
{
    RectTransform _rectTransform;

    [SerializeField] UnityEvent OnSelect;

    protected virtual void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Select()
    {
        OnSelect?.Invoke();
    }

    public virtual void OnChoose() { }
    public virtual void OnUnchoose() { }

    public void SetSelectedEvent(Action method)
    {
        OnSelect.AddListener(delegate { method(); });
    }
}
