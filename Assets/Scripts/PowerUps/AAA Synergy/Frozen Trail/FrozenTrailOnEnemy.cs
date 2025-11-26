using UnityEngine;
using System.Collections;
using System.Reflection;

[DisallowMultipleComponent]
public class FrozenTrailOnEnemy : MonoBehaviour
{
    private MonoBehaviour controller;
    private FieldInfo maxSpeedField;
    private float originalSpeed;
    private Coroutine slowRoutine;

    // VFX
    private GameObject slowVfxPrefab;
    private GameObject vfxInstance;

    private void Awake()
    {
        controller = GetComponent<MonoBehaviour>();
        if (controller == null) return;

        var t = controller.GetType();
        maxSpeedField = t.GetField("maxSpeed",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (maxSpeedField == null || maxSpeedField.FieldType != typeof(float))
        {
            Debug.LogWarning($"[FrozenTrail] {gameObject.name} no tiene campo 'maxSpeed' accesible.");
            controller = null;
            maxSpeedField = null;
            return;
        }

        originalSpeed = (float)maxSpeedField.GetValue(controller);
    }

    // Versión vieja (por compatibilidad), sin VFX explícito
    public void ApplySlow(float slowPercent, float duration)
    {
        ApplySlow(slowPercent, duration, null);
    }

    // Versión nueva: con VFX mientras dure el slow
    public void ApplySlow(float slowPercent, float duration, GameObject vfxPrefab)
    {
        if (controller == null || maxSpeedField == null) return;

        if (vfxPrefab != null)
            slowVfxPrefab = vfxPrefab;

        // Si ya hay un slow corriendo, lo reiniciamos
        if (slowRoutine != null)
            StopCoroutine(slowRoutine);

        slowRoutine = StartCoroutine(SlowRoutine(slowPercent, duration));
    }

    private IEnumerator SlowRoutine(float slowPercent, float duration)
    {
        // Siempre volver al original antes de recalcular
        maxSpeedField.SetValue(controller, originalSpeed);

        float slowedSpeed = originalSpeed * (1f - Mathf.Clamp01(slowPercent));
        maxSpeedField.SetValue(controller, slowedSpeed);

        // Crear VFX solo si tenemos prefab y todavía no hay VFX
        if (slowVfxPrefab != null && vfxInstance == null)
        {
            vfxInstance = Instantiate(
                slowVfxPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );
            vfxInstance.transform.localPosition = Vector3.zero;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Restaurar velocidad original
        maxSpeedField.SetValue(controller, originalSpeed);

        // Destruir VFX
        if (vfxInstance != null)
        {
            Destroy(vfxInstance);
            vfxInstance = null;
        }

        slowRoutine = null;
    }

    private void OnDisable()
    {
        // Seguridad extra si el enemigo se destruye estando ralentizado
        if (controller != null && maxSpeedField != null)
        {
            maxSpeedField.SetValue(controller, originalSpeed);
        }

        if (vfxInstance != null)
        {
            Destroy(vfxInstance);
            vfxInstance = null;
        }

        slowRoutine = null;
    }
}
