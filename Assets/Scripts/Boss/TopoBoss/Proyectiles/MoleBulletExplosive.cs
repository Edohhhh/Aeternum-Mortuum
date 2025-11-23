using UnityEngine;

public class MoleBulletExplosive : MonoBehaviour
{
    private Vector2 dir;
    private float speed;
    private GameObject childPrefab;
    private float childSpeed;
    private float childDamage;
    private LayerMask playerMask;

    [SerializeField] private float life = 1.2f; // tiempo total antes de explotar
    [SerializeField] private float warningTime = 0.5f; // tiempo antes de explotar en el que empieza a titilar
    [SerializeField] private float blinkSpeed = 15f;   // velocidad del parpadeo
    [SerializeField] private Color warningColor = Color.red;

    private SpriteRenderer sr;
    private Color baseColor;
    private bool warningActive = false;
    private float lifeTimer;

    public void Initialize(Vector2 direction, float spd, GameObject child, float childSpd, float childDmg, LayerMask mask)
    {
        dir = direction.normalized;
        speed = spd;
        childPrefab = child;
        childSpeed = childSpd;
        childDamage = childDmg;
        playerMask = mask;
    }

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr) baseColor = sr.color;
        lifeTimer = life;
    }

    private void Update()
    {
        transform.position += (Vector3)(dir * speed * Time.deltaTime);

        lifeTimer -= Time.deltaTime;

        // Activa el parpadeo en los últimos segundos
        if (!warningActive && lifeTimer <= warningTime)
        {
            warningActive = true;
        }

        if (warningActive && sr)
        {
            float t = Mathf.Sin(Time.time * blinkSpeed) * 0.5f + 0.5f;
            sr.color = Color.Lerp(baseColor, warningColor, t);
        }

        if (lifeTimer <= 0f)
        {
            if (sr) sr.color = baseColor;
            Explode();
            Destroy(gameObject);
        }
    }

    private void Explode()
    {
        if (!childPrefab) return;

        SpawnChild(Vector2.up);
        SpawnChild(Vector2.down);
        SpawnChild(Vector2.left);
        SpawnChild(Vector2.right);
    }

    private void SpawnChild(Vector2 d)
    {
        var go = Instantiate(childPrefab, transform.position, Quaternion.identity);
        var cb = go.GetComponent<MoleBulletChild>();
        if (cb) cb.Initialize(d, childSpeed, childDamage, playerMask);
    }
}


//using UnityEngine;


//public class MoleBulletExplosive : MonoBehaviour
//{
//    private Vector2 dir;
//    private float speed;
//    private GameObject childPrefab;
//    private float childSpeed;
//    private float childDamage;
//    private LayerMask playerMask;

//    [SerializeField] private float life = 1.2f; // corta para forzar la explosión

//    public void Initialize(Vector2 direction, float spd, GameObject child, float childSpd, float childDmg, LayerMask mask)
//    {
//        dir = direction.normalized; speed = spd;
//        childPrefab = child; childSpeed = childSpd; childDamage = childDmg; playerMask = mask;
//    }

//    private void Update()
//    {
//        transform.position += (Vector3)(dir * speed * Time.deltaTime);
//        life -= Time.deltaTime;
//        if (life <= 0f) { Explode(); Destroy(gameObject); }
//    }

//    private void Explode()
//    {
//        if (!childPrefab) return;
//        SpawnChild(Vector2.up);
//        SpawnChild(Vector2.down);
//        SpawnChild(Vector2.left);
//        SpawnChild(Vector2.right);
//    }

//    private void SpawnChild(Vector2 d)
//    {
//        var go = Instantiate(childPrefab, transform.position, Quaternion.identity);
//        var cb = go.GetComponent<MoleBulletChild>();
//        if (cb) cb.Initialize(d, childSpeed, childDamage, playerMask);
//    }
//}
