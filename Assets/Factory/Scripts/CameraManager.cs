using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera;

    void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        EventsManager.Subscribe<PlayerCreatedEvent>(OnPlayerCreated);
        EventsManager.Subscribe<PlayerDiedEvent>(OnPlayerDied);
    }

    void OnDestroy()
    {
        EventsManager.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        EventsManager.Unsubscribe<PlayerCreatedEvent>(OnPlayerCreated);
    }

    private void OnPlayerCreated(PlayerCreatedEvent evt)
    {
        var player = evt.Player;
        var playerTransform = player.transform;
        Logger.Log("Player created");
        virtualCamera.Follow = playerTransform;
        virtualCamera.ForceCameraPosition(playerTransform.position, Quaternion.identity);
    }

    private void OnPlayerDied(PlayerDiedEvent evt)
    {
        virtualCamera.Follow = null;
    }
}
