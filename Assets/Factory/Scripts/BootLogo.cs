using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class BootLogo : MonoBehaviour
{
    public string sceneToLoad;
    public Image fadePanel;

    private void GoToNextScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    private async UniTask FadeInOut()
    {
        for (float i = 1f; i > 0f; i -= Time.deltaTime)
        {
            fadePanel.color = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, i);
            await UniTask.Yield();
        }

        await UniTask.Delay(3000);

        for (float i = 0f; i < 1f; i += Time.deltaTime)
        {
            fadePanel.color = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, i);
            await UniTask.Yield();
        }

        GoToNextScene();
    }
    
    void Start()
    {
        FadeInOut();
    }
}
