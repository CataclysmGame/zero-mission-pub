using System;
using System.Collections.Generic;
using Sentry;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class EndlessController : MonoBehaviour
{
    public TMP_Text durationText;
    public TMP_Text enemiesDefeatedText;

    public Tilemap floorTilemap;
    public Tile[] randomTiles;
    public int randomTilesCount = 120;

    public static EndlessController Instance { get; private set; } = null;

    public float ElapsedTime => _elapsedTime;

    public Transform[] randomMapTargets;

    private float _elapsedTime = 0.0f;
    private ulong _enemiesDefeated = 0;
    public ulong EnemiesDefeated => _enemiesDefeated;
    private float _textUpdateTimer = 0.0f;

    [SerializeField]
    private EndlessArena _endlessArena;
    public EndlessArena EndlessArena => _endlessArena;

    private float _startingTime = 0f;

    public void Restart()
    {
        var currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    private void Awake()
    {
        Instance = this;
        durationText.text = "0:00:00";
        enemiesDefeatedText.text = "0";
    }

    private void Start()
    {
        _startingTime = Time.time;

        EventsManager.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        EventsManager.Subscribe<EnemyDiedEvent>(OnEnemyDied);

        var floorSize = floorTilemap.size;
        var radius = Mathf.Min(floorSize.x, floorSize.y);
        for (var i = 0; i < randomTilesCount; ++i)
        {
            var point = Random.insideUnitCircle * radius;
            var tile = Util.PickRandom(randomTiles);
            floorTilemap.SetTile(new Vector3Int((int)point.x, (int)point.y, 0), tile);
        }
    }

    private void OnDestroy()
    {
        EventsManager.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
        EventsManager.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
    }

    private void OnPlayerDied(PlayerDiedEvent evt)
    {
        enabled = false;

        // Submit score
#if UNITY_WEBGL
        try
        {
            var score = Mathf.FloorToInt(_elapsedTime);
            var characterName = GameManager.Instance.Player.character.playerName;

            var signore = new Signore();

            var skin = SceneParameters.Instance.GetParameter<Skin>(SceneParameters.PARAM_PLAYER_SKIN, () => null);
            var skinId = skin == null ? "base" : skin.id;

            SentrySdk.AddBreadcrumb("Submitting endless score", type: "EndlessScore",
                data: new Dictionary<string, string>
                {
                    { "userAddress", PersistentSettings.Instance.UserAddress },
                    { "characterName", characterName },
                    { "skinId", skinId },
                    { "score", score.ToString() },
                    { "enemiesDefeated", _enemiesDefeated.ToString() },
                    { "elapsedTime", _elapsedTime.ToString("0.00") + "s" },
                });

            var scoreSig = signore.SEndless(score, characterName, skinId);
            var endSig = signore.SGameOver(1, score);

            Application.ExternalCall(
                "window._unitySubmitEndlessScore",
                score,
                _enemiesDefeated,
                characterName,
                skinId,
                scoreSig,
                endSig
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
        ++_enemiesDefeated;
        enemiesDefeatedText.text = _enemiesDefeated.ToString();
    }

    private void Update()
    {
        _textUpdateTimer += Time.deltaTime;

        if (_textUpdateTimer > 0.5f)
        {
            _textUpdateTimer = 0.0f;
            UpdateElapsedTime();
            var ts = TimeSpan.FromSeconds(_elapsedTime);
            durationText.text = ts.ToString("h\\:mm\\:ss");
        }
    }

    private void UpdateElapsedTime()
    {
        if (_endlessArena.IsPlayerOutsideArea)
        {
            return;
        }

        _elapsedTime = Time.time - _startingTime;
    }
}