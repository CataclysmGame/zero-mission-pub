using UnityEngine;

public class BossFlameController : MonoBehaviour
{
    public BossController boss;

    public float tickTime = 1.0f;

    private float damageTimer = 0f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (damageTimer < tickTime)
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
            var player = collision.GetComponent<PlayerController>();
            player.ApplyDamage(boss.flameDamage);
            damageTimer = 0f;
        }
    }

    private void Update()
    {
        damageTimer += Time.deltaTime;
    }
}
