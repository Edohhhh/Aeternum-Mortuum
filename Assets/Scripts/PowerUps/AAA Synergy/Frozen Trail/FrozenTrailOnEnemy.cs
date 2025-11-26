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

    public void ApplySlow(float slowPercent, float duration)
    {
        if (controller == null || maxSpeedField == null) return;

        // Si ya hay un slow corriendo, lo reiniciamos
        if (slowRoutine != null)
            StopCoroutine(slowRoutine);

        slowRoutine = StartCoroutine(SlowRoutine(slowPercent, duration));
    }

    private IEnumerator SlowRoutine(float slowPercent, float duration)
    {
        // Por seguridad, siempre volvemos al original antes de recalcular
        maxSpeedField.SetValue(controller, originalSpeed);

        float slowedSpeed = originalSpeed * (1f - Mathf.Clamp01(slowPercent));
        maxSpeedField.SetValue(controller, slowedSpeed);
        // Debug.Log($"[FrozenTrail] {gameObject.name} slowed to {slowedSpeed}");

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Restaurar velocidad original
        maxSpeedField.SetValue(controller, originalSpeed);

        slowRoutine = null;
    }
}
