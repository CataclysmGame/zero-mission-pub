using System.Collections;
using Factory.Scripts.Events;
using UnityEngine;
using UnityEngine.UI;

public class MunitionsController : MonoBehaviour
{
    public Sprite fullSprite;
    public Sprite emptySprite;

    public Image[] images;

    private bool _reloading;

    private Coroutine _fillCoroutine;
    
    private void Awake()
    {
        EventsManager.Subscribe<PlayerReloadStartedEvent>(OnPlayerReloadStarted);
        EventsManager.Subscribe<PlayerMunitionsChangedEvent>(OnMunitionsChanged);
        EventsManager.Subscribe<PlayerDiedEvent>(OnPlayerDied);
    }

    private void OnDestroy()
    {
        EventsManager.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        EventsManager.Unsubscribe<PlayerMunitionsChangedEvent>(OnMunitionsChanged);
        EventsManager.Unsubscribe<PlayerReloadStartedEvent>(OnPlayerReloadStarted);
    }

    private void OnPlayerDied(PlayerDiedEvent evt)
    {
        foreach (var img in images)
        {
            img.enabled = false;
        }
    }

    private void OnMunitionsChanged(PlayerMunitionsChangedEvent evt)
    {
        if (_fillCoroutine != null)
        {
            StopCoroutine(_fillCoroutine);
        }

        Logger.Log($"Munitions changed {evt.CurMunitions}/{evt.MaxMunitions}");
        SetMunitions(evt.CurMunitions, evt.MaxMunitions);
        _reloading = false;
    }

    private void OnPlayerReloadStarted(PlayerReloadStartedEvent evt)
    {
        _reloading = true;
        _fillCoroutine = StartCoroutine(ReloadCoroutine(evt.Duration, evt.CurrentMunitions, evt.MaxMunitions));
    }

    private IEnumerator ReloadCoroutine(float duration, int current, int max)
    {
        var waitTime = duration / (max-current);
        var cur = current;
        while (cur < max && _reloading)
        {
            yield return new WaitForSeconds(waitTime);
            cur++;
            SetMunitions(cur, max);
        }

        _reloading = false;
    }

    private void SetMunitions(int cur, int max)
    {
        for (var i = 0; i < images.Length; i++)
        {
            if (i >= max)
            {
                images[i].enabled = false;
            }
            else
            {
                var full = i < cur;
                images[i].enabled = full || emptySprite != null;
                images[i].sprite = full ? fullSprite : emptySprite;
            }
        }
    }
}