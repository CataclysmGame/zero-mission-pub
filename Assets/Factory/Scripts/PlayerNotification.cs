using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct Notification
{
    public Sprite icon;
    public AudioClip clip;
}

[Serializable]
public struct PowerUpNotification
{
    public PowerUpType powerUpType;
    public Notification notification;
}

public class PlayerNotification : MonoBehaviour
{
    private enum NotificationType
    {
        Heal,
        PowerUp,
    }

    public PowerUpNotification[] powerUpNotifications;
    public Notification healNotification;

    public float moveDuration = 0.3f;

    public float shakeDuration = 0.3f;

    public float fadeDuration = 0.3f;

    public float moveDistance = 1.67f;

    public AudioSource audioSource;

    public UnityEvent onComplete;

    private SpriteRenderer spriteRenderer;

    private NotificationType notificationType = NotificationType.Heal;
    private PowerUpType powerUp = PowerUpType.IncreaseHealth;

    public void SetHeal()
    {
        notificationType = NotificationType.Heal;
    }

    public void SetPowerUp(PowerUpType powerUpType)
    {
        notificationType = NotificationType.PowerUp;
        powerUp = powerUpType;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (notificationType == NotificationType.Heal)
        {
            RunNotification(healNotification);
        }
        else if (notificationType == NotificationType.PowerUp)
        {
            var notif = GetPowerUpNotification();

            if (notif.HasValue)
            {
                RunNotification(notif.Value);
            }
        }
        else
        {
            Logger.LogError($"Unknown notification type: {notificationType}");
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }

    private Notification? GetPowerUpNotification()
    {
        foreach (var n in powerUpNotifications)
        {
            if (powerUp == n.powerUpType)
            {
                return n.notification;
            }
        }

        return null;
    }

    private void RunNotification(Notification notification)
    {
        spriteRenderer.sprite = notification.icon;
        if (audioSource != null && notification.clip != null)
        {
            audioSource.PlayOneShot(notification.clip);
        }

        var tweenParams = new TweenParams().SetEase(Ease.Linear);

        var moveTween = transform
            .DOLocalMoveY(transform.localPosition.y + moveDistance, moveDuration)
            .SetAs(tweenParams);

        var rotationTween = transform.DOShakeRotation(
            shakeDuration,
            new Vector3(0, 0, 90)
        );

        moveTween.onComplete += () => { onComplete.Invoke(); };

        DOTween.Sequence().Append(moveTween).Append(rotationTween).Append(
            spriteRenderer.DOFade(0.0f, fadeDuration)
        ).onComplete += () => Destroy(gameObject);
    }
}