using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;

public class PlayerHurtSlowObserver : MonoBehaviour
{
    [Header("Slow Config")]
    public float slowPercent = 0.1f;
    public float duration = 2f;

    [Header("VFX en ENEMIGOS")]
    [Tooltip("Prefab de partículas que se instancia en cada enemigo ralentizado.")]
    public GameObject enemyEffectPrefab;

    // Enemigos actualmente ralentizados (cada uno con su corutina)
    private readonly Dictionary<GameObject, Coroutine> activeSlows = new();

    private PlayerController player;
    private PlayerDamageHook damageHook;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void AttachTo(PlayerController pc)
    {
        player = pc;
        if (player == null) return;

        damageHook = player.GetComponent<PlayerDamageHook>();
        if (damageHook == null)
            damageHook = player.gameObject.AddComponent<PlayerDamageHook>();

        // Evitar dobles suscripciones
        damageHook.OnPlayerDamaged -= OnPlayerDamaged;
        damageHook.OnPlayerDamaged += OnPlayerDamaged;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(RebindAfterSceneLoad());
    }

    private IEnumerator RebindAfterSceneLoad()
    {
        PlayerController found = null;
        while (found == null)
        {
            found = Object.FindFirstObjectByType<PlayerController>();
            yield return null;
        }

        AttachTo(found);
    }

    public void OnPlayerDamaged()
    {
        // Ralentizar a TODOS los enemigos actuales
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (enemy == null) continue;

            if (!activeSlows.ContainsKey(enemy))
            {
                Coroutine slowRoutine = StartCoroutine(ApplySlowTo(enemy));
                activeSlows.Add(enemy, slowRoutine);
            }
        }
    }

    private IEnumerator ApplySlowTo(GameObject enemy)
    {
        if (enemy == null)
        {
            yield break;
        }

        var controller = enemy.GetComponent<MonoBehaviour>();
        if (controller == null) yield break;

        FieldInfo maxSpeedField = controller.GetType().GetField(
            "maxSpeed",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        );
        if (maxSpeedField == null || maxSpeedField.FieldType != typeof(float)) yield break;

        float originalSpeed = (float)maxSpeedField.GetValue(controller);
        float slowedSpeed = originalSpeed * (1f - slowPercent);
        maxSpeedField.SetValue(controller, slowedSpeed);

        Debug.Log($"[SLOW-PLAYER] {enemy.name} ralentizado por daño al jugador.");

        GameObject vfxInstance = null;
        if (enemyEffectPrefab != null && enemy != null)
        {
            vfxInstance = Instantiate(
                enemyEffectPrefab,
                enemy.transform.position,
                Quaternion.identity,
                enemy.transform
            );
        }

        yield return new WaitForSeconds(duration);

        // Restaurar velocidad
        if (enemy != null && maxSpeedField != null)
        {
            maxSpeedField.SetValue(controller, originalSpeed);
        }

        // Destruir VFX si sigue existiendo
        if (vfxInstance != null)
        {
            Destroy(vfxInstance);
        }

        activeSlows.Remove(enemy);
    }
}
