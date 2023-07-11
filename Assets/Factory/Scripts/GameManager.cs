using System;
using System.Collections.Generic;
using DG.Tweening;
using Sentry;
using UnityEngine;

public enum GameState
{
    Idle,
    Battle,
    BossBattle,
    BossDefeated,
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public List<GameObject> playerPrefabs;

    public GameObject projectilePrefab;
    public GameObject explosionPrefab;

    public bool spawnersTriggerBattle = false;

    [HideInInspector] public GameObjectsPool ProjectilesPool;
    public GameObjectsPool ExplosionsPool;

    [HideInInspector] public PlayerController Player { get; set; }

    [HideInInspector] public bool gameOver = false;

    public bool EnemyNear { get; private set; } = false;

    [HideInInspector] public GameState State { get; private set; } = GameState.Idle;

    private int _defeatedEnemies;

    public int EnemiesDefeated => _defeatedEnemies;

    private void Awake()
    {
        Instance = this;

        _defeatedEnemies = 0;

        ProjectilesPool = new GameObjectsPool(
            "ProjectilesPool",
            projectilePrefab,
            (GameObject obj) =>
            {
                obj.GetComponent<Projectile>().Reset();
                return obj;
            },
            25
        );

        ExplosionsPool = new GameObjectsPool(
            "ExplosionsPool",
            explosionPrefab,
            (GameObject obj) =>
            {
                obj.GetComponent<ExplosionController>().Reset();
                return obj;
            },
            25
        );

        DOTween.Init();

        EventsManager.Subscribe<PlayerCreatedEvent>(OnPlayerCreated);
        EventsManager.Subscribe<PlayerDiedEvent>(OnPlayerDied);

        EventsManager.Subscribe<BossActivatedEvent>(OnBossActivated);
        EventsManager.Subscribe<BossDiedEvent>(OnBossDied);

        EventsManager.Subscribe<EnemyDiedEvent>(OnEnemyDied);

        InstantiatePlayer();

        RenderSettings.ambientIntensity = 1.0f;

        InvokeRepeating(nameof(CheckIsInBattle), 0.1f, 0.25f);
    }

    private void OnDestroy()
    {
        CancelInvoke();
        EventsManager.Unsubscribe<BossDiedEvent>(OnBossDied);
        EventsManager.Unsubscribe<BossActivatedEvent>(OnBossActivated);
        EventsManager.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        EventsManager.Unsubscribe<PlayerCreatedEvent>(OnPlayerCreated);
        EventsManager.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
    }

    private void ChangeState(GameState newState)
    {
        var prevState = State;
        State = newState;

        Logger.Log($"Game state changed: {prevState} -> {State}");

        EventsManager.Publish(new GameStateChangedEvent(prevState, State));
    }

    private void OnPlayerCreated(PlayerCreatedEvent evt)
    {
        Player = evt.Player;
        gameOver = false;
    }

    private void OnPlayerDied(PlayerDiedEvent evt)
    {
        //Player = null;
        gameOver = true;

#if UNITY_WEBGL
        try
        {
            var characterName = Player.character.playerName;
            var enemyArea = GetPlayerEnemyArea();
            var isEndless = GameObject.Find("Endless") != null;
            var endlessInt = isEndless ? 1 : 0;

            SentrySdk.AddBreadcrumb("Submitting player death", type: "Player death",
                data: new Dictionary<string, string>
                {
                    { "userAddress", PersistentSettings.Instance.UserAddress },
                    { "characterName", characterName },
                    { "endlessInt", endlessInt.ToString() },
                });

            var signature = new Signore().SGameOver(
                endlessInt,
                _defeatedEnemies
            );

            Application.ExternalCall(
                "window._unityGameOver",
                characterName,
                _defeatedEnemies,
                enemyArea,
                endlessInt,
                signature
            );
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
            SentrySdk.CaptureException(ex);
        }
#endif
    }

    private void OnEnemyDied(EnemyDiedEvent evt)
    {
        _defeatedEnemies++;
    }

    void OnBossActivated(BossActivatedEvent evt)
    {
        ChangeState(GameState.BossBattle);
    }

    private void OnBossDied(BossDiedEvent evt)
    {
        ChangeState(GameState.BossDefeated);

#if UNITY_WEBGL
        try
        {
            var characterName = Player.character.playerName;
            const string methodName = "window._unityArcadeGameWon";

            SentrySdk.AddBreadcrumb("Submitting arcade win", type: "ArcadeWin", data: new Dictionary<string, string>
            {
                { "userAddress", PersistentSettings.Instance.UserAddress },
                { "characterName", characterName },
            });

            var s = new Signore().SArcade(_defeatedEnemies);
            Application.ExternalCall(methodName, s, characterName, _defeatedEnemies);
        }
        catch (Exception ex)
        {
            Logger.LogError("Failed to sign arcade win");
            SentrySdk.CaptureException(ex);
        }
#endif
    }

    private void InstantiatePlayer()
    {
        var playerIndex = 0;

        var playerPrefab = playerPrefabs[playerIndex];

        var playerPosition = new Vector2(1, 1);
        var spawn = GameObject.Find("PlayerSpawn");
        if (spawn != null)
        {
            playerPosition = spawn.transform.position;
        }

        Instantiate(playerPrefab, playerPosition, Quaternion.identity);

        Logger.Log("Player instantiated");
    }

    private void CheckIsInBattle()
    {
        if (State == GameState.BossBattle ||
            State == GameState.BossDefeated)
        {
            // Skip check
            return;
        }

        bool enemyNear = false;

        var playerBoundsSize = 10.0f;

        var checkBounds = new Bounds(
            Player.transform.position,
            new Vector3(playerBoundsSize, playerBoundsSize, 0)
        );

        var enemiesCount = Util.CountObjectsInsideBounds("Enemy", checkBounds);
        if (spawnersTriggerBattle)
        {
            enemiesCount += Util.CountObjectsInsideBounds("Spawner", checkBounds);
        }

        if (enemiesCount > 0)
        {
            enemyNear = true;
        }
        else
        {
            playerBoundsSize = 2.0f;

            checkBounds = new Bounds(
                Player.transform.position,
                new Vector3(playerBoundsSize, playerBoundsSize, 0)
            );

            var enemyAreas = GameObject.Find("EnemyAreas");

            for (int i = 0; i < enemyAreas.transform.childCount; i++)
            {
                var enemyArea = enemyAreas.transform.GetChild(i).gameObject;
                if (!enemyArea.activeInHierarchy)
                {
                    continue;
                }

                var areaBounds = Util.GetAreaBounds(enemyArea);
                if (checkBounds.Intersects(areaBounds))
                {
                    enemiesCount = Util.CountObjectsInsideArea("Enemy", enemyArea);
                    if (enemiesCount > 0)
                    {
                        enemyNear = true;
                        break;
                    }

                    if (spawnersTriggerBattle)
                    {
                        enemiesCount = Util.CountObjectsInsideArea("Spawner", enemyArea);
                        if (enemiesCount > 0)
                        {
                            enemyNear = true;
                            break;
                        }
                    }
                }
            }
        }

        EnemyNear = enemyNear;

        if (enemyNear && State == GameState.Idle)
        {
            ChangeState(GameState.Battle);
        }
        else if (!enemyNear && State == GameState.Battle)
        {
            ChangeState(GameState.Idle);
        }
    }

    private string GetPlayerEnemyArea()
    {
        var enemyAreas = GameObject.Find("EnemyAreas");

        for (var i = 0; i < enemyAreas.transform.childCount; i++)
        {
            var enemyArea = enemyAreas.transform.GetChild(i).gameObject;
            if (!enemyArea.activeInHierarchy)
            {
                continue;
            }

            var areaBounds = Util.GetAreaBounds(enemyArea);
            if (areaBounds.Contains(Player.transform.position))
            {
                return enemyArea.name;
            }
        }

        return null;
    }
}