using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessChestSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject _spawningArea;

    [SerializeField]
    private bool _isCircleArea = true;

    [SerializeField]
    private float _spawningAreaRadius = 8;

    [SerializeField]
    private GameObject _chestPrefab;

    private Bounds _spawningAreaBounds;
    private Bounds _chestPrefabBounds;
    private Vector2 _chestPrefabColliderOffset;

    [SerializeField]
    private float _spawnInterval = 20;

    [SerializeField]
    private float _randomSpawnIntervalRange = 5;

    private bool _spawningScheduled = false;

    [SerializeField]
    private bool _showDebugMessages = false;

    private HaltonDistribution _haltonDistribution;

    [SerializeField]
    private IncreaseFunction _spawningIncreaseFunction;

    private int _currentSpawnAmount = 0;

    private int _chestsLeftToOpen = 0;

    [SerializeField]
    private AudioClip chestSpawningClip;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private List<GameObject> _spawningPoints = new List<GameObject>();

    private void Awake()
    {
        _haltonDistribution = new HaltonDistribution(UnityEngine.Random.Range(2, 10000));
    }

    private void Start()
    {
        if (_spawningArea == null)
        {
            Logger.LogError("Cannot spawn endless chests because no spawningArea was assigned to the chests spawner.");
            return;
        }

        if (_spawningIncreaseFunction == null)
        {
            Logger.LogError("Cannot spawn endless chests because no spawning amount increase function was assigned to the chests spawner.");
            return;
        }

        _spawningAreaBounds = Util.GetAreaBounds(_spawningArea);
        Collider2D collider = _chestPrefab.GetComponent<Collider2D>();
        _chestPrefabBounds = collider.bounds;
        _chestPrefabColliderOffset = collider.offset;

        if (EndlessController.Instance)
        {
            Instantiate(_chestPrefab, new Vector3(45, 7, 0), Quaternion.identity);
            Instantiate(_chestPrefab, new Vector3(45, -7, 0), Quaternion.identity);
            Instantiate(_chestPrefab, new Vector3(45 - 7, 0, 0), Quaternion.identity);
            Instantiate(_chestPrefab, new Vector3(45 + 7, 0, 0), Quaternion.identity);
            _chestsLeftToOpen = 4;
        }
        else
        {
            // in arcade mode, there area 3 endless chest at the beginning of the boss room
            _chestsLeftToOpen = 3;
        }

        EventsManager.Subscribe<EndlessChestOpenedEvent>(OnEndlessChestOpened);
    }

    private Vector3 GetRandomSpawningPosition(List<GameObject> availableSpawningPoints)
    {
        if (availableSpawningPoints.Count == 0)
            return UnityEngine.Random.insideUnitSphere * 4f + transform.position;

        int pointIndex = UnityEngine.Random.Range(0, availableSpawningPoints.Count);

        Vector3 position = availableSpawningPoints[pointIndex].transform.position;

        availableSpawningPoints.RemoveAt(pointIndex);

        return position + new Vector3(UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1), 0);
    }

    private void OnEndlessChestOpened(EndlessChestOpenedEvent evt)
    {
        _chestsLeftToOpen = Mathf.Max(_chestsLeftToOpen - 1, 0);

        if (_spawningScheduled) return;

        if (_chestsLeftToOpen > 0) return;

        _spawningScheduled = true;
        StartCoroutine(InstantiateNewSpawner());
    }

    // TO-DO: Check if the spawning position is already occupied by an entity!
    private IEnumerator InstantiateNewSpawner()
    {
        if (_showDebugMessages) Logger.Log("Started instantiation coroutine");
        yield return new WaitForSeconds(_spawnInterval + UnityEngine.Random.Range(Mathf.Max(_spawnInterval - _randomSpawnIntervalRange, 0), _spawnInterval + _randomSpawnIntervalRange));

        // update the amount of chest based the amount of enemies defeated, if in arcade mode (endlesscontroller instance is false, we start at 300 so the chest spawn only health boosts)
        _currentSpawnAmount = _spawningIncreaseFunction.GetNewIntValue(EndlessController.Instance ? EndlessController.Instance.EnemiesDefeated : 300);

        for (int i = 0; i < _currentSpawnAmount; i++)
        {
            // to avoid infinite loop
            int tryCounter = 0;
            int maxTries = 100;

            var availableSpawningPoints = new List<GameObject>(_spawningPoints);

            Debug.Log("Starting spawning, there are " + availableSpawningPoints.Count + " spawning points.");

            while (tryCounter < maxTries)
            {

                Vector3 spawningPointPosition = GetRandomSpawningPosition(availableSpawningPoints);
                float t = Random.Range(0.4f, 1f);
                Vector3 candidateSpawningPosition = new Vector3(Mathf.Lerp(transform.position.x, spawningPointPosition.x, t), Mathf.Lerp(transform.position.y, spawningPointPosition.y, t), 0);

                if (!IsSpawningLocationAvailable(candidateSpawningPosition))
                {
                    tryCounter++;
                    continue;
                }

                Instantiate(_chestPrefab);
                _chestPrefab.transform.position = candidateSpawningPosition;
                if (audioSource != null) audioSource.PlayOneShot(chestSpawningClip);
                _chestsLeftToOpen++;
                EventsManager.Publish(new EndlessChestInsantiatedEvent());
                break;
            }
        }

        _spawningScheduled = false;
    }

    private bool IsSpawningLocationAvailable(Vector3 location)
    {
        Bounds candidateChestBounds = _chestPrefabBounds;
        candidateChestBounds.center = location + new Vector3(_chestPrefabColliderOffset.x, _chestPrefabColliderOffset.y, 0);

        // not performant but the thing should happen only sporadically
        Spawner[] spawners = FindObjectsOfType<Spawner>();
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        EndlessChestController[] chests = FindObjectsOfType<EndlessChestController>();
        PlayerController player = GameManager.Instance.Player;
        Bounds entityBounds;

        foreach (var entity in spawners)
        {
            if (entity == null) continue;

            entityBounds = Util.GetAreaBounds(entity.gameObject);

            if (Util.AreBoundsOverlapping(candidateChestBounds, entityBounds))
            {
                if (_showDebugMessages) Logger.Log("spawning chest candidate position overlaps with a spawner. Aborting.");
                return false;
            }
        }

        foreach (var entity in enemies)
        {
            if (entity == null) continue;

            entityBounds = entity.gameObject.GetComponent<Collider2D>().bounds;

            if (Util.AreBoundsOverlapping(candidateChestBounds, entityBounds))
            {
                if (_showDebugMessages) Logger.Log("spawning chest candidate position overlaps with enemies. Aborting.");
                return false;
            }
        }

        foreach (var entity in chests)
        {
            if (entity == null) continue;

            entityBounds = entity.gameObject.GetComponent<Collider2D>().bounds;

            if (Util.AreBoundsOverlapping(candidateChestBounds, entityBounds))
            {
                if (_showDebugMessages) Logger.Log("spawning chest candidate position overlaps with othe chests. Aborting.");
                return false;
            }
        }

        if (player == null) return false;

        entityBounds = player.gameObject.GetComponent<Collider2D>().bounds;

        if (Util.AreBoundsOverlapping(candidateChestBounds, entityBounds))
        {
            if (_showDebugMessages) Logger.Log("spawning chest candidate position overlaps with player. Aborting.");
            return false;
        }

        return true;
    }
}

