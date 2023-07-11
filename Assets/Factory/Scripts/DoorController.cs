using Pathfinding;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DoorController : MonoBehaviour
{
    public GameObject left;
    public GameObject center;
    public GameObject right;

    public GameObject topLeft;
    public GameObject topRight;

    public string topClosedSortingLayer;
    public string topOpenedSortingLayer;

    public GameObject enemyArea;
    public GameObject closeArea;
    public bool includeSpawners = false;

    public AudioSource audioSource;
    public AudioClip openClip;
    public AudioClip closeClip;

    private Animator leftAnimator;
    private Animator centerAnimator;
    private Animator rightAnimator;
    private BoxCollider2D centerCollider;
    private Light2D light;

    private bool opened = false;
    private bool currentlyOpening = false;

    void Awake()
    {
        if (left != null)
        {
            leftAnimator = left.GetComponent<Animator>();
        }
        if (center != null)
        {
            centerAnimator = center.GetComponent<Animator>();
            centerCollider = center.GetComponent<BoxCollider2D>();
        }
        if (right != null)
        {
            rightAnimator = right?.GetComponent<Animator>();
        }

        light = GetComponentInChildren<Light2D>();
    }

    void Start()
    {
        UpdateAStarPath();

        InvokeRepeating("CheckArea", 0, 0.5f);
    }

    private void UpdateAStarPath()
    {
        if (AstarPath.active != null && AstarPath.active.isActiveAndEnabled)
        {
            var guo = new GraphUpdateObject(centerCollider.bounds);
            AstarPath.active.UpdateGraphs(guo);
        }
    }

    private int CountEnemiesInsideArea()
    {
        int count = Util.CountObjectsInsideArea("Enemy", enemyArea);
        if (includeSpawners)
        {
            count += Util.CountObjectsInsideArea("Spawner", enemyArea);
        }

        return count;
    }

    public void Open()
    {
        Logger.Log("Opening");

        currentlyOpening = true;

        light.enabled = false;

        if (leftAnimator != null)
        {
            leftAnimator.SetTrigger("open");
        }

        if (centerAnimator != null)
        {
            centerAnimator.SetTrigger("open");
        }

        if (rightAnimator != null)
        {
            rightAnimator.SetTrigger("open");
        }

        if (audioSource != null && openClip != null)
        {
            audioSource.PlayOneShot(openClip);
        }

        if (closeArea != null)
        {
            InvokeRepeating("CheckCloseArea", 0, 0.5f);
        }
    }

    public void Close()
    {
        Logger.Log("Closing");

        currentlyOpening = false;

        opened = false;
        centerCollider.enabled = true;

        UpdateAStarPath();

        if (topLeft != null)
        {
            var sprite = topLeft.GetComponent<SpriteRenderer>();
            sprite.sortingLayerName = topClosedSortingLayer;
        }

        if (topRight != null)
        {
            var sprite = topRight.GetComponent<SpriteRenderer>();
            sprite.sortingLayerName = topClosedSortingLayer;
        }

        if (leftAnimator != null)
        {
            leftAnimator.SetTrigger("close");
        }

        if (centerAnimator != null)
        {
            centerAnimator.SetTrigger("close");
        }

        if (rightAnimator != null)
        {
            rightAnimator.SetTrigger("close");
        }
        
        light.enabled = true;
    }

    public void DisableCollider()
    {
        if (!currentlyOpening)
        {
            return;
        }

        centerCollider.enabled = false;
        opened = true;

        UpdateAStarPath();

        if (topLeft != null)
        {
            var sprite = topLeft.GetComponent<SpriteRenderer>();
            sprite.sortingLayerName = topOpenedSortingLayer;
        }

        if (topRight != null)
        {
            var sprite = topRight.GetComponent<SpriteRenderer>();
            sprite.sortingLayerName = topOpenedSortingLayer;
        }
    }
    
    private void CheckArea()
    {
        if (opened)
        {
            return;
        }

        var count = CountEnemiesInsideArea();
        if (count == 0)
        {
            CancelInvoke("CheckArea");
            Open();
        }
    }

    private void CheckCloseArea()
    {
        var player = GameManager.Instance.Player;
        if (Util.IsGameObjectInsideArea(player.gameObject, closeArea))
        {
            CancelInvoke("CheckCloseArea");
            Close();
        }
    }
}
