using UnityEngine;
using System.Collections;
using System.Reflection;

[DisallowMultipleComponent]
public class IceBookFreezeOnEnemy : MonoBehaviour
{
    [Header("Freeze")]
    public float freezeDuration = 2f;

    [Header("VFX")]
    public GameObject iceEffectPrefab;

    private Coroutine freezeRoutine;

    // Cache del controller y su campo maxSpeed
    private MonoBehaviour movementController;
    private FieldInfo maxSpeedField;

    private float originalSpeed;
    private bool isFrozen = false;
    private GameObject vfxInstance;

    public void StartFreeze(float duration, GameObject vfxPrefab)
    {
        freezeDuration = duration;
        iceEffectPrefab = vfxPrefab;

        if (!TryResolveMovementController())
            return;

        // Si NO estaba congelado aún, congelar ahora
        if (!isFrozen)
        {
            originalSpeed = (float)maxSpeedField.GetValue(movementController);
            maxSpeedField.SetValue(movementController, 0f);
            isFrozen = true;

            // Crear VFX
            if (iceEffectPrefab != null && vfxInstance == null)
            {
                vfxInstance = Instantiate(
                    iceEffectPrefab,
                    transform.position,
                    Quaternion.identity,
                    transform
                );
            }
        }

        // Reiniciar timer de congelación (refrescar duración)
        if (freezeRoutine != null)
            StopCoroutine(freezeRoutine);

        freezeRoutine = StartCoroutine(FreezeTimer());
    }

    private bool TryResolveMovementController()
    {
        if (movementController != null && maxSpeedField != null)
            return true;

        var controller = GetComponent<MonoBehaviour>();
        if (controller == null)
            return false;

        var field = controller.GetType().GetField(
            "maxSpeed",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        );

        if (field == null || field.FieldType != typeof(float))
        {
            Debug.LogWarning($"[IceBook] {name} no tiene 'maxSpeed' accesible por reflexión.");
            return false;
        }

        movementController = controller;
        maxSpeedField = field;
        return true;
    }

    private IEnumerator FreezeTimer()
    {
        float t = 0f;
        while (t < freezeDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        Unfreeze();
    }

    private void Unfreeze()
    {
        if (!isFrozen) return;

        if (movementController != null && maxSpeedField != null)
        {
            maxSpeedField.SetValue(movementController, originalSpeed);
        }

        if (vfxInstance != null)
        {
            Destroy(vfxInstance);
            vfxInstance = null;
        }

        isFrozen = false;
        freezeRoutine = null;
    }

    private void OnDisable()
    {
        // Por si el enemigo se destruye/desactiva congelado, restaurar
        if (isFrozen && movementController != null && maxSpeedField != null)
        {
            maxSpeedField.SetValue(movementController, originalSpeed);
        }

        if (vfxInstance != null)
        {
            Destroy(vfxInstance);
            vfxInstance = null;
        }

        isFrozen = false;
    }
}
