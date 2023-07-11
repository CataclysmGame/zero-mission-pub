using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanel : MonoBehaviour
{
    public TMP_Text loadingText;
    public Image animationImage;

    public Texture2D playerTexture;

    public float textDotsInterval = 1.0f;
    public float animationInterval = 0.32f;

    private int _textDots;
    private int _curAnimationFrame;

    private List<Sprite> _animation;

    public void SetAnimTexture(Texture2D animTexture)
    {
        _curAnimationFrame = 0;
        _animation = CreateAnimationSprites(animTexture);
    }

    private void Awake()
    {
        _animation = CreateAnimationSprites(playerTexture);
    }

    private void Start()
    {
        InvokeRepeating(nameof(UpdateText), 0, textDotsInterval);
        InvokeRepeating(nameof(UpdateAnimation), 0, animationInterval);
    }

    private void UpdateText()
    {
        loadingText.text = "LOADING" + new string('.', _textDots);
        _textDots = (_textDots + 1) % 4;
    }

    private void UpdateAnimation()
    {
        animationImage.sprite = _animation[_curAnimationFrame];
        _curAnimationFrame = (_curAnimationFrame + 1) % _animation.Count;
    }

    private List<Sprite> CreateAnimationSprites(Texture2D texture, int tileSize = 32)
    {
        var animations = new List<Sprite>();
        const int y = 2;
        const int framesCount = 4;

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
}