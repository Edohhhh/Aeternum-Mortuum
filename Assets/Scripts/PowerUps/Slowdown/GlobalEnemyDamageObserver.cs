using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;

public class GlobalEnemyDamageObserver : MonoBehaviour
{
    [Header("Slow Settings")]
    public float slowPercent = 0.05f;
    public float duration = 2f;

    [Header("VFX")]
    public GameObject slowEffectPrefab; // <-- referencia al prefab

    // Cada enemigo que tiene slow activo tiene una corrutina asociada
    private Dictionary<GameObject, Coroutine> slowCoroutines = new();

    [Header("Auto Hook")]
    [Tooltip("Cada cuántos segundos se escanean enemigos nuevos para agregarles EnemyDamageHook.")]
    public float rescanInterval = 0.5f;
    private float rescanTimer = 0f;

    // 🔥 Nuevo: flag para saber si el slow está activo o no
    [HideInInspector] public bool isActive = true;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // 👇 Si no está activo, no escaneamos ni hacemos nada
        if (!isActive) return;

        rescanTimer -= Time.deltaTime;
        if (rescanTimer <= 0f)
        {
            rescanTimer = rescanInterval;
            AttachHooksToExistingEnemies();
        }
    }

    /// <summary>
    /// Llamado por EnemyDamageHook cuando un enemigo recibe daño.
    /// </summary>
    public static void RegisterDamage(GameObject enemyObject)
    {
        var observer = Object.FindAnyObjectByType<GlobalEnemyDamageObserver>();
        if (observer == null) return;

        // 👇 Si está desactivado, ignoramos el daño
        if (!observer.isActive) return;

        if (enemyObject != null && enemyObject.CompareTag("Enemy"))
        {
            observer.TryApplySlow(enemyObject);
        }
    }

    /// <summary>
    /// Recorre todos los enemigos actuales y les agrega EnemyDamageHook si tienen EnemyHealth.
    /// Lo usamos tanto desde la perk al aplicarse como en el auto-scan.
    /// </summary>
    public void AttachHooksToExistingEnemies()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            if (enemy.GetComponent<EnemyDamageHook>() == null &&
                enemy.GetComponent<EnemyHealth>() != null)
            {
                enemy.AddComponent<EnemyDamageHook>();
            }
        }
    }

    private void TryApplySlow(GameObject enemy)
    {
        if (enemy == null) return;

        // Si ya está ralentizado, no lo aplicamos de nuevo
        if (slowCoroutines.ContainsKey(enemy)) return;

        Coroutine routine = StartCoroutine(ApplySlowViaReflection(enemy));
        slowCoroutines[enemy] = routine;
    }

    private IEnumerator ApplySlowViaReflection(GameObject enemy)
    {
        if (enemy == null) yield break;

        var controller = enemy.GetComponent<MonoBehaviour>();
        if (controller == null)
        {
            yield break;
        }

        var maxSpeedField = controller.GetType().GetField(
            "maxSpeed",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        );

        if (maxSpeedField == null || maxSpeedField.FieldType != typeof(float))
        {
            Debug.LogWarning($"[Slow] {enemy.name} no tiene 'maxSpeed' accesible por reflexión.");
            yield break;
        }

        float originalSpeed = (float)maxSpeedField.GetValue(controller);
        float slowedSpeed = originalSpeed * (1f - slowPercent);
        maxSpeedField.SetValue(controller, slowedSpeed);
        Debug.Log($"[Slow] {enemy.name} ralentizado a {slowedSpeed}");

        // VFX parentado al enemigo
        GameObject vfxInstance = null;
        if (slowEffectPrefab != null && enemy != null)
        {
            vfxInstance = Instantiate(
                slowEffectPrefab,
                enemy.transform.position,
                Quaternion.identity,
                enemy.transform
            );
        }

        // Mantener el slow por la duración
        yield return new WaitForSeconds(duration);

        // Restaurar velocidad
        if (enemy != null && maxSpeedField != null)
        {
            maxSpeedField.SetValue(controller, originalSpeed);
            Debug.Log($"[Slow] {enemy.name} restaurado a {originalSpeed}");
        }

        // Destruir el VFX si sigue existiendo
        if (vfxInstance != null)
        {
            Destroy(vfxInstance);
        }

        slowCoroutines.Remove(enemy);
    }

    //apagar el efecto globalmente (no más slows nuevos)
    public void DisableSlowGlobally()
    {
        isActive = false;
        Debug.Log("[GlobalEnemyDamageObserver] Slow desactivado globalmente por perk removida.");
        // Dejamos que las corutinas actuales terminen solas,
        // así no rompemos estados intermedios.
    }
}
