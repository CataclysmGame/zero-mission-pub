using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    private Resolution[] screenResolutions;

    public Dropdown resolutionDropdown;
    public Dropdown graphicsQualityDropdown;
    public Toggle fullscreenToggle;
    public Toggle testNetToggle;
    public Toggle crtFilterToggle;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider soundVolumeSlider;

    private void Start()
    {
        screenResolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        var options = screenResolutions
            .Select((r) => $"{r.width}x{r.height}")
            .ToList();
        resolutionDropdown.AddOptions(options);

        fullscreenToggle.isOn = Screen.fullScreen;
        testNetToggle.isOn = Web3Manager.Instance.UseTestnet;

        //if (!PersistentSettings.Instance.UseNfts)
        //{
            testNetToggle.gameObject.SetActive(false);
        //}

        crtFilterToggle.isOn = Settings.Instance.CrtFilterEnabled;
        graphicsQualityDropdown.value = QualitySettings.GetQualityLevel();

        masterVolumeSlider.value = Util.GetLinearVolume(audioMixer, "masterVolume");
        musicVolumeSlider.value = Util.GetLinearVolume(audioMixer, "musicVolume");
        soundVolumeSlider.value = Util.GetLinearVolume(audioMixer, "soundVolume");

        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        soundVolumeSlider.onValueChanged.AddListener(SetSoundVolume);

        fullscreenToggle.Select();
    }

    public float GetVolume(string param)
    {
        float volume = 0.0f;
        audioMixer.GetFloat("masterVolume", out volume);
        return volume;
    }

    public void SetMasterVolume(float volume)
    {
        Util.SetLinearVolume(audioMixer, "masterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        Util.SetLinearVolume(audioMixer, "musicVolume", volume);
    }

    public void SetSoundVolume(float volume)
    {
        Util.SetLinearVolume(audioMixer, "soundVolume", volume);
    }

    public void SetGraphicsQuality(int quality)
    {
        QualitySettings.SetQualityLevel(quality);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetUseTestnet(bool useTestnet)
    {
        Web3Manager.Instance.UseTestnet = useTestnet;
    }

    public void SetCrtFilterEnabled(bool crtFilterEnabled)
    {
        Settings.Instance.CrtFilterEnabled = crtFilterEnabled;
    }

    public void SetMouseVisible(bool visible)
    {
        Cursor.visible = visible;
    }

    public void SetResolution(int resolutionIndex)
    {
        var resolution = screenResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, fullscreenToggle.isOn);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
