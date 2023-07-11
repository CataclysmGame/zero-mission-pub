using DG.Tweening;
using UnityEngine;

public class HelpCanvas : MonoBehaviour
{
    public float moveAmount = 0.2f;
    public float duration  = 1.0f;

    private void Start()
    {
        transform.DOLocalMoveY(transform.localPosition.y - moveAmount, duration)
            .SetAs(new TweenParams().SetEase(Ease.Linear)).SetLoops(-1, LoopType.Yoyo);
    }
}