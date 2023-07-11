using Array = System.Array;
using UnityEngine;
using Pathfinding;
using Cysharp.Threading.Tasks;
using DG.Tweening;

[System.Serializable]
public struct Spawnable
{
    public GameObject prefab;
    public Transform transform;
    public float rate;
}

public class Spawner : MonoBehaviour
{
    #region Editor Properties
    public Spawnable[] spawnables;
    [Min(0.1f)]
    public float spawnInterval = 10.0f;
    public float firstSpawnDelay = 0.0f;
    public bool spawnImmediately = true;
    public bool activateWhenPlayerInsideArea = false;

    [Header("Area")]
    public GameObject spawnArea;
    public int areaTrigger = 0;
    public GameObject spawnPoint;

    [Min(1)]
    public int spawnGroupSize = 1;
    [Min(0)]
    public int maxSpawns = 0;

    public int MaxHP = 100;
    public bool canBeKilled = false;
    public bool hitsAlwaysDoOneDamage = false;

    public GameObject drop;
    public float dropRate = 0.5f;

    public GameObject spawnEffect;
    public GameObject deathEffect;

    public Animator animator;
    public AudioClip spawnClip;
    public AudioClip deathClip;
    public AudioClip dropFallenClip;
    public AudioSource audioSource;
    #endregion

    private CapsuleCollider2D collider2d;

    private readonly Vector3 ZeroZ = new Vector3(1, 1, 0);

    protected (int, float)[] sortedRates;
    protected int spawnCounter = 0;
    private int HP = 0;

    private bool destroyed = false;

    void OnValidate()
    {
        if (activateWhenPlayerInsideArea && spawnArea == null)
        {
            activateWhenPlayerInsideArea = false;
            throw new System.Exception("You must set a spawn area");
        }
        ValidateRates();
    }

    private void ValidateRates()
    {
        if (spawnables == null)
        {
            return;
        }
        float totalRate = 0.0f;
        for (int i = 0; i < spawnables.Length; i++)
        {
            ref var s = ref spawnables[i];

            if (s.rate > 1.0f)
            {
                s.rate = 1.0f;
            }
            else if ((totalRate + s.rate + Mathf.Epsilon) > 1.0f)
            {
                s.rate = Mathf.Max(0.0f, 1.0f - totalRate);
            }
            else
            {
                totalRate += s.rate;
            }
        }
    }

    public void SortRates()
    {
        if (spawnables.Length == 0)
        {
            return;
        }

        sortedRates = new (int, float)[spawnables.Length];

        for (int i = 0; i < spawnables.Length; i++)
        {
            sortedRates[i] = (i, spawnables[i].rate);
        }

        Array.Sort(sortedRates, (a, b) => a.Item2.CompareTo(b.Item2));
    }

    private void Awake()
    {
        ValidateRates();
        SortRates();

        HP = MaxHP;

        collider2d = GetComponent<CapsuleCollider2D>();

        Logger.Log("HP: " + HP);
    }

    public void ChangeInterval(float interval, bool spawnAfterwards)
    {
        if (interval < 0.1f)
        {
            return;
        }

        spawnInterval = interval;

        StopSpawning();

        if (spawnAfterwards)
        {
            Invoke(nameof(Spawn), firstSpawnDelay);
        }
        InvokeRepeating(
            nameof(Spawn),
            spawnInterval,
            spawnInterval
        );
    }

    private void StartSpawning()
    {
        if (spawnImmediately)
        {
            Invoke(nameof(Spawn), firstSpawnDelay);
        }
        InvokeRepeating(
            nameof(Spawn),
            spawnInterval,
            spawnInterval
        );
    }

    private void Start()
    {
        UpdateAStarPath();

        if (activateWhenPlayerInsideArea)
        {
            InvokeRepeating("WaitForPlayer", 0, 1.0f);
        }
        else
        {
            StartSpawning();
        }
    }

    void OnDestroy()
    {
        StopSpawning();

        UpdateAStarPath();
    }

    protected void StopSpawning()
    {
        CancelInvoke("Spawn");
    }

    public void ApplyDamage(int amount)
    {
        if (destroyed)
        {
            return;
        }

        if (hitsAlwaysDoOneDamage)
        {
            amount = 1;
        }
        if (canBeKilled)
        {
            Logger.Log($"Applied {amount} damage to spawner");
            HP = Mathf.Max(0, HP - amount);
            Logger.Log($"Spawner HP changed to {HP}");
            if (HP == 0)
            {
                Die();
            }
        }
    }

    private void SpawnDrop()
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

    private async UniTask Die()
    {
        if (destroyed)
        {
            return;
        }

        destroyed = true;

        collider2d.enabled = false;
        foreach (var spr in GetComponentsInChildren<SpriteRenderer>())
        {
            spr.enabled = false;
        }

        if (audioSource != null && deathClip != null)
        {
            Logger.Log("Playing spawner death clip");
            audioSource.PlayOneShot(deathClip);
            await Util.WaitClipEnd(audioSource, deathClip);
        }
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        if (drop != null)
        {
            if (Random.value <= dropRate)
            {
                SpawnDrop();
            }
        }
        Destroy(gameObject);
    }

    private void UpdateAStarPath()
    {
        if (AstarPath.active != null && AstarPath.active.isActiveAndEnabled)
        {
            var bounds = collider2d.bounds;
            bounds.Expand(1.0f);
            var guo = new GraphUpdateObject(bounds);
            AstarPath.active.UpdateGraphs(guo);
        }
    }

    public virtual void Spawn()
    {
        if (spawnArea != null && areaTrigger >= 0)
        {
            var count = CountEnemiesInsideArea();
            if (count > areaTrigger)
            {
                return;
            }
        }

        for (var i = 0; i < spawnGroupSize; i++)
        {
            var maybeSpawnable = RandomSpawnable();
            if (!maybeSpawnable.HasValue)
            {
                Logger.Log("Got a null spawnable");
                continue;
            }

            var spawnable = maybeSpawnable.Value;
            var location = transform.position;
            var rotation = Quaternion.identity;
            if (spawnPoint != null)
            {
                location = spawnPoint.transform.position;
            }
            if (spawnable.transform != null)
            {
                location = spawnable.transform.position;
                rotation = spawnable.transform.rotation;
            }

            Instantiate(spawnable.prefab, location, rotation);
            spawnCounter++;

            if (spawnEffect != null)
            {
                Instantiate(spawnEffect, transform);
            }

            if (animator != null)
            {
                animator.SetTrigger("spawn");
            }

            if (maxSpawns != 0 && spawnCounter >= maxSpawns)
            {
                StopSpawning();
                return;
            }
        }

        if (audioSource != null && spawnClip != null)
        {
            audioSource.PlayOneShot(spawnClip);
        }
    }

    public virtual Spawnable? RandomSpawnable()
    {
        if (sortedRates != null)
        {
            float rand = Random.value;
            float totalRate = 0.0f;

            foreach (var s in sortedRates)
            {
                if (rand < totalRate + s.Item2)
                {
                    return spawnables[s.Item1];
                }

                totalRate += s.Item2;
            }
        }

        return null;
    }

    private void WaitForPlayer()
    {
        if (CheckPlayerInsideArea())
        {
            StartSpawning();
            CancelInvoke("WaitForPlayer");
        }
    }

    private bool CheckPlayerInsideArea()
    {
        var player = GameManager.Instance.Player;

        return Util.IsGameObjectInsideArea(player.gameObject, spawnArea);
    }

    private int CountEnemiesInsideArea() => Util.CountObjectsInsideArea("Enemy", spawnArea);
}
