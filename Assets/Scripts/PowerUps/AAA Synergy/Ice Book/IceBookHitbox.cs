using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class IceBookHitbox : MonoBehaviour
{
    [Header("Daño")]
    public float damagePerSecond = 3f;
    public float tickInterval = 0.2f;

    [Header("Freeze")]
    [Tooltip("Duración del congelamiento en segundos.")]
    public float freezeDuration = 2f;

    private readonly HashSet<EnemyHealth> enemiesInside = new();
    private Vector2 currentDirection = Vector2.right;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnEnable()
    {
        StartCoroutine(DamageLoop());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        enemiesInside.Clear();
    }

    public void SetDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude > 0.0001f)
            currentDirection = dir.normalized;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        var enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
            enemiesInside.Add(enemy);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        var enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
            enemiesInside.Remove(enemy);
    }

    private IEnumerator DamageLoop()
    {
        while (true)
        {
            int damage = Mathf.CeilToInt(damagePerSecond * tickInterval);

            foreach (var enemy in enemiesInside)
            {
                if (enemy == null) continue;

                // Daño directo por estar en el cono de hielo
                enemy.TakeDamage(damage, currentDirection, 0f);

                // Congelar o refrescar congelación
                var freeze = enemy.GetComponent<IceBookFreezeOnEnemy>();
                if (freeze == null)
                    freeze = enemy.gameObject.AddComponent<IceBookFreezeOnEnemy>();

                freeze.StartFreeze(freezeDuration);
            }

            yield return new WaitForSeconds(tickInterval);
        }
    }
}
