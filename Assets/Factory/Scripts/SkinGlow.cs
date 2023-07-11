using DG.Tweening;
using UnityEngine;

public class SkinGlow : MonoBehaviour
{
    public float animStartScale = 1.0f;
    public float animEndScale = 1.2f;

    public float animStartAlpha = 0.0f;
    public float animEndAlpha = 1.0f;

    public float animDuration = 0.5f;

    public Transform runTransform;

    private SpriteRenderer _spriteRenderer;

    private Vector3 _idleTransform;
    private Vector3 _runTransform;
    private PlayerController _player;

    private static readonly int AnimPropIsMoving = Animator.StringToHash("isMoving");

    private Sprite _glowNormalSprite;
    private Sprite _glowRunSprite;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _idleTransform = transform.localPosition;
        _player = GetComponentInParent<PlayerController>();
        _runTransform = runTransform.localPosition;
    }

    private void Update()
    {
        if (_player.animator.GetBool(AnimPropIsMoving))
        {
            _spriteRenderer.sprite = _glowRunSprite != null ? _glowRunSprite : _glowNormalSprite;
            transform.localPosition = _runTransform;
        }
        else
        {
            _spriteRenderer.sprite = _glowNormalSprite;
            transform.localPosition = _idleTransform;
        }
    }

    public void SetSkin(Skin skin)
    {
        if (skin.glow == null)
        {
            return;
        }

        _glowNormalSprite = Sprite.Create(
            skin.glow,
            new Rect(0, 0, skin.glow.width, skin.glow.height),
            new Vector2(0.5f, 0.5f),
            16
        );

        if (skin.runGlow != null)
        {
            _glowRunSprite = Sprite.Create(
                skin.runGlow,
                new Rect(0, 0, skin.runGlow.width, skin.runGlow.height),
                new Vector2(0.5f, 0.5f),
                16
            );
        }

        _spriteRenderer.DOKill();
        DOTween.Sequence()
            .Append(_spriteRenderer.DOFade(animStartAlpha, animDuration))
            .Append(_spriteRenderer.DOFade(animEndAlpha, animDuration))
            .SetLoops(-1);

        transform.DOKill();
        DOTween.Sequence()
            .Append(transform.DOScale(animStartScale, animDuration))
            .Append(transform.DOScale(animEndScale, animDuration))
            .SetLoops(-1);
    }
}