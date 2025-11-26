using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerFrozenTrail : MonoBehaviour
{
    [Header("Slow")]
    [Tooltip("Porcentaje de slow (0.2 = 20%)")]
    public float slowPercent = 0.2f;

    [Tooltip("Duración del slow en segundos")]
    public float slowDuration = 2f;

    [Header("Partículas de hielo")]
    [Tooltip("Prefab de partículas que se aplica en los enemigos ralentizados")]
    public GameObject slowVfxPrefab;

    [Header("Charco")]
    [SerializeField]
    [Tooltip("Cuánto dura este charco de hielo en el mundo")]
    private float lifetime = 3f;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        var slowComp = other.GetComponent<FrozenTrailOnEnemy>();
        if (slowComp == null)
            slowComp = other.gameObject.AddComponent<FrozenTrailOnEnemy>();

        // ⬇️ Ahora pasa también el prefab de partículas
        slowComp.ApplySlow(slowPercent, slowDuration, slowVfxPrefab);
    }
}
