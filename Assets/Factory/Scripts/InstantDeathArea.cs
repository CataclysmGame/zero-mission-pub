using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerEnteredInstantDeathAreaEvent : IEvent { }

public class InstantDeathArea : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            EventsManager.Publish(new PlayerEnteredInstantDeathAreaEvent());
        }
    }
}
