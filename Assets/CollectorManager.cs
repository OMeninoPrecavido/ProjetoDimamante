using UnityEngine;

public class CollectorManager : MonoBehaviour
{
    int _counter = 0;
    [SerializeField] int _goal = 6;
    [SerializeField] GameObject _levelEndRef;
    PurpleDiamond[] _purpleDiamonds;

    private void Start()
    {
        _levelEndRef.SetActive(false);

        _purpleDiamonds = FindObjectsByType<PurpleDiamond>(FindObjectsSortMode.None);

        foreach (PurpleDiamond pd in _purpleDiamonds)
        {
            pd.OnCollected += IncrementCounter;
        }
    }

    private void IncrementCounter()
    {
        _counter++;

        UIManager.Instance.UpdatePurpleDiamonds(_counter);

        if (_counter >= _goal)
        {
            _levelEndRef?.SetActive(true);
        }
    }
}
