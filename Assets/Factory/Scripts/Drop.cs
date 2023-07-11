using Enum = System.Enum;
using UnityEngine;
using System.Collections.Generic;

public enum DropType
{
    HealthPotion,
    PowerUp,
}

public class Drop : MonoBehaviour
{
    public DropType dropType;
    public int value = 10;
    public PowerUpType powerUpType;
    public bool randomizePowerUp;
    public bool canDropGa = false;

    public GameObject pickupEffect;

    public AudioSource audioSource;
    public AudioClip pickupClip;

    public bool CanBePickedUp
    {
        get { return GetComponent<Collider2D>().enabled; }
        set { GetComponent<Collider2D>().enabled = value; }
    }

    void Pickup(PlayerController player)
    {
        if (!CanBePickedUp)
        {
            return;
        }

        switch (dropType)
        {
            case DropType.HealthPotion:
                player.Heal(value);
                break;
            case DropType.PowerUp:
                EventsManager.Publish(new PowerUpUnlockedEvent(GetCurrentPowerUpType()));
                break;
            default:
                break;
        }

        if (audioSource != null && pickupClip != null)
        {
            audioSource.PlayOneShot(pickupClip);
        }

        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private PowerUpType GetCurrentPowerUpType()
    {
        if (randomizePowerUp)
        {
            return GetRandomPowerUp();
        }

        return powerUpType;
    }

    public virtual PowerUpType GetRandomPowerUp()
    {
        var player = GameManager.Instance.Player;
        var playerPowerUps = player.GetUniquePowerUps();
        var allValues = (IEnumerable<PowerUpType>)Enum.GetValues(typeof(PowerUpType));
        var remainingValues = new List<PowerUpType>(allValues);

        remainingValues.RemoveAll((p) => playerPowerUps.Contains(p));

        if (canDropGa && !player.guardianAngel)
        {
            remainingValues.Add(PowerUpType.GuardianAngel);
        }

        var randomIndex = Random.Range(0, remainingValues.Count);
        return remainingValues[randomIndex];
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var player = collision.GetComponent<PlayerController>();
            Pickup(player);
        }
    }
}