using UnityEngine;
using UnityEngine.UI;

public class PowerUpUIController : MonoBehaviour
{
    public Image diagonalShot;
    public Image doubleShot;
    public Image backShot;
    public Image guardianAngel;

    // Start is called before the first frame update
    private void Start()
    {
        EventsManager.Subscribe<PowerUpUnlockedEvent>(OnPowerUpUnlocked);
    }

    private void OnDestroy()
    {
        EventsManager.Unsubscribe<PowerUpUnlockedEvent>(OnPowerUpUnlocked);
    }

    private void OnPowerUpUnlocked(PowerUpUnlockedEvent evt)
    {
        Logger.Log($"Power Up unlocked: {evt.PowerUpType}");
        switch (evt.PowerUpType)
        {
            case PowerUpType.DiagonalShot:
                diagonalShot.gameObject.SetActive(true);
                break;
            case PowerUpType.DoubleShot:
                doubleShot.gameObject.SetActive(true);
                break;
            case PowerUpType.BackShot:
                backShot.gameObject.SetActive(true);
                break;
            case PowerUpType.GuardianAngel:
                guardianAngel.gameObject.SetActive(true);
                break;
        }
    }
}
