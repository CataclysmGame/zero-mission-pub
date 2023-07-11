using UnityEngine;
using UnityEngine.Video;

public class TrailerBanner : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public SpriteRenderer videoSpriteRenderer;
    public RenderTexture videoRenderTexture;

    [Tooltip("This is the URL to the video file that will be played on WebGL builds")]
    public string webGlVideoUrl;

    private Texture2D _spriteTexture;

    private bool _visible;

    public void Break()
    {
        videoPlayer.Stop();
        enabled = false;
    }

    private void Awake()
    {
        _spriteTexture = new Texture2D(videoRenderTexture.width, videoRenderTexture.height);

#if UNITY_WEBGL
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = webGlVideoUrl;
#endif

        videoSpriteRenderer.sprite = Sprite.Create(_spriteTexture,
            new Rect(0, 0, videoRenderTexture.width, videoRenderTexture.height), Vector2.zero);
    }

    private void OnBecameVisible()
    {
        _visible = true;

        if (!videoPlayer.isPlaying)
        {
            videoPlayer.Play();
        }
    }

    private void OnBecameInvisible()
    {
        _visible = false;
    }

    private void Update()
    {
        if (!_visible)
        {
            return;
        }

        UpdateTexture();
    }

    private void UpdateTexture()
    {
        RenderTexture.active = videoRenderTexture;
        _spriteTexture.ReadPixels(new Rect(0, 0, videoRenderTexture.width, videoRenderTexture.height), 0, 0);
        _spriteTexture.Apply();
    }
}