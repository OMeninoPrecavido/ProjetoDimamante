using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDamage : MonoBehaviour
{
    Collider2D _collider2D;
    PlayerMovement _playerMovement;
    SpriteRenderer _spriteRenderer;
    DashController _dashController;

    [SerializeField] LayerMask _ignoreOnInvulnerability;
    [SerializeField] float _invulnerabilityTime;
    [SerializeField] float _blinkingInterval;

    [SerializeField] int _startingLives = 3;
    public int Lives { get; private set; }
    public int Diamonds { get; private set; } = 0;
    public int AddDiamonds(int diamondsNum)
    {
        Diamonds += diamondsNum;
        if (Diamonds >= 10)
        {
            AddToLives(1);
            Diamonds = 0;
        }
        return diamondsNum;
    }

    void Start()
    {
        Lives = _startingLives;

        _collider2D = GetComponentInChildren<Collider2D>();    
        _playerMovement = GetComponent<PlayerMovement>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _dashController = GetComponent<DashController>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponentInParent<Enemy>() != null)
        {
            _dashController.CancelDash();
            AddToLives(-1);
            StartCoroutine(OnHit());

            if (Lives <= 0)
                SceneManager.LoadScene("Menu");
        }
    }

    IEnumerator OnHit()
    {
        _collider2D.excludeLayers = _ignoreOnInvulnerability;
        _playerMovement.Hit();
        
        yield return StartCoroutine(Blink());

        _collider2D.excludeLayers = new LayerMask();
    }

    IEnumerator Blink()
    {
        float elapsedTime = 0;
        float blinkingTime = 0;
        bool isTranslucent = false;
        while (elapsedTime < _invulnerabilityTime)
        {
            if (blinkingTime > _blinkingInterval)
            {
                Color c = _spriteRenderer.color;
                c.a = isTranslucent ? 1f : 0.5f;
                _spriteRenderer.color = c;
                blinkingTime = 0;
                isTranslucent = !isTranslucent;
            }

            blinkingTime += Time.deltaTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Color d = _spriteRenderer.color;
        d.a = 1f;
        _spriteRenderer.color = d;
    }

    public void AddToLives(int i)
    {
        Lives += i;
        UIManager.Instance.UpdateLives(Lives);
        if (Lives <= 0)
            Die();
    }

    public IEnumerator OnPitFall()
    {
        AddToLives(-1);
        _playerMovement.GoToClosestRespawn();

        _collider2D.excludeLayers = _ignoreOnInvulnerability;
        yield return StartCoroutine(Blink());
        _collider2D.excludeLayers = new LayerMask();
    }

    public void Die()
    {
        Debug.Log("Morreu!");
    }
}
