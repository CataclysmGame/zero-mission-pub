using DG.Tweening;
using UnityEngine;

public class ShopNowButton : MonoBehaviour
{
    public float upDuration = 1.0f;

    public float downDuration = 1.0f;

    public float upScale = 1.3f;

    private Vector3 _originalScale = Vector3.one;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    public void ScaleUp()
    {
        transform.DOScale(new Vector3(upScale, upScale, 1.0f), upDuration);
    }

    public void ScaleDown()
    {
        transform.DOScale(_originalScale, downDuration);
    }
}