using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class LavaBurnOnEnemy : MonoBehaviour
{
    private EnemyHealth health;
    private Coroutine burnRoutine;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    public void StartBurn(float damagePerSecond, float duration)
    {
        if (health == null)
        {
            health = GetComponent<EnemyHealth>();
            if (health == null) return;
        }

        // Si ya hay una quemadura activa, la reiniciamos
        if (burnRoutine != null)
            StopCoroutine(burnRoutine);

        burnRoutine = StartCoroutine(BurnRoutine(damagePerSecond, duration));
    }

    private IEnumerator BurnRoutine(float dps, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration && health != null && health.GetCurrentHealth() > 0)
        {
            int damage = Mathf.CeilToInt(dps);
            // Knockback 0, dirección cero
            health.TakeDamage(damage, Vector2.zero, 0f);

            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }

        burnRoutine = null;
    }
}
