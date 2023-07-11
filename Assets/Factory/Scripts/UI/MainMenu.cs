using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject persistentPlayerPrefab;

    public AudioMixer audioMixer;

    public AudioClip introMusic;

    public AudioClip loopMusic1;
    public AudioClip loopMusic2;

    public AudioClip newGameClip;
    public AudioClip selectClip;
    public AudioClip submitClip;

    public Image logoImage;

    public Button exitButton;

    private UIControls uiControls;

    public AudioSource musicAudioSource;
    public AudioSource soundAudioSource;

    private System.Threading.CancellationTokenSource cts;

    private bool firstSelectFired = false;

    private void Awake()
    {
        uiControls = new UIControls();

        uiControls.UI.VolumeDown.performed += (ctx) => DecreaseVolume();
        uiControls.UI.VolumeUp.performed += (ctx) => IncreaseVolume();

#if UNITY_WEBGL
        exitButton.gameObject.SetActive(false);
#endif
    }

    private void OnEnable()
    {
        uiControls.Enable();
    }

    private void OnDisable()
    {
        uiControls.Disable();
    }

    private void OnDestroy()
    {
        //cts.Cancel();
        PersistentMusicPlayer.Instance?.Stop();
    }

    private void Update()
    {
        /*
        if (!musicAudioSource.isPlaying)
        {
            if (musicAudioSource.clip == loopMusic1 ||
                musicAudioSource.clip == loopMusic2)
            {
                musicAudioSource.clip = Util.RandomBool() ? loopMusic1 : loopMusic2;
                musicAudioSource.Play();
            }
        }
        */
    }

    /*
    private async UniTaskVoid StartPlayingMusic(System.Threading.CancellationToken ct)
    {
        musicAudioSource.Stop();

        await UniTask.WaitWhile(
            () => PersistentMusicPlayer.Instance.MusicPlaying,
            cancellationToken: ct
        );

        if (musicAudioSource != null && musicAudioSource.isActiveAndEnabled)
        {
            musicAudioSource.clip = loopMusic1;
            musicAudioSource.Play();
        }
    }
    */

    private void Start()
    {
        if (PersistentMusicPlayer.Instance == null)
        {
            Instantiate(persistentPlayerPrefab);
        }

        //cts = new System.Threading.CancellationTokenSource();
        //StartPlayingMusic(cts.Token);

        PersistentMusicPlayer.Instance.EnqueMusic(
            new AudioClip[] { loopMusic1, loopMusic2 },
            true
        );

        DOTween.Sequence()
            .Append(logoImage.transform.DOScale(0.85f, 1.0f).SetAs(new TweenParams().SetEase(Ease.InBounce)))
            .Append(logoImage.transform.DOScale(1.0f, 1.0f).SetAs(new TweenParams().SetEase(Ease.InElastic)))
            .SetAs(new TweenParams().SetLoops(-1));
    }

    public void NewGame()
    {
        PersistentMusicPlayer.Instance.PlaySoundOneShot(newGameClip);

        SceneManager.LoadScene("SelectCharacter");
        
        /*
        if (PersistentSettings.Instance.UseNfts)
        {
            SceneManager.LoadScene("ConnectWalletScene");
        }
        else
        {
            SceneManager.LoadScene("SelectCharacter");
        }*/
    }

    public void GotoSettings()
    {
        SceneManager.LoadScene("SettingsMenu");
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    private void IncreaseVolume()
    {
        Util.ChangeVolume(audioMixer, 5);
    }

    private void DecreaseVolume()
    {
        Util.ChangeVolume(audioMixer, -5);
    }

    // UI elements events
    public void OnSelect(BaseEventData evt)
    {
        if (!firstSelectFired)
        {
            firstSelectFired = true;
            return;
        }

        soundAudioSource.PlayOneShot(selectClip);
    }

    public void OnNewGame(BaseEventData evt)
    {
        PersistentMusicPlayer.Instance.PlaySoundOneShot(newGameClip);
    }

    public void OnSubmit(BaseEventData evt)
    {
        PersistentMusicPlayer.Instance.PlaySoundOneShot(submitClip);
    }
}