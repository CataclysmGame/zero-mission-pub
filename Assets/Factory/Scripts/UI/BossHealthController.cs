using UnityEngine;
using UnityEngine.UI;

public class BossHealthController : MonoBehaviour
{
    public Image barImage;
    public GameObject backGraphics;

    private BossController currentBoss;
    
    void Start()
    {
        EventsManager.Subscribe<BossActivatedEvent>(OnBossActivated);
        EventsManager.Subscribe<BossHealthChangedEvent>(OnBossHealthChanged);
        EventsManager.Subscribe<BossDiedEvent>(OnBossDied);
    }

    void OnDestroy()
    {
        EventsManager.Unsubscribe<BossDiedEvent>(OnBossDied);
        EventsManager.Unsubscribe<BossHealthChangedEvent>(OnBossHealthChanged);
        EventsManager.Unsubscribe<BossActivatedEvent>(OnBossActivated);
    }

    void OnBossActivated(BossActivatedEvent evt)
    {
        currentBoss = evt.Boss;
        SetBarEnabled(true);
        SetHealthBar(currentBoss.currentHP, currentBoss.maxHP);
    }

    void OnBossDied(BossDiedEvent evt)
    {
        if (evt.Boss == currentBoss)
        {
            SetBarEnabled(false);
            currentBoss = null;
        }
    }

    void OnBossHealthChanged(BossHealthChangedEvent evt)
    {
        if (currentBoss != null && currentBoss == evt.Boss)
        {
            SetHealthBar(evt.CurrentHP, evt.Boss.maxHP);
        }
    }

    private void SetBarEnabled(bool enabled)
    {
        barImage.enabled = enabled;
        backGraphics.SetActive(enabled);
    }

    private void SetHealthBar(int currentHP, int maxHP)
    {
        barImage.gameObject.transform.localScale = new Vector3(
            currentHP / (float)maxHP,
            1.0f,
            1.0f
        );
    }
}
