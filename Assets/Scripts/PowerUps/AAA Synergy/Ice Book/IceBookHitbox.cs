using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class IceBookHitbox : MonoBehaviour
{
    [Header("Congelación")]
    public float freezeDuration = 2f;
    public GameObject freezeVfxPrefab;

    [Header("Tick de aplicación")]
    [Tooltip("Cada cuántos segundos se vuelve a intentar aplicar congelación mientras el enemigo esté dentro.")]
    public float tickInterval = 0.2f;

    [Header("Vida del hielo")]
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
        StartCoroutine(FreezeLoop());

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

    private IEnumerator FreezeLoop()
    {
        while (true)
        {
            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;

                var freeze = enemy.GetComponent<IceBookFreezeOnEnemy>();
                if (freeze == null)
                    freeze = enemy.gameObject.AddComponent<IceBookFreezeOnEnemy>();

                freeze.StartFreeze(freezeDuration, freezeVfxPrefab);
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
