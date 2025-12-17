using UnityEngine;

[DisallowMultipleComponent]
public class LaRosaHitTracker : MonoBehaviour
{
    public EnemyHealth LastEnemy { get; private set; }

    private void OnTriggerEnter2D(Collider2D other) => CacheEnemy(other);
    private void OnTriggerStay2D(Collider2D other) => CacheEnemy(other);
    private void OnCollisionEnter2D(Collision2D collision) => CacheEnemy(collision.collider);
    private void OnCollisionStay2D(Collision2D collision) => CacheEnemy(collision.collider);

    private void CacheEnemy(Collider2D col)
    {
        if (col == null) return;
        if (!col.CompareTag("Enemy")) return;

        var eh = col.GetComponentInParent<EnemyHealth>();
        if (eh != null)
            LastEnemy = eh;
    }
}
