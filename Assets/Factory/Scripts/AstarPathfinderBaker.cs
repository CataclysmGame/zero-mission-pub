using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstarPathfinderBaker : MonoBehaviour
{
    private AstarPath _astarPath;

    private void Awake()
    {
        _astarPath = GetComponent<AstarPath>();
        EventsManager.Subscribe<EndlessChestInsantiatedEvent>(OnChestInstantiated);
    }

    private void OnChestInstantiated(EndlessChestInsantiatedEvent evt)
    {
        _astarPath.ScanAsync();
    }
}
