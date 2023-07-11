using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class NPCController : MonoBehaviour
{
    public int maxHP = 200;
    public bool canBeKilled = true;

    public bool canTalk = true;
    public bool startTalking = true;

    [Multiline]
    public string dialogLines;

    public Text uiText;

    public Image dialogPanel;

    public GameObject talkArea;

    public AudioClip hitClip;
    public AudioClip deathClip;

    public GameObject drop;

    private int currentHP;
    private bool dead = false;

    private Animator animator;
    private AudioSource audioSource;

    private System.Threading.CancellationTokenSource talkCts;

    private void Awake()
    {
        currentHP = maxHP;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (canTalk && startTalking)
        {
            StartTalking();
        }

        InvokeRepeating("CheckPlayerInsideArea", 0.3f, 0.3f);
    }

    private void CheckPlayerInsideArea()
    {
        var player = GameManager.Instance.Player;
        if (!Util.IsGameObjectInsideArea(player.gameObject, talkArea))
        {
            StopTalking();
        }
    }

    private async UniTask StartTalking()
    {
        talkCts = new System.Threading.CancellationTokenSource();

        animator.SetBool("talking", true);

        var ts = System.TimeSpan.FromSeconds(4);
        var lines = dialogLines.Split('\n');
        foreach (var line in lines)
        {
            uiText.text = line;
            await UniTask.Delay(ts, cancellationToken: talkCts.Token);
        }

        StopTalking();
    }

    private void StopTalking()
    {
        CancelInvoke("CheckPlayerInsideArea");
        if (talkCts != null)
        {
            if (talkCts.Token.CanBeCanceled)
            {
                talkCts.Cancel();
            }
        }
        animator.SetBool("talking", false);
        if (uiText != null)
        {
            //uiText.text = "";
            dialogPanel.GetComponent<Animator>().SetTrigger("AnimationStop");
        }
    }

    public void ApplyDamage(int amount)
    {
        if (dead)
        {
            return;
        }
        currentHP = Mathf.Clamp(currentHP - amount, 0, maxHP);
        if (currentHP == 0)
        {
            Die();
        }
        else
        {
            audioSource.PlayOneShot(hitClip);
            animator.SetTrigger("hit");
        }
    }

    private void Die()
    {
        if (dead)
        {
            return;
        }
        dead = true;
        audioSource.PlayOneShot(deathClip);
        StopTalking();
        animator.SetTrigger("dead");
    }

    public void RemoveFromScene()
    {
        if (drop != null)
        {
            Instantiate(drop, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
