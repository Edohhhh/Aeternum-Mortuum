using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerLavaTrail : MonoBehaviour
{
    [Header("Quemadura")]
    [Tooltip("Daño por segundo que aplica la quemadura")]
    public float burnDamagePerSecond = 2f;

    [Tooltip("Duración de la quemadura en segundos")]
    public float burnDuration = 5f;

    [Header("Partículas de quemadura")]
    [Tooltip("Prefab de partículas que se mostrará en los enemigos quemados")]
    public GameObject burnVfxPrefab;

    [Header("Charco")]
    [SerializeField]
    [Tooltip("Cuánto dura este charco de lava en el mundo")]
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

        var health = other.GetComponent<EnemyHealth>();
        if (health == null) return;

        var burn = other.GetComponent<LavaBurnOnEnemy>();
        if (burn == null)
            burn = other.gameObject.AddComponent<LavaBurnOnEnemy>();

        // 🔥 Ahora le pasamos también el prefab de partículas
        burn.StartBurn(burnDamagePerSecond, burnDuration, burnVfxPrefab);
    }
}
