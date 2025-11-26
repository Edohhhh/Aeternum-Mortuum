using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerLavaTrail : MonoBehaviour
{
    [Header("Quemadura")]
    [Tooltip("Daño por segundo que aplica la quemadura")]
    public float burnDamagePerSecond = 2f;

    [Tooltip("Duración de la quemadura en segundos")]
    public float burnDuration = 5f;

    [Header("Charco")]
    [SerializeField]
    [Tooltip("Cuánto dura este charco de lava en el mundo")]
    private float lifetime = 3f;

    private void Awake()
    {
        // Aseguramos que el collider sea trigger
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

        // Buscar o agregar el componente de quemadura en el enemigo
        var burn = other.GetComponent<LavaBurnOnEnemy>();
        if (burn == null)
            burn = other.gameObject.AddComponent<LavaBurnOnEnemy>();

        // Iniciar / refrescar quemadura
        burn.StartBurn(burnDamagePerSecond, burnDuration);
    }
}
