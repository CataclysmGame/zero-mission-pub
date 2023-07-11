using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Factory.Scripts.Events;
using Sentry;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    public CharactersList characters;

    public Character character;

    public float movementSpeed = 5.0f;
    public bool normalizeMovement = false;
    public float shootInterval = 1.0f;
    public int maxMunitions = 5;
    public float manualReloadFactor = 1.2f;

    public float stepsSoundInterval = 0.3f;
    public float stepsVolumeScale = 0.6f;

    public int maxHP = 100;

    [HideInInspector] public int currentHP = 1;

    public int hpOverflowAmount = 5;

    public int projectilePower = 10;

    public float lifeSteal = 0.0f;

    public bool diagonalShot = false;
    public bool doubleShot = false;
    public bool backShot = false;
    public bool bouncingShot = false;
    public bool guardianAngel = false;

    public bool disableLifeSteal = false;
    public bool disableBounce = false;

    public float movementSpeedLimit = 8.0f;

    public int hpCap = 20 * 11;
    public int munitionsCap = 15;
    public float lifeStealCap = 0.10f;
    public float shootIntervalCap = 0.1f;

    public float reloadInterval = 1.0f;
    public float reloadIntervalCap = 0.1f;

    public float lifeStealIncrease = 0.05f;

    public float joystickTollerance = 0.1f;

    public float diagonalShotDamageFactor = 0.5f;

    [FormerlySerializedAs("reloadSpeedDecrease")]
    public float reloadIntervalDecrease = 0.1f;

    [FormerlySerializedAs("shootSpeedDecrease")]
    public float shootIntervalDecrease = 0.1f;

    public float movementSpeedIncrease = 0.2f;

    public int hpIncrease = 20;

    public int damageIncrease = 5;
    public bool randomizeDamageIncrease = true;
    public int minDamageIncrease = 2;

    public float guardianInterval = 5f;
    public float guardianDuration = 1.5f;
    public float guardianRate = 0.5f;

    public Rigidbody2D rbd2d;
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public Transform firePoint;
    public GameObject projectilePrefab;
    public GameObject lightObj;

    public GameObject guardianEffect;

    public AudioSource audioSource;
    public AudioClip walkClip;
    public AudioClip reloadClip;
    public AudioClip shootClip;
    public AudioClip doubleShootClip;
    public AudioClip tripleShotClip;
    public AudioClip gaActivationClip;
    public AudioClip gaHitClip;
    public AudioClip hitClip;
    public AudioClip deathClip;

    public AudioClip ceoPowerUpClip;

    public GameObject notificationObj;
    public Vector3 notificationScale = new Vector3(1f, 1f, 1f);

    public AnimationClip reloadingAnimClip;
    public AnimationClip shootingAnimClip;

    public float endlessWalkedDistanceCheckPeriod = 25.0f;
    public float endlessWalkedDistanceCheckMinStd = 1.0f;

    private Skin _skin;

    private GameControls controls;

    private int currentMunitions;
    private float shootTimer = 0;
    private float stepsTimer = 0;
    private float guardianActivationTimer = 0.0f;
    private float guardianDurationTimer = 0.0f;
    private float lastHitTime = 0;

    private Vector2 movementVec;
    private bool isShooting = false;
    private bool isReloading = false;
    private bool gameWon = false;

    private Queue<(DropType, PowerUpType)> _notificationsQueue = new();
    private bool _showingNotification;

    private static readonly int AnimPropIsMoving = Animator.StringToHash("isMoving");
    private static readonly int AnimPropShootSpeed = Animator.StringToHash("shootSpeed");
    private static readonly int AnimTriggerShoot = Animator.StringToHash("shoot");
    private static readonly int AnimTriggerReload = Animator.StringToHash("reload");
    private static readonly int AnimPropReloadSpeed = Animator.StringToHash("reloadSpeed");
    private static readonly int AnimatorTriggerHit = Animator.StringToHash("hit");
    private static readonly int AnimatorTriggerDie = Animator.StringToHash("die");
    private static readonly int AnimatorPropMovementSpeed = Animator.StringToHash("movementSpeed");

    private bool _isEndless;
    private float _endlessCheckTimer;
    private double _walkedDistance;

    private Queue<double> _walkedDistanceQueue;

    private bool IsCEO => character != null &&
                          character.playerName == "CEO";

    private bool IsCEOWithBaseSkin => IsCEO && (_skin == null || _skin.id == "base");

    [SerializeField] private Collider2D _wallsCollider;

    public Collider2D WallsCollider => _wallsCollider;

    private void Awake()
    {
        controls = new GameControls();

        currentHP = maxHP;

        EventsManager.Publish(new PlayerCreatedEvent(this));

        FillMunitions();

        EventsManager.Subscribe<PowerUpUnlockedEvent>(OnPowerUpUnlocked);
        EventsManager.Subscribe<BossDiedEvent>(OnBossDied);
        EventsManager.Subscribe<PlayerEnteredInstantDeathAreaEvent>(OnInstantDeathAreaEntered);
    }

    private void Start()
    {
        if (SceneParameters.Instance.ContainsParameter(SceneParameters.PARAM_PLAYER_ATLAS))
        {
            var nftTex = GetComponent<NFTTexture>();
            var atlasTexture = SceneParameters.Instance.GetParameter<Texture2D>(SceneParameters.PARAM_PLAYER_ATLAS);
            nftTex.SetNFTTexture(atlasTexture);
        }

        var skin = SceneParameters.Instance.GetParameter<Skin>(SceneParameters.PARAM_PLAYER_SKIN, () => null);

        _skin = skin;

        var enableSkinGlow = false;

        if (SceneParameters.Instance.ContainsParameter(SceneParameters.PARAM_PLAYER_SKIN_FIRST_HAND))
        {
            var isSkinFirstHand =
                SceneParameters.Instance.GetParameter<bool>(SceneParameters.PARAM_PLAYER_SKIN_FIRST_HAND);

            Logger.Log("Skin is first hand: " + isSkinFirstHand);

            if (skin != null)
            {
                enableSkinGlow = true;
            }
        }

        var skinGlowObj = transform.Find("SkinGlow");
        if (skinGlowObj != null)
        {
            skinGlowObj.gameObject.SetActive(enableSkinGlow);
            if (skin != null)
            {
                var skinGlow = skinGlowObj.GetComponent<SkinGlow>();
                if (skinGlow != null)
                {
                    skinGlow.SetSkin(skin);
                }
            }
        }

        if (character != null)
        {
            LoadStatsFromCharacter(character);
        }

        var characterIndex = -1;
        if (SceneParameters.Instance.ContainsParameter(SceneParameters.PARAM_PLAYER_CHR_IDX))
        {
            characterIndex = SceneParameters.Instance.GetParameter<int>(SceneParameters.PARAM_PLAYER_CHR_IDX);
        }

        if (characterIndex >= 0 && characterIndex < characters.characters.Length)
        {
            LoadStatsFromCharacter(characters.characters[characterIndex]);
        }

        _isEndless = GameObject.Find("Endless") != null;
        if (_isEndless)
        {
            _walkedDistanceQueue = new();
            InvokeRepeating(nameof(EnqueueWalkedDistance), 1, 1);
        }
    }

    private void EnqueueWalkedDistance()
    {
        _walkedDistanceQueue.Enqueue(_walkedDistance);
        if (_walkedDistanceQueue.Count > (int)endlessWalkedDistanceCheckPeriod)
        {
            _walkedDistanceQueue.Dequeue();
        }
    }

    private void CheckEndlessWalkedDistance()
    {
        var avg = _walkedDistanceQueue.Average();
        var std = Mathf.Sqrt(_walkedDistanceQueue.Average(v => Mathf.Pow((float)(v - avg), 2)));

        if (std < endlessWalkedDistanceCheckMinStd)
        {
            var endlessObj = GameObject.Find("Endless");
            var endlessController = endlessObj == null ? null : endlessObj.GetComponent<EndlessController>();
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");

            SentrySdk.AddBreadcrumb(new Breadcrumb("endlessCheater", "playerInfo", data: new Dictionary<string, string>
            {
                { "userAddress", PersistentSettings.Instance.UserAddress },
                { "walkedDistance", _walkedDistance.ToString(CultureInfo.InvariantCulture) },
                { "checkPeriod", endlessWalkedDistanceCheckPeriod.ToString(CultureInfo.InvariantCulture) },
                { "checkMinStd", endlessWalkedDistanceCheckMinStd.ToString(CultureInfo.InvariantCulture) },
                { "std", std.ToString(CultureInfo.InvariantCulture) },
                { "enemiesCount", enemies.Length.ToString() },
                { "killedEnemies", endlessController == null ? null : endlessController.EnemiesDefeated.ToString() }
            }));

            const string msg = "Player is not moving and not taking damage in endless mode";
            Logger.LogError(msg);
            SentrySdk.CaptureMessage(msg, SentryLevel.Error);
        }

        Debug.Log("Std: " + std);
    }

    private void OnEnable()
    {
        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        controls.Gameplay.Disable();
    }

    private void OnDestroy()
    {
        EventsManager.Unsubscribe<PowerUpUnlockedEvent>(OnPowerUpUnlocked);
        EventsManager.Unsubscribe<BossDiedEvent>(OnBossDied);
    }

    private void LoadStatsFromCharacter(Character character)
    {
        maxHP = character.maxHp;
        maxMunitions = character.maxMunitions;
        movementSpeed = character.movementSpeed;
        shootInterval = character.shootInterval;
        reloadInterval = character.reloadInterval;
        lifeSteal = character.lifeSteal;
        projectilePower = character.damage;

        hpCap = character.hpCap;
        munitionsCap = character.munitionsCap;
        lifeStealCap = character.lifeStealCap;
        shootIntervalCap = character.shootIntervalCap;
        reloadIntervalCap = character.reloadIntervalCap;
        movementSpeedLimit = character.movementSpeedCap;

        currentMunitions = maxMunitions;
        currentHP = maxHP;

        this.character = character;

        EventsManager.Publish(new PlayerHealthChangedEvent(currentHP));

        FillMunitions();

        EventsManager.Publish(new PlayerStatsChanged(null, 0.0f));
        EventsManager.Publish(new PlayerCharacterLoadedEvent(character));

        Logger.Log("Character " + character.playerName + " loaded");
    }

    private void OnBossDied(BossDiedEvent evt)
    {
        gameWon = true;
    }

    private void FixedUpdate()
    {
        if (gameWon)
        {
            return;
        }

        if (_isEndless)
        {
            var distance = movementSpeed * Time.fixedDeltaTime * movementVec.magnitude;
            _walkedDistance += distance;
        }

        rbd2d.MovePosition(rbd2d.position + movementVec * movementSpeed * Time.fixedDeltaTime);
    }

    public bool GuardianActive => guardianEffect.activeInHierarchy;

    private void Update()
    {
        if (gameWon)
        {
            return;
        }

        shootTimer += Time.deltaTime;
        stepsTimer += Time.deltaTime;

        if (_isEndless)
        {
            _endlessCheckTimer += Time.deltaTime;
            if (_endlessCheckTimer >= endlessWalkedDistanceCheckPeriod)
            {
                _endlessCheckTimer = 0;
                Debug.Log("Checking distance");
                CheckEndlessWalkedDistance();
            }
        }

        if (guardianAngel)
        {
            if (!GuardianActive)
            {
                guardianActivationTimer += Time.deltaTime;
                if (guardianActivationTimer >= guardianInterval)
                {
                    guardianActivationTimer = 0;
                    guardianDurationTimer = 0;

                    if (Random.value <= guardianRate)
                    {
                        audioSource.PlayOneShot(gaActivationClip, 0.3f);
                        guardianEffect.SetActive(true);
                    }
                }
            }
            else
            {
                guardianDurationTimer += Time.deltaTime;
                if (guardianDurationTimer >= guardianDuration)
                {
                    guardianDurationTimer = 0;
                    guardianEffect.SetActive(false);
                }
            }
        }

        if (isReloading || isShooting)
        {
            return;
        }

        if (controls.Gameplay.Fire.IsPressed())
        {
            TryShoot();
            return;
        }

        if (controls.Gameplay.Reload.IsPressed())
        {
            TryReload(true);
            return;
        }

        var controlsMoveVec = controls.Gameplay.Move.ReadValue<Vector2>();

        var x = controlsMoveVec.x;
        var y = controlsMoveVec.y;

#if UNITY_ANDROID || UNITY_IOS
        var joystick = UIManager.Instance.joystickInstance;

        x = joystick.Horizontal;
        y = joystick.Vertical;
#endif

        if (x < -joystickTollerance)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else if (x > joystickTollerance)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }

        movementVec.Set(x, y);
        if (normalizeMovement)
        {
            movementVec.Normalize();
        }

        bool isMoving = movementVec.sqrMagnitude > 0;

        if (isMoving)
        {
            const float minMovementSpeed = 3.0f;
            const float maxMovementSpeed = 8.0f;
            const float minAnimatorSpeed = 0.6f;
            const float speedFactor = minAnimatorSpeed / (maxMovementSpeed - minMovementSpeed);

            var speed = (movementSpeed - minMovementSpeed) * speedFactor + minAnimatorSpeed;
            animator.SetFloat(AnimatorPropMovementSpeed, speed);
        }

        animator.SetBool(AnimPropIsMoving, isMoving);

        if (isMoving && stepsTimer >= stepsSoundInterval)
        {
            stepsTimer = 0;
            if (walkClip != null)
            {
                audioSource.PlayOneShot(walkClip, stepsVolumeScale);
            }
        }

        //        if (isMoving && !audioSource.isPlaying)
        //        {
        //        }
    }

    public void TryShoot()
    {
        if (currentMunitions == 0 || isShooting || isReloading || shootTimer < shootInterval)
        {
            return;
        }

        shootTimer = 0;
        isShooting = true;
        Shoot();
    }

    private void Shoot()
    {
        movementVec.Set(0, 0);
        animator.SetBool(AnimPropIsMoving, false);
        var speed = shootingAnimClip.length / shootInterval;
        animator.SetFloat(AnimPropShootSpeed, speed);
        animator.SetTrigger(AnimTriggerShoot);
    }

    public void CheckShouldReload()
    {
        isShooting = false;
        if (currentMunitions == 0)
        {
            TryReload(false);
        }
    }

    public void TryReload(bool isManual)
    {
        if (!isReloading && currentMunitions < maxMunitions)
        {
            Reload(isManual);
        }
    }

    private void Reload(bool isManual)
    {
        movementVec.Set(0, 0);
        animator.SetBool(AnimPropIsMoving, false);

        if (audioSource != null && reloadClip != null)
        {
            audioSource.PlayOneShot(reloadClip);
        }

        animator.SetTrigger(AnimTriggerReload);

        var baseSpeed = reloadingAnimClip.length / reloadInterval;

        var reloadSpeed = baseSpeed * (isManual ? manualReloadFactor : 1.0f);
        animator.SetFloat(AnimPropReloadSpeed, reloadSpeed);

        isReloading = true;

        var reloadDuration = reloadingAnimClip.length * (1.0f / reloadSpeed);
        EventsManager.Publish(new PlayerReloadStartedEvent(currentMunitions, maxMunitions, reloadDuration));
    }

    private Projectile InstantiateRotatedProjectile(float rotation)
    {
        var projectileInstance = GameManager.Instance.ProjectilesPool.GetObject();
        projectileInstance.transform.SetPositionAndRotation(
            firePoint.position,
            firePoint.rotation * Quaternion.Euler(0, 0, rotation)
        );

        var projectile = projectileInstance.GetComponent<Projectile>();
        projectile.fromPlayer = true;
        projectile.power = projectilePower;
        projectile.canBounce = bouncingShot;

        var useDollars = IsCEOWithBaseSkin;
        projectile.SetAnimationIndex(useDollars ? 3 : 1);

        projectile.AddForce();

        return projectile;
    }

    private void InstantiateTransformedProjectiles()
    {
        (float, float)[] rotationsAndFactors;
        if (diagonalShot)
        {
            rotationsAndFactors = new[]
            {
                (0.0f, 1.0f),
                (45.0f, diagonalShotDamageFactor),
                (-45.0f, diagonalShotDamageFactor)
            };
        }
        else
        {
            rotationsAndFactors = new[] { (0.0f, 1.0f) };
        }

        foreach (var rt in rotationsAndFactors)
        {
            var proj = InstantiateRotatedProjectile(rt.Item1);
            proj.power = Mathf.FloorToInt(proj.power * rt.Item2);
        }

        if (backShot)
        {
            InstantiateRotatedProjectile(-180.0f);
        }

        if (diagonalShot)
        {
            audioSource.PlayOneShot(tripleShotClip);
        }
        else if (doubleShot)
        {
            audioSource.PlayOneShot(doubleShootClip);
        }
        else
        {
            audioSource.PlayOneShot(shootClip);
        }
    }

    public void InstantiateProjectile()
    {
        InstantiateTransformedProjectiles();
        if (doubleShot)
        {
            Invoke(nameof(InstantiateTransformedProjectiles), shootInterval / 5.0f);
        }

        currentMunitions--;
        EventsManager.Publish(
            new PlayerMunitionsChangedEvent(
                currentMunitions,
                maxMunitions
            )
        );
    }

    public void FillMunitions()
    {
        currentMunitions = maxMunitions;
        isReloading = false;

        EventsManager.Publish(
            new PlayerMunitionsChangedEvent(
                currentMunitions,
                maxMunitions
            )
        );
    }

    public void ApplyDamage(int amount, bool overrideGuardianAngel = false)
    {
        if (guardianAngel && GuardianActive && !overrideGuardianAngel)
        {
            audioSource.PlayOneShot(gaHitClip, 0.3f);
            // Damage avoided
            return;
        }

        isShooting = false;
        isReloading = false;

        _endlessCheckTimer = 0.0f;

        Logger.Log("Player hit");

        if (Time.realtimeSinceStartup - lastHitTime >= 1.2f)
        {
            lastHitTime = Time.realtimeSinceStartup;
            animator.SetTrigger(AnimatorTriggerHit);
            if (!IsCEO)
            {
                audioSource.PlayOneShot(hitClip);
            }
        }

        EventsManager.Publish(new PlayerHitEvent(amount));

        currentHP = Mathf.Max(currentHP - amount, 0);

        EventsManager.Publish(new PlayerHealthChangedEvent(currentHP));

        if (currentHP == 0)
        {
            Die();
        }
    }

    public void ApplyLifeSteal(int damageInflicted)
    {
        if (!disableLifeSteal && lifeSteal > 0)
        {
            var healAmount = Mathf.FloorToInt(damageInflicted * lifeSteal);
            Heal(healAmount, false, false);
        }
    }

    public void Heal(int amount, bool showNotification = true, bool canOverflow = true)
    {
        if (currentHP == maxHP && maxHP < hpCap && canOverflow)
        {
            maxHP += hpOverflowAmount;
        }

        currentHP = Mathf.Min(currentHP + amount, maxHP);
        EventsManager.Publish(new PlayerHealthChangedEvent(currentHP));

        if (showNotification && notificationObj != null)
        {
            ShowNotification(DropType.HealthPotion, PowerUpType.IncreaseHealth);
        }

        Logger.Log("Player healed");
    }

    public ISet<PowerUpType> GetUniquePowerUps(bool isEndlessMode = false)
    {
        var powerUps = new HashSet<PowerUpType>();
        powerUps.Add(PowerUpType.GuardianAngel);

        if (diagonalShot)
        {
            powerUps.Add(PowerUpType.DiagonalShot);
        }

        if (doubleShot)
        {
            powerUps.Add(PowerUpType.DoubleShot);
        }

        if (backShot)
        {
            powerUps.Add(PowerUpType.BackShot);
        }

        if (bouncingShot)
        {
            powerUps.Add(PowerUpType.BouncingShot);
        }

        if (disableBounce)
        {
            // Remove BouncingShot from available power ups
            powerUps.Add(PowerUpType.BouncingShot);
        }

        if (movementSpeed >= movementSpeedLimit)
        {
            powerUps.Add(PowerUpType.IncreaseMovementSpeed);
        }

        if (shootInterval <= shootIntervalCap)
        {
            powerUps.Add(PowerUpType.IncreaseShootingSpeed);
        }

        if (reloadInterval <= reloadIntervalCap)
        {
            powerUps.Add(PowerUpType.IncreaseReloadSpeed);
        }

        if (maxHP >= hpCap)
        {
            powerUps.Add(PowerUpType.IncreaseHealth);
        }

        if (maxMunitions >= munitionsCap)
        {
            powerUps.Add(PowerUpType.IncreaseMunitions);
        }

        if (isEndlessMode && projectilePower >= 70)
        {
            powerUps.Add(PowerUpType.IncreasedDamage);
        }

        if (disableLifeSteal || lifeSteal >= lifeStealCap)
        {
            powerUps.Add(PowerUpType.IncreaseLifeSteal);
        }

        return powerUps;
    }

    private void DequeueNotification()
    {
        if (_notificationsQueue.TryDequeue(out var notif))
        {
            ShowNotification(notif.Item1, notif.Item2);
        }
    }

    private void ShowNotification(DropType dropType, PowerUpType powerUpType)
    {
        if (_showingNotification)
        {
            _notificationsQueue.Enqueue((dropType, powerUpType));
            return;
        }

        _showingNotification = true;

        var notifInst = Instantiate(
            notificationObj,
            transform.position,
            Quaternion.identity,
            transform
        );
        notifInst.transform.localScale = notificationScale;

        var playerNotif = notifInst.GetComponent<PlayerNotification>();

        if (dropType == DropType.HealthPotion)
        {
            playerNotif.SetHeal();
        }
        else
        {
            playerNotif.SetPowerUp(powerUpType);
        }

        playerNotif.onComplete.AddListener(() =>
        {
            _showingNotification = false;
            DequeueNotification();
        });
    }

    private void Die()
    {
        Logger.Log("Player died");
        animator.SetTrigger(AnimatorTriggerDie);
        audioSource.PlayOneShot(deathClip);
    }

    private void OnPowerUpUnlocked(PowerUpUnlockedEvent evt)
    {
        Logger.Log($"Power up '{evt.PowerUpType}' unlocked");

        switch (evt.PowerUpType)
        {
            case PowerUpType.DoubleShot:
                doubleShot = true;
                break;
            case PowerUpType.DiagonalShot:
                diagonalShot = true;
                break;
            case PowerUpType.BackShot:
                backShot = true;
                break;
            case PowerUpType.BouncingShot:
                if (!disableBounce)
                {
                    bouncingShot = true;
                }

                break;
            case PowerUpType.IncreasedDamage:
                int inc = damageIncrease;
                if (randomizeDamageIncrease)
                {
                    inc = Random.Range(minDamageIncrease, damageIncrease + 1);
                }

                projectilePower += inc;
                EventsManager.Publish(new PlayerStatsChanged(evt.PowerUpType, projectilePower));
                break;
            case PowerUpType.IncreaseLifeSteal:
                lifeSteal += lifeStealIncrease;
                EventsManager.Publish(new PlayerStatsChanged(evt.PowerUpType, lifeSteal));
                break;
            case PowerUpType.IncreaseHealth:
                maxHP += hpIncrease;
                currentHP += hpIncrease;
                EventsManager.Publish(new PlayerHealthChangedEvent(currentHP));
                break;
            case PowerUpType.IncreaseMunitions:
                maxMunitions++;
                currentMunitions++;
                EventsManager.Publish(new PlayerMunitionsChangedEvent(currentMunitions, maxMunitions));
                break;
            case PowerUpType.IncreaseMovementSpeed:
            {
                movementSpeed = Mathf.Min(movementSpeed + movementSpeedIncrease, movementSpeedLimit);
                EventsManager.Publish(new PlayerStatsChanged(evt.PowerUpType, movementSpeed));
                break;
            }
            case PowerUpType.IncreaseShootingSpeed:
                shootInterval = Mathf.Max(shootInterval - shootIntervalDecrease, shootIntervalCap);
                EventsManager.Publish(new PlayerStatsChanged(evt.PowerUpType, shootInterval));
                break;
            case PowerUpType.IncreaseReloadSpeed:
                reloadInterval = Mathf.Max(reloadInterval - reloadIntervalDecrease, reloadIntervalCap);
                EventsManager.Publish(new PlayerStatsChanged(evt.PowerUpType, reloadInterval));
                break;
            case PowerUpType.GuardianAngel:
                guardianAngel = true;
                break;
        }

        if (notificationObj != null)
        {
            ShowNotification(DropType.PowerUp, evt.PowerUpType);
        }

        if (IsCEOWithBaseSkin && ceoPowerUpClip != null)
        {
            audioSource.PlayOneShot(ceoPowerUpClip);
        }
    }

    private void GameOver()
    {
        gameObject.SetActive(false);

        // Enable camera audio listener instead of the player one
        GameObject.Find("MainCamera").GetComponent<AudioListener>().enabled = true;

        EventsManager.Publish(new PlayerDiedEvent());

        //Destroy(gameObject);
    }

    private void OnInstantDeathAreaEntered(PlayerEnteredInstantDeathAreaEvent evt)
    {
        ApplyDamage(3000, true);
    }
}