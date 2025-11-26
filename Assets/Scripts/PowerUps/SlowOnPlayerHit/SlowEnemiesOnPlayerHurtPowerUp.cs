using UnityEngine;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "SlowEnemiesOnPlayerHurt", menuName = "PowerUps/Slow Enemies On Player Hurt")]
public class SlowEnemiesOnPlayerHurtPowerUp : PowerUp
{
    [Header("Slow Config")]
    [Tooltip("Porcentaje de slow (0.1 = 10% más lento).")]
    public float slowPercent = 0.1f;

    [Tooltip("Duración del slow en segundos.")]
    public float Powerduration = 2f;

    [Header("VFX en ENEMIGOS")]
    [Tooltip("Partícula de congelamiento que se mostrará en cada enemigo ralentizado.")]
    public GameObject slowEffectPrefab;

    private GameObject observerInstance;

    public override void Apply(PlayerController player)
    {
        if (observerInstance == null)
        {
            observerInstance = new GameObject("PlayerHurtSlowObserver");
            var observer = observerInstance.AddComponent<PlayerHurtSlowObserver>();
            observer.slowPercent = slowPercent;
            observer.duration = Powerduration;
            observer.enemyEffectPrefab = slowEffectPrefab;

            observer.AttachTo(player);
            Object.DontDestroyOnLoad(observerInstance);
        }
        else
        {
            var observer = observerInstance.GetComponent<PlayerHurtSlowObserver>();
            if (observer != null)
            {
                observer.slowPercent = slowPercent;
                observer.duration = Powerduration;
                observer.enemyEffectPrefab = slowEffectPrefab;
                observer.AttachTo(player);
            }
        }
    }

    public override void Remove(PlayerController player)
    {
        if (observerInstance != null)
        {
            Object.Destroy(observerInstance);
            observerInstance = null;
        }
    }
}
