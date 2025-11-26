using System.Linq;
using UnityEngine;

public class SpectralBullet : MonoBehaviour
{
    [Header("Velocidad y daño")]
    public float speed = 5f;
    [Tooltip("Daño infligido al enemigo")]
    public int damage = 1;

    [Header("Rotación")]
    [Tooltip("Si el sprite de la bala apunta hacia la derecha (+X), dejá true. Si apunta hacia arriba (+Y), marcá false.")]
    public bool spriteFacesRight = true;

    [Tooltip("Si > 0, la rotación será suave; si 0, será instantánea.")]
    public float rotationSpeed = 0f;

    private Transform target;

    void Start()
    {
        FindClosestEnemy();

        if (target == null)
        {
            Destroy(gameObject);
        }
        else
        {
            RotateToTargetInstant();
        }
    }

    private void FindClosestEnemy()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (var e in enemies)
        {
            if (e == null) continue;

            float dist = Vector2.Distance(transform.position, e.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = e.transform;
            }
        }

        target = closest;
    }

    void Update()
    {
        if (target == null)
        {
            FindClosestEnemy();
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }
        }

        Vector3 dir = (target.position - transform.position).normalized;

        float targetAngleDeg;
        if (spriteFacesRight)
            targetAngleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        else
            targetAngleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

        Quaternion targetRot = Quaternion.Euler(0f, 0f, targetAngleDeg);

        if (rotationSpeed <= 0f)
            transform.rotation = targetRot;
        else
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 360f * Time.deltaTime / Mathf.Max(0.0001f, rotationSpeed));

        transform.position += (Vector3)dir * speed * Time.deltaTime;

#if UNITY_EDITOR
        Debug.DrawLine(transform.position, target.position, Color.cyan);
#endif
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy"))
            return;

        var enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
            enemy.TakeDamage(damage, knockbackDir, 5f);
        }

        Destroy(gameObject);
    }

    private void RotateToTargetInstant()
    {
        if (target == null) return;

        Vector3 dir = (target.position - transform.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (!spriteFacesRight) angle -= 90f;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
