using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class FireBookHitbox : MonoBehaviour
{
    [Header("Daño directo")]
    public float damagePerSecond = 4f;
    public float tickInterval = 0.2f;

    [Header("Quemadura")]
    public float burnDamagePerSecond = 2f;
    public float burnDuration = 5f;

    [Header("Vida de la llama")]
    public float lifeTime = 1.5f;

    private HashSet<EnemyHealth> enemies = new();
    private Vector2 direction = Vector2.right;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnEnable()
    {
        StartCoroutine(DamageLoop());

        if (lifeTime > 0f)
            StartCoroutine(LifeRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        enemies.Clear();
    }

    public void SetDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude > 0.01f)
            direction = dir.normalized;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        var hp = other.GetComponent<EnemyHealth>();
        if (hp != null) enemies.Add(hp);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        var hp = other.GetComponent<EnemyHealth>();
        if (hp != null) enemies.Remove(hp);
    }

    private IEnumerator DamageLoop()
    {
        while (true)
        {
            int damage = Mathf.CeilToInt(damagePerSecond * tickInterval);

            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;

                enemy.TakeDamage(damage, direction, 0f);

                var burn = enemy.GetComponent<FireBookBurnOnEnemy>();
                if (burn == null)
                    burn = enemy.gameObject.AddComponent<FireBookBurnOnEnemy>();

                burn.StartBurn(burnDamagePerSecond, burnDuration);
            }

            yield return new WaitForSeconds(tickInterval);
        }
    }

    private IEnumerator LifeRoutine()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }
}