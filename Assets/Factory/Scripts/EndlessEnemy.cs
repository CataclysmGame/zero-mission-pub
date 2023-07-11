using UnityEngine;

public class EndlessEnemy : MonoBehaviour
{
    [Tooltip("How long the enemy will stay in one place before switching targets")]
    public float targetSwitchTime = 10.0f;

    public float switchTargetMapPercentage = 0.3f;

    [SerializeField] private IncreaseFunction _damageIncreaseFunction;

    [SerializeField] private IncreaseFunction _hpIncreaseFunction;

    private EnemyController _enemy;

    private void Awake()
    {
        _enemy = GetComponent<EnemyController>();
    }

    private void Start()
    {
        if (_damageIncreaseFunction == null || _hpIncreaseFunction == null)
        {
            Logger.LogError(
                "Someone forgot to assign the increase functions to the EndlessEnemy component. Their HP and damage won't be updated");
            return;
        }

        if (_enemy)
        {
            _enemy.projectilePower = _damageIncreaseFunction.GetNewIntValue(EndlessController.Instance.EnemiesDefeated);
            _enemy.maxHP = _hpIncreaseFunction.GetNewIntValue(EndlessController.Instance.EnemiesDefeated);
            _enemy.currentHP = _enemy.maxHP;
            Debug.Log("<color=lightblue> Spawned enemy with hp " + _enemy.currentHP + "</color>");
        }

        var boss = GetComponent<BossController>();
        if (boss)
        {
            boss.projectilePower = _damageIncreaseFunction.GetNewIntValue(EndlessController.Instance.EnemiesDefeated);
            boss.maxHP = _hpIncreaseFunction.GetNewIntValue(EndlessController.Instance.EnemiesDefeated);
            boss.currentHP = boss.maxHP;
            Debug.Log("<color=lightblue> Spawned boss with hp " + boss.currentHP + "</color>");
        }

        InvokeRepeating(nameof(SwitchTarget), targetSwitchTime, targetSwitchTime);
    }

    private void SwitchTarget()
    {
        var randomTargets = EndlessController.Instance.randomMapTargets;

        if (randomTargets == null || randomTargets.Length == 0) return;

        if (Random.value < switchTargetMapPercentage)
        {
            var randomTarget = Util.PickRandom(randomTargets);
            _enemy.targetTransform = randomTarget;
        }
        else
        {
            _enemy.targetTransform = GameManager.Instance.Player.transform;
        }
    }
}