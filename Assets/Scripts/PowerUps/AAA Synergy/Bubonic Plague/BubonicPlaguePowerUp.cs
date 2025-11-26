using UnityEngine;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "BubonicPlaguePowerUp", menuName = "PowerUps/Bubonic Plague")]
public class BubonicPlaguePowerUp : PowerUp
{
    [Header("Prefabs")]
    [Tooltip("Prefab del rastro de slime (puede ser el mismo que usás en Acid Trail).")]
    public GameObject slimeTrailPrefab;

    [Tooltip("Prefab de la nube de gas venenoso (ej: AcidCloud).")]
    public GameObject poisonCloudPrefab;

    [Header("Spawn")]
    [Tooltip("Intervalo entre spawns mientras el jugador se está moviendo (en segundos).")]
    public float spawnInterval = 3f;

    private GameObject observerInstance;

    public override void Apply(PlayerController player)
    {
        if (player == null) return;

        // Evitar múltiples observers para la misma perk
        if (observerInstance != null)
            return;

        // Crear el observer global / persistente
        observerInstance = new GameObject("BubonicPlagueObserver");
        var observer = observerInstance.AddComponent<BubonicPlagueObserver>();

        observer.Initialize(
            player.transform,
            slimeTrailPrefab,
            poisonCloudPrefab,
            spawnInterval
        );

        Object.DontDestroyOnLoad(observerInstance);
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
