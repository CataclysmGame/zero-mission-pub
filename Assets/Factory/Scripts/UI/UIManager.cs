using System.Collections.Generic;
using Factory.Scripts.Events;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; } = null;

    public Joystick joystick;

    public AudioMixer audioMixer;

    public GameObject enemyArea0;

    public Image avatarImage;
    public Texture2D defaultAvatarTexture;

    public TMP_Text characterNameText;
    public Color deadCharacterNameColor = Color.red;

    public TMP_Text atkDmg;
    public TMP_Text reloadSpd;
    public TMP_Text atkSpd;
    public TMP_Text lifeSteal;
    public TMP_Text mvmSpd;

    public Image settingsCanvas;

    public Button settingsButton;

    public Button fireButton;
    public Button reloadButton;

    public int avatarPortraitX = 250;
    public int avatarPortraitY = 330;
    public int avatarPortraitSize = 700;

    private UIControls uiControls;

    public Image gameOverPanel;

    public LoadingPanel loadingPanel;

    public Button mainMenu;
    public Button retry;

    public AudioSource gameOverSource;
    public AudioClip gameOverClip;

    public Image wonPanel;
    public Button wonMainMenu;

    public Image npcImage;
    public Image playerImage;

    public Sprite[] npcSprites;
    public Texture2D defaultPlayerAtlas;

    public AudioClip wonClip;

    private int npcAnimIndex = 0;
    private float npcTimer = 0;

    private int playerAnimIndex = 0;
    private float playerTimer = 0;

    //private System.Threading.CancellationTokenSource introTextCts;

#if UNITY_ANDROID || UNITY_IOS
    [HideInInspector]
    public Joystick joystickInstance;
    [HideInInspector]
    public Button fireButtonInstance;
    [HideInInspector]
    public Button reloadButtonInstance;
#endif

    private Sprite[] playerSprites;

    private bool gameOver = false;
    private bool gameWon = false;

    private bool IsEndless => GameObject.Find("Endless") != null;

    private void Awake()
    {
        Instance = this;
#if UNITY_ANDROID || UNITY_IOS
        joystickInstance = Instantiate(joystick, transform);
        fireButtonInstance = Instantiate(fireButton, transform);
        reloadButtonInstance = Instantiate(reloadButton, transform);
#endif

        uiControls = new UIControls();
        uiControls.UI.VolumeUp.performed += (ctx) => IncreaseVolume();
        uiControls.UI.VolumeDown.performed += (ctx) => DecreaseVolume();
        uiControls.UI.OpenSettings.performed += (ctx) => OpenSettings();

        mainMenu.onClick.AddListener(ReturnToMainMenu);
        wonMainMenu.onClick.AddListener(ReturnToMainMenu);
        retry.onClick.AddListener(Restart);

        playerSprites = CreatePlayerSprites().ToArray();
    }

    private List<Sprite> CreatePlayerSprites()
    {
        Texture2D texture;

        if (SceneParameters.Instance.ContainsParameter(SceneParameters.PARAM_PLAYER_ATLAS))
        {
            texture = SceneParameters.Instance.GetParameter<Texture2D>(SceneParameters.PARAM_PLAYER_ATLAS);
        }
        else
        {
            texture = defaultPlayerAtlas;
        }

        var animations = new List<Sprite>();
        const int y = 2;
        const int framesCount = 4;
        const int tileSize = 32;

        for (var x = 0; x < framesCount; x++)
        {
            animations.Add(Sprite.Create(
                texture,
                new Rect(x * tileSize, y * tileSize, tileSize, tileSize),
                new Vector2(0.5f, 0.5f)
            ));
        }

        return animations;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1.0f;

        mainMenu.onClick.RemoveAllListeners();
        wonMainMenu.onClick.RemoveAllListeners();
        retry.onClick.RemoveAllListeners();

        EventsManager.Unsubscribe<GameStateChangedEvent>(GameStateChanged);
        EventsManager.Unsubscribe<PlayerDiedEvent>(PlayGameOver);
        EventsManager.Unsubscribe<PlayerStatsChanged>(OnPlayerStatsChanged);
        EventsManager.Unsubscribe<PlayerCharacterLoadedEvent>(OnPlayerCharacterLoaded);
    }

    private void OnEnable()
    {
        uiControls.Enable();
    }

    private void OnDisable()
    {
        uiControls.Disable();
    }

    private void Restart()
    {
        if (IsEndless && PersistentSettings.Instance.WaitExternalStart)
        {
#if UNITY_WEBGL
            Application.ExternalCall("window._unityEndlessStarted", true);
#endif
            loadingPanel.gameObject.SetActive(true);
            loadingPanel.SetAnimTexture(
                GameManager.Instance.Player.GetComponent<SpriteRenderer>().sprite.texture
            );
            return;
        }

        var currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void Start()
    {
        EventsManager.Subscribe<PlayerCharacterLoadedEvent>(OnPlayerCharacterLoaded);
        EventsManager.Subscribe<PlayerStatsChanged>(OnPlayerStatsChanged);
        EventsManager.Subscribe<PlayerDiedEvent>(PlayGameOver);
        EventsManager.Subscribe<GameStateChangedEvent>(GameStateChanged);
        OnPlayerStatsChanged();

        Texture2D avatarTexture;

        if (SceneParameters.Instance.ContainsParameter(SceneParameters.PARAM_PLAYER_AVATAR))
        {
            avatarTexture = SceneParameters.Instance.GetParameter<Texture2D>(SceneParameters.PARAM_PLAYER_AVATAR);
        }
        else
        {
            avatarTexture = defaultAvatarTexture;
        }

        if (SceneParameters.Instance.ContainsParameter(SceneParameters.PARAM_PLAYER_SKIN))
        {
            var skin = SceneParameters.Instance.GetParameter<Skin>(SceneParameters.PARAM_PLAYER_SKIN);
            var avatar = skin.inGameAvatar ? skin.inGameAvatar : skin.avatar;
            if (avatar != null)
            {
                avatarTexture = avatar;
            }
        }

        //var useNfts = PersistentSettings.Instance.UseNfts;
        //var avatarPortrait = useNfts ? CutAvatarPortrait(avatarTexture) : avatarTexture;

        var avatarSprite = Sprite.Create(
            avatarTexture,
            new Rect(0, 0, avatarTexture.width, avatarTexture.height),
            new Vector2(0.5f, 0.5f)
        );
        avatarImage.sprite = avatarSprite;

        settingsButton.onClick.AddListener(OpenSettings);
#if UNITY_ANDROID || UNITY_IOS
        fireButtonInstance.onClick.AddListener(Shoot);
        reloadButtonInstance.onClick.AddListener(Reload);
#endif
    }

    private void PlayGameOver(PlayerDiedEvent evt)
    {
        characterNameText.color = deadCharacterNameColor;
        gameOver = true;
        gameOverPanel.gameObject.SetActive(true);
        gameOverSource.PlayOneShot(gameOverClip);
    }

    private void Reload()
    {
        var player = GameManager.Instance.Player;

        player.TryReload(true);
    }

    private void Shoot()
    {
        var player = GameManager.Instance.Player;

        player.TryShoot();
    }

    private void OpenSettings()
    {
        if (!settingsCanvas.gameObject.activeInHierarchy)
        {
            Time.timeScale = 0;
            settingsCanvas.gameObject.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
            settingsCanvas.gameObject.SetActive(false);
        }
    }

    private void Animate()
    {
        if ((npcTimer += Time.deltaTime) >= (0.6f / npcSprites.Length))
        {
            npcTimer = 0;
            npcImage.sprite = npcSprites[npcAnimIndex];
            npcAnimIndex = (npcAnimIndex + 1) % npcSprites.Length;
        }

        if ((playerTimer += Time.deltaTime) >= (0.6f / playerSprites.Length))
        {
            playerTimer = 0;
            playerImage.sprite = playerSprites[playerAnimIndex];
            playerAnimIndex = (playerAnimIndex + 1) % playerSprites.Length;
        }
    }

    private void Update()
    {
        var gameManager = GameManager.Instance;
        var gameOver = gameManager.gameOver;

        //if (gameWon)
        {
            Animate();
        }

#if UNITY_ANDROID || UNITY_IOS
        if (joystickInstance != null)
        {
            joystickInstance.gameObject.SetActive(!gameOver);
        }

        if (fireButtonInstance != null)
        {
            fireButtonInstance.gameObject.SetActive(!gameOver);
        }

        if (reloadButtonInstance != = null)
        {
            reloadButtonInstance.gameObject.SetActive(!gameOver);
        }
#endif
    }

    private void IncreaseVolume()
    {
        Util.ChangeVolume(audioMixer, 5);
        Logger.Log("Volume increased");
    }

    private void DecreaseVolume()
    {
        Util.ChangeVolume(audioMixer, -5);
        Logger.Log("Volume decreased");
    }

    private Texture2D CutAvatarPortrait(Texture2D avatarImage)
    {
        var portraitRect = new RectInt(
            avatarPortraitX,
            avatarPortraitY,
            avatarPortraitSize,
            avatarPortraitSize
        );

        var avatarPortrait = new Texture2D(
            portraitRect.width,
            portraitRect.height,
            TextureFormat.ARGB32,
            false
        );
        for (int x = 0; x < portraitRect.width; x++)
        {
            for (int y = 0; y < portraitRect.height; y++)
            {
                int ix = portraitRect.x + x;
                int iy = portraitRect.y + y;
                if (ix < avatarImage.width && iy < avatarImage.height)
                {
                    var color = avatarImage.GetPixel(ix, iy);
                    avatarPortrait.SetPixel(x, y, color);
                }
            }
        }

        avatarPortrait.Apply();

        return avatarPortrait;
    }

    private void OnPlayerCharacterLoaded(PlayerCharacterLoadedEvent evt)
    {
        characterNameText.text = evt.Character.playerName;
    }

    private void OnPlayerStatsChanged(PlayerStatsChanged evt)
    {
        OnPlayerStatsChanged();
    }

    private void ShowVictoryPanel()
    {
        if (gameWon && !gameOver)
        {
            wonPanel.gameObject.SetActive(true);
        }
    }

    private void GameStateChanged(GameStateChangedEvent evt)
    {
        if (evt.State == GameState.BossDefeated)
        {
            gameWon = true;
            Invoke(nameof(ShowVictoryPanel), 1.0f);
        }
    }

    private void OnPlayerStatsChanged()
    {
        var player = GameManager.Instance.Player;

        Logger.Log($"Player stats: (Atk spd.: 1/{player.shootInterval}");

        atkDmg.text = player.projectilePower.ToString();
        atkSpd.text = player.shootInterval.ToString("F1");
        reloadSpd.text = player.reloadInterval.ToString("F1");
        if (lifeSteal != null)
        {
            lifeSteal.text = player.lifeSteal.ToString("F2");
        }

        mvmSpd.text = ((int)(player.movementSpeed * 10)).ToString();
    }

    public void OpenDiscord()
    {
        Application.OpenURL("https://discord.gg/ZeJH2sBqAd");
    }
}