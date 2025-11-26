using UnityEngine;
using System.Collections;
using System.Reflection;

[DisallowMultipleComponent]
public class IceBookFreezeOnEnemy : MonoBehaviour
{
    private MonoBehaviour controllerWithSpeed;
    private FieldInfo maxSpeedField;
    private float originalSpeed;
    private Coroutine freezeRoutine;

    private void Awake()
    {
        // Buscar algún componente con campo 'maxSpeed' float (igual que tus otros slows)
        var comps = GetComponents<MonoBehaviour>();
        foreach (var c in comps)
        {
            if (c == null) continue;

            var t = c.GetType();
            var f = t.GetField("maxSpeed",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (f != null && f.FieldType == typeof(float))
            {
                controllerWithSpeed = c;
                maxSpeedField = f;
                originalSpeed = (float)maxSpeedField.GetValue(controllerWithSpeed);
                break;
            }
        }

        if (controllerWithSpeed == null || maxSpeedField == null)
        {
            Debug.LogWarning($"[IceBook] {gameObject.name} no tiene campo 'maxSpeed' accesible para congelar.");
        }
    }

    public void StartFreeze(float duration)
    {
        if (controllerWithSpeed == null || maxSpeedField == null) return;

        if (freezeRoutine != null)
            StopCoroutine(freezeRoutine);

        freezeRoutine = StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        // Reset al valor original antes de congelar (por si había otros efectos)
        maxSpeedField.SetValue(controllerWithSpeed, originalSpeed);

        // Congelar: velocidad = 0
        maxSpeedField.SetValue(controllerWithSpeed, 0f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Restaurar velocidad original
        maxSpeedField.SetValue(controllerWithSpeed, originalSpeed);
        freezeRoutine = null;
    }
}
