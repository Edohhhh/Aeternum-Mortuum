using UnityEngine;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "SlowOnHitPowerUp", menuName = "PowerUps/Slow On Any Damage")]
public class SlowOnHitPowerUp : PowerUp
{
    [Header("Slow Settings")]
    public float slowPercent = 0.05f;
    public float Slowduration = 2f;

    [Header("VFX")]
    public GameObject slowEffectPrefab;

    public override void Apply(PlayerController player)
    {
        // Buscar o crear el observer
        var observer = Object.FindAnyObjectByType<GlobalEnemyDamageObserver>();
        if (observer == null)
        {
            var go = new GameObject("GlobalEnemyDamageObserver");
            observer = go.AddComponent<GlobalEnemyDamageObserver>();
            Object.DontDestroyOnLoad(go);
        }

        // Configurar parámetros del slow y el prefab
        observer.slowPercent = slowPercent;
        observer.duration = Slowduration;
        observer.slowEffectPrefab = slowEffectPrefab;

        // Asegurarnos que esté activo cuando se aplica la perk
        observer.isActive = true;

        // Agregar hook a todos los enemigos activos (primera pasada)
        observer.AttachHooksToExistingEnemies();
    }

    public override void Remove(PlayerController player)
    {
        var observer = Object.FindAnyObjectByType<GlobalEnemyDamageObserver>();
        if (observer != null)
        {
            observer.DisableSlowGlobally();
        }
    }
}
