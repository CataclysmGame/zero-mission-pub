using DG.Tweening;
using UnityEngine;

public class ChestController : MonoBehaviour
{
    public GameObject drop;

    public AudioSource audioSource;
    public AudioClip chestOpenedClip;
    public AudioClip dropFallenClip;

    private Animator animator;

    private static readonly int AnimTriggerOpen = Animator.StringToHash("open");

    private Canvas _helpCanvas;

    void Awake()
    {
        animator = GetComponent<Animator>();
        _helpCanvas = GetComponentInChildren<Canvas>();
    }

    public virtual void Open()
    {
        animator.SetTrigger(AnimTriggerOpen);
        if (audioSource != null && chestOpenedClip != null)
        {
            audioSource.PlayOneShot(chestOpenedClip);
            if (_helpCanvas != null)
            {
                Destroy(_helpCanvas.gameObject);
                _helpCanvas = null;
            }
        }
    }

    public virtual void InstantiateDrop()
    {
        var startPos = transform.position - Vector3.forward;

        var dropInst = Instantiate(drop,
            startPos,
            Quaternion.identity
        );

        var tweenParams = new TweenParams().SetEase(Ease.InSine);
        var duration = 0.6f;

        var dropComponent = dropInst.GetComponent<Drop>();
        dropComponent.CanBePickedUp = false;

        dropInst.transform
            .DOMoveY(transform.position.y - 1, duration)
            .SetAs(tweenParams)
            .OnComplete(() =>
            {
                if (audioSource != null && dropFallenClip != null)
                {
                    audioSource.PlayOneShot(dropFallenClip);
                }
                dropComponent.CanBePickedUp = true;
            });
    }
}
