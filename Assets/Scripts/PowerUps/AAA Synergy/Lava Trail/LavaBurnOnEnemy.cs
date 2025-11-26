using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class LavaBurnOnEnemy : MonoBehaviour
{
    private EnemyHealth health;
    private Coroutine burnRoutine;

    // VFX
    private GameObject vfxInstance;
    private GameObject burnVfxPrefab;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    /// <summary>
    /// Versión antigua: sin VFX. La dejo para no romper nada.
    /// </summary>
    public void StartBurn(float damagePerSecond, float duration)
    {
        StartBurn(damagePerSecond, duration, null);
    }

    /// <summary>
    /// Versión nueva: con VFX mientras dure la quemadura.
    /// </summary>
    public void StartBurn(float damagePerSecond, float duration, GameObject vfxPrefab)
    {
        burnVfxPrefab = vfxPrefab;

        if (health == null)
        {
            health = GetComponent<EnemyHealth>();
            if (health == null) return;
        }

        // Si ya estaba quemándose, reiniciamos la rutina,
        // pero NO destruimos el VFX (se mantiene hasta que termine la última quemadura).
        if (burnRoutine != null)
            StopCoroutine(burnRoutine);

        burnRoutine = StartCoroutine(BurnRoutine(damagePerSecond, duration));
    }

    private IEnumerator BurnRoutine(float dps, float duration)
    {
        float elapsed = 0f;

        // Si tenemos prefab y todavía no hay VFX, lo creamos
        if (burnVfxPrefab != null && vfxInstance == null)
        {
            vfxInstance = Instantiate(
                burnVfxPrefab,
                transform.position,
                Quaternion.identity,
                transform // se pega al enemigo
            );
        }

        while (elapsed < duration && health != null && health.GetCurrentHealth() > 0)
        {
            int damage = Mathf.CeilToInt(dps);
            health.TakeDamage(damage, Vector2.zero, 0f);

            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }

        // Cuando esta quemadura termina, paramos la rutina.
        burnRoutine = null;


        if (burnRoutine == null && vfxInstance != null)
        {
            Destroy(vfxInstance);
            vfxInstance = null;
        }
    }

    private void OnDisable()
    {
        // Por seguridad, si el enemigo se destruye o desactiva, limpiamos el VFX
        if (vfxInstance != null)
        {
            Destroy(vfxInstance);
            vfxInstance = null;
        }
    }
}
