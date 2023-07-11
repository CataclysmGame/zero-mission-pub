using Enum = System.Enum;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class EndlessDrop : Drop
{
    [Serializable]
    private struct DropsListEntry
    {
        public PowerUpType powerUpType;

        [Range(0, 1)]
        public float weight;
    }

    [Tooltip("The list of possible drops for this randomized drop, with theri relative sorting weight.")]
    [SerializeField]
    private List<DropsListEntry> _dropsPool;

    public override PowerUpType GetRandomPowerUp()
    {
        var startPos = transform.position - Vector3.forward;

        var playerPowerUps = GameManager.Instance.Player.GetUniquePowerUps(true).ToList();
        var uniqueDrops = new List<DropsListEntry>();
        foreach (var dropEntry in _dropsPool)
        {
            if (!playerPowerUps.Contains(dropEntry.powerUpType))
            {
                uniqueDrops.Add(dropEntry);
            };
        }

        if (uniqueDrops.Count == 0)
        {
            return PowerUpType.IncreasedDamage;
        }

        return uniqueDrops[Util.GetRandomWeightedIndex(uniqueDrops.Select(d => d.weight).ToList())].powerUpType;
    }
}