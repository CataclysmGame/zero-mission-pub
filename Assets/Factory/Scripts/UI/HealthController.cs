using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthController : MonoBehaviour
{
    public Image barImage;
    public TMP_Text hpText;
    public Color[] barColors;

    public Vector2 shakeStrength = new(5, 5);
    public float shakeDuration = 0.3f;

    private Vector3 _originalPosition;

    private void Awake()
    {
        _originalPosition = transform.position;
    }

    private void Start()
    {
        EventsManager.Subscribe<PlayerHitEvent>(OnPlayerHit);
        EventsManager.Subscribe<PlayerHealthChangedEvent>(OnPlayerHealthChanged);
        EventsManager.Subscribe<PlayerDiedEvent>(OnPlayerDied);

        Logger.Log("Subscribed to player hit event");

        UpdateBar();
    }

    private void OnDestroy()
    {
        EventsManager.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        EventsManager.Unsubscribe<PlayerHealthChangedEvent>(OnPlayerHealthChanged);
        EventsManager.Unsubscribe<PlayerHitEvent>(OnPlayerHit);
    }

    private void OnPlayerHit(PlayerHitEvent evt)
    {
        ShakeBar();
        UpdateBar();
    }

    private void OnPlayerHealthChanged(PlayerHealthChangedEvent evt)
    {
        UpdateBar();
    }

    private void OnPlayerDied(PlayerDiedEvent evt)
    {
        barImage.transform.localScale = new Vector3(1, 1, 1);
        barImage.color = barColors[0];
    }

    private void UpdateBar()
    {
        var player = GameManager.Instance.Player;
        var factor = player.currentHP / (float)player.maxHP;

        barImage.transform.localScale = new Vector3(factor, 1, 1);

        hpText.text = $"{player.currentHP}/{player.maxHP}";
        if (barColors.Length > 0)
        {
            var color = barColors[Mathf.CeilToInt(factor * (barColors.Length - 1))];
            barImage.color = color;
        }
    }

    private void ResetBarPosition() =>
        transform.position = _originalPosition;

    private void ShakeBar()
    {
        if (shakeDuration > 0)
        {
            var tween = transform.DOShakePosition(shakeDuration, shakeStrength);
            tween.onComplete += ResetBarPosition;
            // Fix for onComplete not being called sometimes
            Invoke(nameof(ResetBarPosition), shakeDuration + 0.05f);
        }
    }
}