using DG.Tweening;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class EndlessChestController : ChestController
{
    private bool _isOpening = false;

    [Serializable]
    private struct DropListEntry
    {
        public GameObject dropPrefab;
        public IncreaseFunction weight;
    }

    [Tooltip("The possible drops list for this chest. It OVERRIDES the chest component drops.")]
    [SerializeField]
    private List<DropListEntry> _drops;

    public override void Open()
    {
        if (_isOpening) return;

        _isOpening = true;

        base.Open();

        EventsManager.Publish(new EndlessChestOpenedEvent());

        StartCoroutine(DestroyChest());
    }

    public override void InstantiateDrop()
    {
        var startPos = transform.position - Vector3.forward;

        int chosenDropIndex = Util.GetRandomWeightedIndex(_drops.Select(d => d.weight.GetNewValue(GameManager.Instance.EnemiesDefeated)).ToList());

        if (chosenDropIndex == -1)
        {
            chosenDropIndex = 1;
        }

        var dropPrefab = _drops[chosenDropIndex].dropPrefab;

        // if we finished the available powerups, spawn health kit 
        if (dropPrefab == null)
        {
            dropPrefab = _drops.First(d => d.dropPrefab.GetComponent<Drop>().powerUpType == PowerUpType.IncreaseHealth).dropPrefab;
        }

        var dropInst = Instantiate(dropPrefab,
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

    private IEnumerator DestroyChest()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(this.gameObject);
    }
}
