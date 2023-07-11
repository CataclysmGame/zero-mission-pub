using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class SettingPanel : MonoBehaviour
{
    public AudioMixer audioMixer;

    private Resolution[] screenResolutions;

    public Dropdown resolutionDropdown;
    public Dropdown graphicsQualityDropdown;
    public Toggle fullscreenToggle;
    public Toggle crtFilterToggle;

    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider soundVolumeSlider;

    public Button mainMenu;
    //public Button exit;

    public Image settingsCanvas;
    public Button backButton;

    private void Start()
    {
        screenResolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        var options = screenResolutions
            .Select((r) => $"{r.width}x{r.height}")
            .ToList();
        resolutionDropdown.AddOptions(options);

        fullscreenToggle.isOn = Screen.fullScreen;
        crtFilterToggle.isOn = Settings.Instance.CrtFilterEnabled;
        graphicsQualityDropdown.value = QualitySettings.GetQualityLevel();

        mainMenu.onClick.AddListener(ReturnToMainMenu);
// #if UNITY_WEBGL
//         exit.gameObject.SetActive(false);
// #else
//         exit.onClick.AddListener(Exit);
// #endif
        backButton.onClick.AddListener(ClosePanel);

        masterVolumeSlider.value = Util.GetLinearVolume(audioMixer, "masterVolume");
        musicVolumeSlider.value = Util.GetLinearVolume(audioMixer, "musicVolume");
        soundVolumeSlider.value = Util.GetLinearVolume(audioMixer, "soundVolume");

        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        soundVolumeSlider.onValueChanged.AddListener(SetSoundVolume);
    }

    private void OnEnable()
    {
        backButton.Select();
    }

    public void ClosePanel()
    {
        settingsCanvas.gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    private float GetVolume(string name)
    {
        float volume = 0.0f;
        audioMixer.GetFloat(name, out volume);
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


    public void SetCrtFilterEnabled(bool crtFilterEnabled)
    {
        Settings.Instance.CrtFilterEnabled = crtFilterEnabled;
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

    public void Exit()
    {
        Application.Quit();
    }
}