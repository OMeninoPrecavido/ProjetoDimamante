using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SignUIManager : MonoBehaviour
{
    public static SignUIManager Instance;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        //So there isn't lag the first time it loads;
        _signView.gameObject.SetActive(true);
        _signView.GetComponentInChildren<TextMeshProUGUI>().text = "";
        _signView.GetComponent<Image>().color = new Color(1, 1, 1, 0);
    }

    #region Sign

    [SerializeField] RectTransform _signView;
    public void ActivateSignView(string text, Vector3 worldPosition)
    {
        _signView.GetComponent<Image>().color = new Color(1, 1, 1, 1);
        _signView.gameObject.SetActive(true);
        _signView.GetComponentInChildren<TextMeshProUGUI>().text = text;
        _signView.position = worldPosition;
    }

    public void DeactivateSignView()
    {
        _signView.gameObject.SetActive(false);
    }

    #endregion
}
