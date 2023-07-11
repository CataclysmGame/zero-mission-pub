using System;
using Pathfinding;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[Serializable]
public struct Droppable
{
    public GameObject prefab;
    public Transform transform;
    public float rate;
}

public class EnemyController : MonoBehaviour
{
    public int maxHP = 20;

    [HideInInspector] public int currentHP = 0;

    public int projectilePower = 20;
    public int collisionPower = 10;

    public bool increaseDamageWhenExploding = true;
    public float explosionDamageFactor = 2.0f;

    public float movementSpeed = 5.0f;
    public bool useForce = true;
    public float shootTime = 2.0f;
    public float collisionTime = 1.0f;
    public float nextWaypointDistance = 3.0f;
    public float steerTime = 0.2f;

    public float movementSoundInterval = 0.5f;

    public GameObject projectilePrefab;
    public Vector3 projectileScale = new Vector3(1.0f, 1.0f, 1.0f);
    public Vector3 explosionScale = new Vector3(1.0f, 1.0f, 1.0f);
    public Transform firePoint;
    public bool useRaycast = false;
    public LayerMask shootMask;

    public int gridTraversalSize = 1;

    public Droppable[] drops;

    public AudioSource audioSource;
    public AudioClip movementClip;
    public AudioClip shootClip;
    public AudioClip deathClip;
    public AudioClip dropClip;

    protected Rigidbody2D rbd2d;
    protected BoxCollider2D collider2d;
    protected Animator animator;
    protected Seeker seeker;

    private float shootTimer = 0.0f;
    private float collisionTimer = 0.0f;
    private float steerTimer = 0.0f;
    private float movementClipTimer = 0.0f;

    protected Path path;
    protected int currentWaypoint = 0;
    protected bool reachedTarget = false;
    protected Vector2 movementVec;
    protected bool lastDirLeft = false;
    protected bool died = false;
    protected bool gameOver = false;

    private Vector2 movementStartPosition = Vector2.zero;

    [SerializeField] private bool isEndless = false;

    [SerializeField] private IncreaseFunction _endlessDropRateFunction;

    [HideInInspector]
    public bool Died
    {
        get => died;
    }

    [HideInInspector] public Transform targetTransform;

    protected Vector2 lastTargetPos;

    private (int, float)[] sortedDropRates;

    private ITraversalProvider traversalProvider = null;

    private static readonly int AnimatorTriggerShoot = Animator.StringToHash("shoot");

    private void OnValidate()
    {
        ValidateRates();
    }

    private void GetComponents()
    {
        rbd2d = GetComponent<Rigidbody2D>();
        collider2d = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        seeker = GetComponent<Seeker>();
    }

    private void Awake()
    {
        ValidateRates();
        SortRates();

        GetComponents();

        currentHP = maxHP;

        //traversalProvider = new Collider2DTraversalProvider(
        //    Mathf.CeilToInt(collider2d.size.x),
        //    Mathf.CeilToInt(collider2d.size.y)
        //);

        if (gridTraversalSize > 0)
        {
            traversalProvider = GridShapeTraversalProvider.SquareShape(gridTraversalSize);
        }

        EventsManager.Subscribe<PlayerCreatedEvent>(OnPlayerCreated);
        EventsManager.Subscribe<PlayerDiedEvent>(OnPlayerDied);
    }

    private void OnDestroy()
    {
        //UpdateAStarGraphs();

        EventsManager.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        EventsManager.Unsubscribe<PlayerCreatedEvent>(OnPlayerCreated);

        //AstarPath.active.data.pointGraph.AddNode()
    }

    private void OnPlayerCreated(PlayerCreatedEvent evt)
    {
        targetTransform = evt.Player.transform;
        lastTargetPos = targetTransform.position;
        gameOver = false;
    }

    private void OnPlayerDied(PlayerDiedEvent evt)
    {
        gameOver = true;
        CancelInvoke(nameof(UpdatePath));
    }

    protected virtual void Start()
    {
        //UpdateAStarGraphs();

        InvokeRepeating(nameof(UpdatePath), 0, 1.0f);
    }

    private void ValidateRates()
    {
        if (drops == null)
        {
            return;
        }

        float totalRate = 0.0f;
        for (int i = 0; i < drops.Length; i++)
        {
            ref var d = ref drops[i];

            if (d.rate > 1.0f)
            {
                d.rate = 1.0f;
            }
            else if ((totalRate + d.rate + Mathf.Epsilon) > 1.0)
            {
                d.rate = Mathf.Max(0.0f, 1.0f - totalRate);
            }
            else
            {
                totalRate += d.rate;
            }
        }
    }

    private void SortRates()
    {
        if (drops.Length == 0)
        {
            return;
        }

        sortedDropRates = new (int, float)[drops.Length];

        for (int i = 0; i < drops.Length; i++)
        {
            sortedDropRates[i] = (i, drops[i].rate);
        }

        Array.Sort(sortedDropRates, (a, b) => a.Item2.CompareTo(b.Item2));
    }

    /*
    private void UpdateAStarGraphs()
    {
        if (AstarPath.active != null && AstarPath.active.isActiveAndEnabled)
        {
            var guo = new GraphUpdateObject(collider2d.bounds);
            AstarPath.active.UpdateGraphs(guo);
        }
    }
    */

    private void UpdatePath()
    {
        if (gameOver)
        {
            return;
        }

        if (seeker.IsDone())
        {
            var start = useForce ? rbd2d.position : (Vector2)transform.position;
            var destPos = lastTargetPos;

            if ((start - movementStartPosition).magnitude < 0.2f)
            {
                Logger.Log($"[{gameObject.name}] Probably stuck");
                // Pick a random point to "unstuck"
                destPos -= new Vector2(
                    Random.Range(-10.0f, 10.0f),
                    Random.Range(-10.0f, 10.0f)
                );
            }

            movementStartPosition = start;

            var path = ABPath.Construct(start, destPos, OnPathComplete);

            if (traversalProvider != null)
            {
                path.traversalProvider = traversalProvider;
            }

            path.calculatePartial = true;
            seeker.StartPath(path);
            // animator.SetBool("isMoving", true);
        }
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    protected virtual void UpdateTimers()
    {
        shootTimer += Time.deltaTime;
        collisionTimer += Time.deltaTime;
        steerTimer += Time.deltaTime;
        movementClipTimer += Time.deltaTime;
    }

    private void Update()
    {
        if (gameOver || died)
        {
            return;
        }

        UpdateTimers();

        if (targetTransform != null)
        {
            lastTargetPos = targetTransform.position;
        }
        else
        {
            targetTransform = GameManager.Instance.Player.transform;
        }

        if (reachedTarget)
        {
            UpdatePath();
        }

        if (path == null)
        {
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedTarget = true;
            return;
        }
        else
        {
            reachedTarget = false;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rbd2d.position).normalized;

        Move(direction);

        float distance = Vector2.Distance(rbd2d.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        float x = direction.x;

        if (x < 0)
        {
            ChangeDirection(Direction.Left);
            //transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else if (x > 0)
        {
            ChangeDirection(Direction.Right);
            //transform.eulerAngles = new Vector3(0, 0, 0);
        }

        bool isMoving = direction.sqrMagnitude > 0;

        animator.SetBool("isMoving", isMoving);

        if (isMoving && movementClipTimer >= movementSoundInterval)
        {
            movementClipTimer = 0;
            if (movementClip != null)
            {
                audioSource.PlayOneShot(movementClip);
            }
        }

        DoAction(direction);
    }

    private void FixedUpdate()
    {
        if (!useForce)
        {
            rbd2d.MovePosition(rbd2d.position + movementVec * movementSpeed * Time.fixedDeltaTime);
        }
    }

    public virtual void ApplyDamage(int amount)
    {
        if (amount >= currentHP)
        {
            Die();
        }
        else
        {
            currentHP -= amount;
        }
    }

    protected void TryShoot()
    {
        if (shootTimer >= shootTime)
        {
            Shoot();
            shootTimer = 0;
        }
    }

    protected void FacePlayer()
    {
        var playerPos = GameManager.Instance.Player.transform.position;
        var dir = (playerPos - transform.position).normalized;

        if (dir.x < 0)
        {
            ChangeDirection(Direction.Left);
        }
        else if (dir.x > 0)
        {
            ChangeDirection(Direction.Right);
        }
    }

    protected virtual void Shoot()
    {
        FacePlayer();
        animator.SetTrigger(AnimatorTriggerShoot);
    }

    protected virtual Projectile InstantiateProjectile()
    {
        audioSource.PlayOneShot(shootClip);

        /*
        var projInst = Instantiate(
            projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );
        */

        var projInst = GameManager.Instance.ProjectilesPool.GetObject();
        projInst.transform.position = firePoint.position;
        projInst.transform.rotation = firePoint.rotation;
        projInst.transform.localScale = projectileScale;
        var proj = projInst.GetComponent<Projectile>();
        proj.fromPlayer = false;
        proj.power = projectilePower;
        proj.explosionScale = explosionScale;
        proj.AddForce();

        return proj;
    }

    private Droppable? RandomDroppable()
    {
        if (sortedDropRates != null)
        {
            float rand = Random.value;
            float totalRate = 0.0f;

            foreach (var d in sortedDropRates)
            {
                if (rand < ((totalRate + d.Item2) * (isEndless
                        ? _endlessDropRateFunction.GetNewValue(EndlessController.Instance.EnemiesDefeated)
                        : 1f)))
                {
                    return drops[d.Item1];
                }

                totalRate += d.Item2;
            }
        }

        return null;
    }

    protected virtual void Die()
    {
        if (gameOver || died)
        {
            return;
        }

        if (audioSource != null && deathClip != null)
        {
            audioSource.PlayOneShot(deathClip);
        }

        currentHP = 0;
        died = true;
        animator.SetTrigger("die");
        //collider2d.enabled = false;

        EventsManager.Publish(new EnemyDiedEvent());

        if (drops.Length > 0)
        {
            var maybeDrop = RandomDroppable();
            if (maybeDrop.HasValue)
            {
                var drop = maybeDrop.Value;
                var location = transform.position;

                if (isEndless)
                {
                    var endlessController = EndlessController.Instance;
                    if (!endlessController.EndlessArena.isPointInsideArea(location))
                    {
                        location = endlessController.EndlessArena.GetNearestPointInsideArenaFromOutside(location);
                    }
                }

                var rotation = Quaternion.identity;

                if (drop.transform != null)
                {
                    location = drop.transform.position;
                    rotation = drop.transform.rotation;
                }

                Instantiate(drop.prefab, location, rotation);

                if (audioSource != null && dropClip != null)
                {
                    audioSource.PlayOneShot(dropClip);
                }
            }
        }
    }

    public void RemoveFromScene()
    {
        Destroy(gameObject);
    }

    protected void ChangeDirection(Direction dir)
    {
        if (steerTimer < steerTime)
        {
            return;
        }

        steerTimer = 0.0f;

        if (dir == Direction.Left)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else if (dir == Direction.Right)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }

    private void Move(Vector2 direction)
    {
        if (useForce)
        {
            Vector2 force = direction * movementSpeed * Time.deltaTime;

            rbd2d.AddForce(force);
        }
        else
        {
            movementVec = direction;
        }

        //UpdateAStarGraphs();
    }

    protected virtual void DoAction(Vector2 direction)
    {
        if (useRaycast)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                firePoint.position,
                new Vector2(direction.x, 0).normalized,
                Mathf.Infinity,
                shootMask
            );
            foreach (var hit in hits)
            {
                if (hit && hit.collider.CompareTag("Player"))
                {
                    TryShoot();
                    break;
                }
            }
        }
        else
        {
            var y = firePoint.position.y;
            var playerY = lastTargetPos.y;

            if (Mathf.Abs(y - playerY) < nextWaypointDistance)
            {
                TryShoot();
            }
        }
    }

    private bool ApplyDamageIfPlayer(GameObject gameObject)
    {
        if (collisionTimer >= collisionTime &&
            gameObject.CompareTag("Player"))
        {
            collisionTimer = 0.0f;

            var damageToApply = collisionPower;
            if (died && increaseDamageWhenExploding)
            {
                damageToApply = Mathf.FloorToInt(damageToApply * explosionDamageFactor);
                Logger.Log("Double damage applied");
            }

            var player = gameObject.GetComponent<PlayerController>();
            player.ApplyDamage(damageToApply);
            return true;
        }

        return false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ApplyDamageIfPlayer(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ApplyDamageIfPlayer(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        ApplyDamageIfPlayer(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        ApplyDamageIfPlayer(collision.gameObject);
    }
}