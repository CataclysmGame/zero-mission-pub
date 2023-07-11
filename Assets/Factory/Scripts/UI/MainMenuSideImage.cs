using DG.Tweening;
using UnityEngine;

public class MainMenuSideImage : MonoBehaviour
{
    public float actionPeriod = 2.0f;
    public float actionThreshold = 0.4f;

    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = transform.localScale;
        
        InvokeRepeating(nameof(DoAction), actionPeriod, actionPeriod);
    }

    private void DoScale()
    {
        DOTween.Sequence()
            .Append(transform.DOScale(_originalScale * 1.05f, 1.5f).SetAs(new TweenParams().SetEase(Ease.InOutBack)))
            .Append(transform.DOScale(_originalScale, 1.5f).SetAs(new TweenParams().SetEase(Ease.InOutBack)))
            .SetAs(new TweenParams().SetLoops(0));
    }

    private void DoAction()
    {
        var randomValue = Random.value;
        if (randomValue < actionThreshold)
        {
            DoScale();
        }
    }
}
