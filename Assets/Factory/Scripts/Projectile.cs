using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 64.0f;

    public int power = 10;

    public bool fromPlayer = false;

    public bool canBounce = false;

    public bool applyLifeStealOnSpawners = false;

    public Vector3 explosionScale = new Vector3(1, 1, 1);

    public float maxLifetime = 5.0f;

    public Rigidbody2D rbd2d;
    public Animator animator;
    public GameObject explosion;

    private bool bounced = false;

    private bool hit = false;

    private enum HitResult
    {
        Unknown,
        Enemy,
        Player,
        NPC,
        Item,
        Chest,
        Drop,
        Spawner,
        Wall,
        Skip,
        Acid,
    }

    public void Reset()
    {
        transform.localScale = Vector3.one;
        explosionScale = Vector3.one;

        rbd2d.velocity = Vector3.zero;
        rbd2d.angularVelocity = 0.0f;
        bounced = false;
        hit = false;

        SetAnimationIndex(0);

        Invoke("LifetimeExceeded", maxLifetime);
    }

    public void AddForce()
    {
        rbd2d.AddForce(transform.right * speed, ForceMode2D.Impulse);
    }

    public void SetAnimationIndex(int index)
    {
        for (int i = 0; i < 4; i++)
        {
            animator.SetBool($"projectile{i}", i == index);
        }
        animator.Play(0);
    }

    private void Start()
    {
        AddForce();

        Invoke("LifetimeExceeded", maxLifetime);
    }

    private void RemoveFromScene()
    {
        CancelInvoke();

        GameManager.Instance.ProjectilesPool.ReturnToPool(gameObject);
        //Destroy(gameObject);
    }

    private void LifetimeExceeded()
    {
        //Destroy(gameObject);
        RemoveFromScene();
    }

    private void OnEnemyHit()
    {
        // Apply LifeSteal
        var player = GameManager.Instance.Player;
        player.ApplyLifeSteal(power);
    }

    private void OnSpawnerHit()
    {
        if (applyLifeStealOnSpawners)
        {
            // Apply LifeSteal
            var player = GameManager.Instance.Player;
            player.ApplyLifeSteal(power);
        }
    }

    private void HandleCollision(GameObject collider, Vector2 contactNormal)
    {
        if (hit && !bounced)
        {
            return;
        }

        var hitResult = GetHitResult(collider.gameObject);

        bool shouldExplode = true;
        bool shouldBounce = false;

        switch (hitResult)
        {
            case HitResult.Enemy:
                {
                    var enemy = collider.GetComponent<EnemyController>();
                    if (enemy != null)
                    {
                        enemy.ApplyDamage(power);
                        OnEnemyHit();
                    }
                    break;
                }
            case HitResult.Player:
                {
                    var player = collider.GetComponent<PlayerController>();
                    if (player != null)
                    {
                        player.ApplyDamage(power);
                    }
                    break;
                }
            case HitResult.NPC:
                {
                    var npc = collider.GetComponent<NPCController>();
                    if (npc != null)
                    {
                        npc.ApplyDamage(power);
                    }
                    break;
                }
            case HitResult.Chest:
                {
                    var chest = collider.GetComponent<ChestController>();

                    if (chest != null)
                    {
                        chest.Open();
                    }

                    break;
                }
            case HitResult.Item:
                {
                    var item = collider.GetComponent<BaseItemController>();
                    if (item != null)
                    {
                        item.Break();
                    }
                    break;
                }
            case HitResult.Spawner:
                {
                    var spawner = collider.GetComponent<Spawner>();
                    if (spawner != null && spawner.canBeKilled)
                    {
                        spawner.ApplyDamage(power);
                        OnSpawnerHit();
                    }
                    else
                    {
                        shouldExplode = false;
                    }
                    break;
                }
            case HitResult.Wall:
                shouldExplode = false;
                shouldBounce = true;
                break;
            case HitResult.Skip:
                return;
            case HitResult.Unknown:
                break;
            default:
                break;
        }

        hit = true;

        if (canBounce && shouldBounce && !bounced)
        {
            Bounce(contactNormal);
            return;
        }

        if (shouldExplode && explosion != null)
        {
            //var explosionInst = Instantiate(explosion, transform.position, Quaternion.identity);
            var explosionInst = GameManager.Instance.ExplosionsPool.GetObject();
            explosionInst.transform.position = transform.position;
            explosionInst.transform.rotation = Quaternion.identity;
            explosionInst.transform.localScale = explosionScale;
        }

        //Destroy(gameObject);
        RemoveFromScene();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        var collisionPoint = collider.ClosestPoint(transform.position);
        var normal = ((Vector2)transform.position - collisionPoint).normalized;

        HandleCollision(collider.gameObject, normal);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject, collision.contacts[0].normal);
    }

    private HitResult GetHitResult(GameObject objectHit)
    {
        switch (objectHit.tag)
        {
            case "Player":
                return fromPlayer ? HitResult.Skip : HitResult.Player;
            case "Enemy":
                if (fromPlayer)
                {
                    var enemy = objectHit.GetComponent<EnemyController>();
                    if (!enemy.Died)
                    {
                        // Skip dead enemies
                        return HitResult.Enemy;
                    }
                }
                return HitResult.Skip;
            case "NPC":
                return HitResult.NPC;
            case "Item":
                if (objectHit.GetComponent<ChestController>() != null)
                {
                    if (!fromPlayer)
                    {
                        return HitResult.Skip;
                    }
                    return HitResult.Chest;
                }
                else if (objectHit.GetComponent<BaseItemController>() != null)
                {
                    return HitResult.Item;
                }
                else if (objectHit.GetComponent<Drop>() != null)
                {
                    // Skip hitting drops
                    return HitResult.Skip;
                }
                else
                {
                    return HitResult.Unknown;
                }
            case "Spawner":
                return fromPlayer ? HitResult.Spawner : HitResult.Skip;
            case "Wall":
                return HitResult.Wall;
            case "Projectile":
                return HitResult.Skip;
            case "Obstacles":
                return HitResult.Skip;
            default:
                return HitResult.Unknown;
        }
    }

    private void Bounce(Vector2 normal)
    {
        var reflectDir = Vector2.Reflect(
            rbd2d.velocity,
            normal
        ).normalized;

        rbd2d.velocity = reflectDir * speed;

        bounced = true;
    }
}
