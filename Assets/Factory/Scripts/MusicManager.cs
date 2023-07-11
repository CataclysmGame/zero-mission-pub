using Cysharp.Threading.Tasks;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private LoggerInstance log = new LoggerInstance("MusicManager");

    enum MusicState
    {
        Idle,
        BattleIntro,
        Battle,
        BossIntro,
        BossBattle,
        BossDefeated,
        Muted,
        GameOver,
    }

    public MusicAlternativePickType ambienceMusicPickType;

    public float battleTimeoutSeconds = 15.0f;

    public float fadeDuration = 1.0f;

    public GameObject noMusicArea;

    public AudioSource idleMusicSource;
    public AudioSource battleMusicSource;
    public AudioSource ambienceSource;

    public AudioClip idleMusicLoop1;
    public AudioClip idleMusicLoop2;
    public AudioClip battleMusicIntro;
    public AudioClip battleMusic;
    public AudioClip bossTransition;
    public AudioClip bossIntro;
    public AudioClip bossMusicLoop1;
    public AudioClip bossMusicLoop2;
    public AudioClip bossDefeatedMusic;

    public AudioClip ambience1;
    public AudioClip ambience2;

    private float maxIdleMusicVol = 1.0f;
    private float maxBattleMusicVol = 1.0f;

    private bool bossIntroPlayed = false;
    private bool battleIntroPlayed = false;

    private bool appPaused = false;

    private MusicState currentState = MusicState.Idle;

    private System.Threading.CancellationTokenSource fadeCts = null;
    
    public static MusicManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        maxIdleMusicVol = idleMusicSource.volume;
        maxBattleMusicVol = battleMusicSource.volume;
    }

    private void Start()
    {
        idleMusicSource.volume = maxIdleMusicVol;
        idleMusicSource.loop = true;
        idleMusicSource.clip = idleMusicLoop1;
        idleMusicSource.Play();

        battleMusicSource.loop = false;
        battleMusicSource.volume = maxBattleMusicVol;
        battleMusicSource.clip = battleMusicIntro;
        //battleMusicSource.Play();

        ambienceSource.loop = false;
        ambienceSource.clip = ambience1;
        ambienceSource.Play();

        EventsManager.Subscribe<GameStateChangedEvent>(GameStateChanged);
        EventsManager.Subscribe<PlayerDiedEvent>(OnPlayerDied);

        if (noMusicArea != null)
        {
            InvokeRepeating("CheckNoMusicArea", 0.3f, 0.3f);
        }
    }

    private void OnDestroy()
    {
        CancelInvoke();

        EventsManager.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        EventsManager.Unsubscribe<GameStateChangedEvent>(GameStateChanged);
    }

    private MusicState MusicStateFromGameState(GameState? state = null)
    {
        state = state ?? GameManager.Instance.State;

        switch (state)
        {
            case GameState.Battle:
                return battleIntroPlayed ? MusicState.Battle : MusicState.BattleIntro;
            case GameState.BossBattle:
                return bossIntroPlayed ? MusicState.BossBattle : MusicState.BossIntro;
            case GameState.BossDefeated:
                return MusicState.BossDefeated;
            case GameState.Idle:
                return MusicState.Idle;
            default:
                log.LogWarn($"Unhandled game state: {state}");
                return MusicState.Idle;
        }
    }

    private void ChangeState(MusicState newState, bool force = false)
    {
        if (!force && newState == currentState)
        {
            log.Log($"Duplicate state change {currentState}->{newState}");
            return;
        }

        var previousState = currentState;

        log.Log($"State changed from {previousState} to {newState}");

        currentState = newState;
        OnMusicStateChanged(previousState, newState);
    }
    
    public void GoToIdle()
    {
        log.Log("GoToIdle");
        ChangeState(MusicState.Idle);
    }

    private void OnApplicationPause(bool pause)
    {
        appPaused = pause;
    }

    private void Update()
    {
        if (appPaused)
        {
            return;
        }

        if (!battleMusicSource.isPlaying)
        {
            switch (currentState)
            {
                case MusicState.BattleIntro:
                    ChangeState(MusicState.Battle);
                    break;
                case MusicState.BossIntro:
                    ChangeState(MusicState.BossBattle);
                    break;
                case MusicState.Battle:
                    log.LogWarn($"Battle music stopped while in battle state");
                    ChangeState(MusicState.Battle, true);
                    break;
                case MusicState.BossBattle:
                    log.LogWarn($"Boss battle music stopped while in boss battle state");
                    ChangeState(MusicState.BossBattle, true);
                    break;
                default:
                    break;
            }
        }
        
        if (!idleMusicSource.isPlaying)
        {
            if (currentState == MusicState.Idle)
            {
                log.LogWarn($"Idle music stopped while in idle state");
                ChangeState(MusicState.Idle, true);
            }
        }

        if (!ambienceSource.isPlaying)
        {
            ambienceSource.clip = ambienceMusicPickType.Pick(ambience1, ambience2);
            ambienceSource.Play();
        }

        if (GameManager.Instance.EnemyNear && currentState == MusicState.Idle)
        {
            // Missed Event
            ChangeState(MusicStateFromGameState());
        }
    }

    private void OnMusicStateChanged(MusicState previousState, MusicState newState)
    {
        if (previousState != MusicState.BattleIntro)
        {

            log.Log($"Invoke canceled {previousState} -> {newState}");
            CancelInvoke("GoToIdle");
        }

        // If fading, cancel fading
        if (fadeCts != null)
        {
            fadeCts.Cancel();
            fadeCts = null;
        }

        if (previousState == MusicState.Muted)
        {
            idleMusicSource.mute = false;
            battleMusicSource.mute = false;
        }

        if (newState == MusicState.Muted)
        {
            idleMusicSource.mute = true;
            battleMusicSource.mute = true;
        }
        else if (newState == MusicState.Idle)
        {
            if (!idleMusicSource.isPlaying)
            {
                idleMusicSource.clip = idleMusicLoop1;
                idleMusicSource.loop = true;
                idleMusicSource.mute = false;
                idleMusicSource.Play();
            }

            Fade(battleMusicSource, idleMusicSource, maxIdleMusicVol);
        }
        else if (newState == MusicState.BattleIntro)
        {
            battleMusicSource.clip = battleMusicIntro;
            battleMusicSource.loop = false;
            battleMusicSource.mute = false;
            battleMusicSource.Play();

            if (previousState == MusicState.Idle)
            {
                Fade(idleMusicSource, battleMusicSource, maxBattleMusicVol);
            }
            else
            {
                battleMusicSource.volume = maxBattleMusicVol;
            }
        }
        else if (newState == MusicState.Battle)
        {
            bool play = false;

            if (!battleMusicSource.isPlaying)
            {
                battleMusicSource.clip = battleMusic;
                battleMusicSource.loop = true;
                play = true;
            }

            if (previousState == MusicState.BattleIntro)
            {
                battleMusicSource.clip = battleMusic;
                battleIntroPlayed = true;
                play = true;
            }

            if (play)
            {
                battleMusicSource.Play();
            }

            if (previousState == MusicState.Idle)
            {
                Fade(idleMusicSource, battleMusicSource, maxBattleMusicVol);
            }
            else
            {
                battleMusicSource.volume = maxBattleMusicVol;
            }
        }
        else if (newState == MusicState.BossIntro)
        {
            battleMusicSource.clip = bossIntro;
            battleMusicSource.loop = false;
            battleMusicSource.mute = false;
            battleMusicSource.Play();

            if (previousState == MusicState.Idle)
            {
                Fade(idleMusicSource, battleMusicSource, maxBattleMusicVol);
            }
            else
            {
                battleMusicSource.volume = maxBattleMusicVol;
            }
        }
        else if (newState == MusicState.BossBattle)
        {
            bool play = false;

            if (!battleMusicSource.isPlaying)
            {
                battleMusicSource.clip = bossMusicLoop1;
                battleMusicSource.loop = true;
                play = true;
            }

            if (previousState == MusicState.BossIntro)
            {
                battleMusicSource.clip = bossMusicLoop1;
                bossIntroPlayed = true;
                play = true;
            }

            if (play)
            {
                battleMusicSource.Play();
            }

            if (previousState == MusicState.Idle)
            {
                Fade(idleMusicSource, battleMusicSource, maxBattleMusicVol);
            }
            else
            {
                battleMusicSource.volume = maxBattleMusicVol;
            }
        }
        else if (newState == MusicState.BossDefeated)
        {
            // TODO: Add victory music
            //battleMusicSource.Stop();
            //idleMusicSource.clip = victoryClip;
            //idleMusicSource.volume = maxIdleMusicVol;
            //idleMusicSource.mute = false;
            //idleMusicSource.Play();
        }
        else if (newState == MusicState.GameOver)
        {
            //battleMusicSource.Stop();
            //idleMusicSource.clip = gameOverClip;
            //idleMusicSource.volume = maxIdleMusicVol;
            //idleMusicSource.mute = false;
            //idleMusicSource.Play();

            idleMusicSource.mute = true;
            battleMusicSource.mute = true;
        }
    }

    private void CheckNoMusicArea()
    {
        var player = GameManager.Instance.Player;

        if (Util.IsGameObjectInsideArea(player.gameObject, noMusicArea))
        {
            ChangeState(MusicState.Muted);
        }
        else if (currentState == MusicState.Muted)
        {
            ChangeState(MusicStateFromGameState());
        }
    }

    private void GameStateChanged(GameStateChangedEvent evt)
    {
        //log.Log($"Game state changed: {evt.PrevState} -> {evt.State}");

        if (evt.State == GameState.Idle)
        {
            log.Log($"Idle timer started. Timeout: {battleTimeoutSeconds}");
            
            Invoke("GoToIdle", battleTimeoutSeconds);
        }
        else
        {
            ChangeState(MusicStateFromGameState(evt.State));
        }
    }

    private void OnPlayerDied(PlayerDiedEvent evt)
    {
        ChangeState(MusicState.GameOver);
    }

    private async UniTask Fade(AudioSource from, AudioSource to, float targetVolume)
    {
        fadeCts = new System.Threading.CancellationTokenSource();

        var token = fadeCts.Token;

        float curTime = 0.0f;
        float fromStart = from.volume;
        float toStart = to.volume;

        while (curTime < fadeDuration)
        {
            curTime += Time.deltaTime;

            from.volume = Mathf.Lerp(fromStart, 0.0f, curTime);
            to.volume = Mathf.Lerp(toStart, targetVolume, curTime);

            await UniTask.Yield(token);
        }

        from.volume = 0.0f;
        to.volume = targetVolume;

        fadeCts = null;
    }
}
