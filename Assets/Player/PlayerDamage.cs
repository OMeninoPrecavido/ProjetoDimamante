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
    [SerializeField] AudioClip _pitFallClip;

    [SerializeField] int _startingLives = 3;
    public int Lives { get; private set; }
    public bool IsInvulnerable { get; private set; } = false;
    public int Diamonds { get; private set; } = 0;
    public int AddDiamonds(int diamondsNum)
    {
        Diamonds += diamondsNum;
        if (Diamonds >= 10)
        {
            AudioManager.Instance.Play("NewLife");
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
        }
    }

    IEnumerator OnHit()
    {
        AudioManager.Instance.Play("Thump");

        _collider2D.excludeLayers = _ignoreOnInvulnerability;
        _playerMovement.Hit();
        IsInvulnerable = true;
        
        if (Lives > 0)
        {
            yield return StartCoroutine(Blink());

            IsInvulnerable = false;
            _collider2D.excludeLayers = new LayerMask();
        }
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
        AudioManager.Instance.Play("PitFall");

        _playerMovement.EnableMovement(false);
        _playerMovement.RemoveHorizontalVelocity();

        yield return new WaitForSeconds(_pitFallClip.length);

        if (!IsInvulnerable)
            AddToLives(-1);

        if (Lives > 0)
        {
            _playerMovement.EnableMovement(true);
            _playerMovement.GoToClosestRespawn();
            _collider2D.excludeLayers = _ignoreOnInvulnerability;
            yield return StartCoroutine(Blink());
            _collider2D.excludeLayers = new LayerMask();
        }
    }

    public void Die()
    {
        AudioManager.Instance.Play("Lose");

        _playerMovement.Freeze(true);
        EnemyManager.Instance.EnableEnemyMovement(false);

        LevelManager.Instance.ChangeToScene("Menu");
    }
}
