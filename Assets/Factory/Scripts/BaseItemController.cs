using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

public class BaseItemController : MonoBehaviour
{
    public bool canBeBroken = true;

    public bool removeLightingWhenBroken = true;
    public bool stopAnimationWhenBroken = true;
    public bool useBrokeTrigger = false;

    public AudioSource audioSource;
    public AudioClip brokenClip;

    public UnityEvent onBroken;

    private void Awake()
    {
        InvokeRepeating(nameof(CheckPlayerDistance), 0.2f, 0.2f);
    }

    public void Break()
    {
        if (!canBeBroken)
        {
            return;
        }

        CancelInvoke();
        audioSource.enabled = true;
        audioSource.Stop();

        if (stopAnimationWhenBroken)
        {
            var animators = GetComponentsInChildren<Animator>();
            foreach (var animator in animators)
            {
                if (useBrokeTrigger)
                {
                    animator.SetTrigger("broken");
                }
                else
                {
                    animator.enabled = false;
                }
            }
        }

        if (removeLightingWhenBroken)
        {
            var lights = GetComponentsInChildren<Light2D>();
            foreach (var light in lights)
            {
                light.enabled = false;
            }
        }

        if (audioSource != null && brokenClip != null)
        {
            audioSource.loop = false;
            audioSource.volume = 1.0f;
            audioSource.clip = brokenClip;
            audioSource.Play();
        }

        onBroken.Invoke();
    }

    private void CheckPlayerDistance()
    {
        // Disable audio source if too far from player
        var distance = audioSource.maxDistance;
        var player = GameManager.Instance.Player;

        if (Vector2.Distance(player.transform.position, transform.position) > distance)
        {
            audioSource.enabled = false;
        }
        else
        {
            audioSource.enabled = true;
        }
    }
}