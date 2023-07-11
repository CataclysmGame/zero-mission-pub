using UnityEngine;

public class TutorialPanel : MonoBehaviour
{
    private UIControls _uiControls;

    public void Awake()
    {
        Time.timeScale = 0.0f;
    }

    public void OnDestroy()
    {
        Time.timeScale = 1.0f;
    }

    private void OnEnable()
    {
        _uiControls = new UIControls();
        _uiControls.Enable();
        _uiControls.UI.Cancel.performed += (_) => OnOk();
    }

    private void OnDisable()
    {
        _uiControls.Disable();
    }

    public void OnOk()
    {
        Destroy(gameObject);
    }
}