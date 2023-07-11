using UnityEngine;

public class EndlessSpawner : Spawner
{
    [SerializeField]
    private IncreaseFunction _spawnGroupSizeFunction;

    [SerializeField]
    private IncreaseFunction _spawnIntervalFunction;

    [SerializeField]
    private IncreaseFunction _maxEnemiesOnScreenFunction;

    [SerializeField]
    private GameObject _bossPrefab;

    [SerializeField]
    private IncreaseFunction _bossSpawnAmountFunction;

    [SerializeField]
    private float _killingRateThreshold = 0.4f;

    private void Update()
    {
        float enemiesDefeated = EndlessController.Instance.EnemiesDefeated;
        var elapsedTime = EndlessController.Instance.ElapsedTime;

        if (_spawnIntervalFunction.GetNewIntValue(enemiesDefeated) < spawnInterval)
        {
            ChangeInterval(_spawnIntervalFunction.GetNewIntValue(enemiesDefeated), true);
        }

        if (_spawnGroupSizeFunction.GetNewIntValue(enemiesDefeated) > spawnGroupSize)
        {
            spawnGroupSize = _spawnGroupSizeFunction.GetNewIntValue(enemiesDefeated);
        }

        float killingRate = (enemiesDefeated + 1) / elapsedTime;
        float maxEnemiesOnScreenInputVariable = (enemiesDefeated + 1) / (killingRate < _killingRateThreshold ? Mathf.Pow(killingRate, 1.4f) : 1f);
        int newMaxEnemiesOnScreen = _maxEnemiesOnScreenFunction.GetNewIntValue(maxEnemiesOnScreenInputVariable);
        if (newMaxEnemiesOnScreen > areaTrigger)
        {
            areaTrigger = newMaxEnemiesOnScreen;
        }
    }

    public override Spawnable? RandomSpawnable()
    {
        if (sortedRates == null) return null;

        float enemiesDefeated = EndlessController.Instance.EnemiesDefeated;
        BossController[] _currentlySpawnedBosses = FindObjectsOfType<BossController>();

        int bossesLeftToSpawn = Mathf.Max(_bossSpawnAmountFunction.GetNewIntValue(enemiesDefeated) - _currentlySpawnedBosses.Length);

        if (bossesLeftToSpawn > 0)
        {
            return new Spawnable()
            {
                prefab = _bossPrefab
            };
        }

        return base.RandomSpawnable();
    }

    public override void Spawn()
    {
        var count = FindObjectsOfType<EnemyController>().Length;

        if (spawnArea != null && areaTrigger >= 0)
        {
            if (count > areaTrigger)
            {
                return;
            }
        }

        for (var i = 0; i < spawnGroupSize; i++)
        {
            Debug.Log("There are " + count + " enemies in game and there can be max " + areaTrigger);
            if (count > areaTrigger)
            {
                Debug.Log("Skipping spawn");
                continue;
            }

            var maybeSpawnable = RandomSpawnable();
            if (!maybeSpawnable.HasValue)
            {
                Logger.Log("Got a null spawnable");
                continue;
            }

            var spawnable = maybeSpawnable.Value;
            var location = transform.position;
            var rotation = Quaternion.identity;
            if (spawnPoint != null)
            {
                location = spawnPoint.transform.position;
            }
            if (spawnable.transform != null)
            {
                location = spawnable.transform.position;
                rotation = spawnable.transform.rotation;
            }
            var randomizedLocation = location + new Vector3(UnityEngine.Random.Range(1f, 2f), UnityEngine.Random.Range(1f, 2f), 0);

            Instantiate(spawnable.prefab, randomizedLocation, rotation);
            spawnCounter++;
            count++;

            if (spawnEffect != null)
            {
                Instantiate(spawnEffect, transform);
            }

            if (animator != null)
            {
                animator.SetTrigger("spawn");
            }

            if (maxSpawns != 0 && spawnCounter >= maxSpawns)
            {
                StopSpawning();
                return;
            }
        }

        if (audioSource != null && spawnClip != null)
        {
            audioSource.PlayOneShot(spawnClip);
        }
    }
}