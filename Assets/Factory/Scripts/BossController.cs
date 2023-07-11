using Enum = System.Enum;
using UnityEngine;
using System.Collections;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class BossController : EnemyController
{
    enum BossAction
    {
        None,
        BoostMovementSpeed,
        Shot,
        DoubleShot,
        DiagonalShot,
        Minigun,
        Flame,
    }

    public GameObject activationArea;

    public DoorController bossDoor;

    public bool finalBoss = true;

    public bool endless = false;

    public int flameDamage = 60;

    public int minigunDamage = 5;

    public int minigunShots = 4;

    public float maxShootInterval = 5.0f;

    public float actionInterval = 2.0f;

    public Transform minigunFirePoint;
    public GameObject flameCollider;
    public GameObject minigunLights;

    public AudioClip minigunClip;
    public AudioClip flameClip;
    public AudioClip retractLegsClip;

    private float actionTimer = 0.0f;

    private float lastShootTimer = 0.0f;

    private BossAction currentAction = BossAction.None;

    private readonly BossAction[] shootActions = {
        BossAction.Shot,
        BossAction.DoubleShot,
        BossAction.DiagonalShot,
        BossAction.Minigun
    };

    private bool movementBoosted = false;
    private bool flaming = false;

    protected override void UpdateTimers()
    {
        base.UpdateTimers();
        actionTimer += Time.deltaTime;
        lastShootTimer += Time.deltaTime;
    }

    protected override void Start()
    {
        InvokeRepeating(nameof(CheckForPlayerInsideArea), 0, 0.5f);
    }

    private void CheckForPlayerInsideArea()
    {
        if (endless || Util.CountObjectsInsideArea("Player", activationArea) > 0)
        {
            base.Start();
            CancelInvoke(nameof(CheckForPlayerInsideArea));

            Logger.Log("Boss activated");

            if (!endless)
            {
                bossDoor.Close();
                EventsManager.Publish(new BossActivatedEvent(this));
            }
        }
    }

    private BossAction GetRandomAction()
    {
        var values = new List<BossAction>();
        foreach (var a in Enum.GetValues(typeof(BossAction)))
        {
            values.Add((BossAction)a);
        }

        values.Remove(BossAction.None);

        if (movementBoosted)
        {
            values.Remove(BossAction.BoostMovementSpeed);
        }

        if (flaming)
        {
            values.Remove(BossAction.Flame);
        }

        return values[Random.Range(0, values.Count)];
    }

    private bool IsPlayerShootable(Vector2 position, Vector2 direction)
    {
        if (useRaycast)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                position,
                new Vector2(direction.x, 0).normalized,
                Mathf.Infinity,
                shootMask
            );
            foreach (var hit in hits)
            {
                if (hit && hit.collider.CompareTag("Player"))
                {
                    return true;
                }
            }
        }
        else
        {
            var y = position.y;
            var playerY = lastTargetPos.y;

            if (Mathf.Abs(y - playerY) < nextWaypointDistance)
            {
                return true;
            }
        }

        return false;
    }

    protected override void DoAction(Vector2 direction)
    {
        if (flaming)
        {
            return;
        }

        var action = BossAction.None;

        Vector2 playerPos = GameManager.Instance.Player.transform.position;
        var distance = Vector2.Distance(transform.position, playerPos);

        if (distance < 3)
        {
            actionTimer = 0;
            action = BossAction.Flame;
        }
        else if (IsPlayerShootable(minigunFirePoint.position, direction))
        {
            actionTimer = 0;
            action = BossAction.Minigun;
        }
        else if (IsPlayerShootable(firePoint.position, direction))
        {
            actionTimer = 0;
            action = Util.RandomBool() ? BossAction.DoubleShot : BossAction.Shot;
        }
        else if (lastShootTimer >= maxShootInterval)
        {
            actionTimer = 0;
            action = Util.PickRandom(shootActions);
        }
        else if (actionTimer >= actionInterval)
        {
            actionTimer = 0;
            action = GetRandomAction();
        }

        if (action != BossAction.None)
        {
            currentAction = action;

            switch (action)
            {
                case BossAction.Shot:
                case BossAction.DoubleShot:
                case BossAction.DiagonalShot:
                case BossAction.Minigun:
                case BossAction.Flame:
                    ShotAction();
                    break;
                case BossAction.BoostMovementSpeed:
                    BoostMovementSpeedAction();
                    break;
                default:
                    currentAction = BossAction.None;
                    break;
            }
        }
    }

    private void ShotAction()
    {
        lastShootTimer = 0;
        TryShoot();
    }

    private void BoostMovementSpeedAction()
    {
        StartCoroutine(BoostMovementSpeedRoutine());
    }

    protected override void Shoot()
    {
        FacePlayer();

        audioSource.PlayOneShot(retractLegsClip);

        switch (currentAction)
        {
            case BossAction.Flame:
                animator.SetTrigger("shoot1");
                break;
            case BossAction.Minigun:
                minigunLights.SetActive(true);
                animator.SetTrigger("shoot2");
                break;
            default:
                animator.SetTrigger("shoot");
                break;
        }
    }

    public void EnableFlame()
    {
        flaming = true;
        audioSource.PlayOneShot(flameClip);
        flameCollider.SetActive(true);
    }

    public void DisableFlame()
    {
        flameCollider.SetActive(false);
        flaming = false;
    }

    private IEnumerator BoostMovementSpeedRoutine()
    {
        movementBoosted = true;
        var prevMovementSpeed = movementSpeed;
        movementSpeed = movementSpeed * 2.0f + 1.0f;
        Logger.Log("Movement boosted");
        yield return new WaitForSeconds(actionInterval * 0.9f);
        movementSpeed = prevMovementSpeed;
        movementBoosted = false;
    }

    private Projectile InstantiateBossProjectile(BossAction action, Quaternion rotation)
    {
        var projInst = GameManager.Instance.ProjectilesPool.GetObject();

        var proj = projInst.GetComponent<Projectile>();
        proj.fromPlayer = false;

        if (action == BossAction.Minigun)
        {
            projInst.transform.position = minigunFirePoint.position;
            projInst.transform.rotation = minigunFirePoint.rotation * rotation;
            projInst.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            proj.explosionScale = new Vector3(0.5f, 0.5f, 0.5f);
            proj.power = minigunDamage;

            proj.SetAnimationIndex(0);
        }
        else
        {
            projInst.transform.position = firePoint.position;
            projInst.transform.rotation = firePoint.rotation * rotation;
            projInst.transform.localScale = projectileScale;
            proj.power = projectilePower;
            proj.explosionScale = explosionScale;

            proj.SetAnimationIndex(2);
        }

        proj.AddForce();

        return proj;
    }

    private Projectile[] InstantiateBossProjectiles(BossAction action)
    {
        float[] rotations;
        if (action == BossAction.DiagonalShot)
        {
            rotations = new float[] { -45.0f, 0.0f, 45.0f };
        }
        else
        {
            rotations = new float[] { 0.0f };
        }

        var projectiles = new Projectile[rotations.Length];
        for (int i = 0; i < rotations.Length; i++)
        {
            var rot = rotations[i];
            var proj = InstantiateBossProjectile(action, Quaternion.Euler(0, 0, rot));
            projectiles[i] = proj;
        }

        return projectiles;
    }

    private async UniTask<Projectile[]> InstantiateBossProjectilesDelayed(BossAction action, float delay)
    {
        await UniTask.Delay(Mathf.FloorToInt(delay * 1000));
        return InstantiateBossProjectiles(action);
    }

    protected override Projectile InstantiateProjectile()
    {
        var action = currentAction;

        if (currentAction == BossAction.Shot || currentAction == BossAction.DiagonalShot)
        {
            audioSource.PlayOneShot(shootClip);
            return InstantiateBossProjectiles(action)[0];
        }
        else if (currentAction == BossAction.DoubleShot)
        {
            var projs = InstantiateBossProjectiles(action);

            audioSource.PlayOneShot(shootClip);
            InstantiateBossProjectilesDelayed(action, 0.15f).ContinueWith((_) =>
            {
                audioSource.PlayOneShot(shootClip);
            });
            return projs[0];
        }
        else if (currentAction == BossAction.Minigun)
        {
            audioSource.PlayOneShot(minigunClip);
            var proj = InstantiateBossProjectiles(action);
            var tasks = new UniTask<Projectile[]>[4];
            for (var i = 0; i < minigunShots; i++)
            {
                tasks[i] = InstantiateBossProjectilesDelayed(action, (i + 1) * 0.08f);
            }

            UniTask.WhenAll(tasks).ContinueWith((_) => minigunLights.SetActive(false));
        }

        return null;
    }

    protected override void Die()
    {
        base.Die();
        if (!endless && finalBoss)
        {
            EventsManager.Publish(new BossDiedEvent(this));
            bossDoor.Open();
        }
    }

    public override void ApplyDamage(int amount)
    {
        base.ApplyDamage(amount);

        if (!endless)
        {
            EventsManager.Publish(new BossHealthChangedEvent(this, currentHP));
        }
    }
}
