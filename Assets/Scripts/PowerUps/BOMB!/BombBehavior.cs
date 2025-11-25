using UnityEngine;

public class BombBehavior : MonoBehaviour
{
    [Header("Bomb Settings")]
    public float explosionDelay = 3f;
    public float explosionRadius = 2f;
    public float damage = 5f;

    [Header("Prefab al explotar")]
    public GameObject prefabAlExplotar;   // ← Asignalo desde el inspector
    public Transform spawnPoint;          // Opcional, si querés otro punto de spawn

    private void Start()
    {
        Invoke(nameof(Explode), explosionDelay);
    }

    private void Explode()
    {
        // --- Daño a enemigos ---
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                var enemy = hit.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage((int)damage, Vector2.zero, 0f);
                }
            }
        }

        // --- Instanciar prefab ---
        if (prefabAlExplotar != null)
        {
            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
            Instantiate(prefabAlExplotar, pos, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
