using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [Header("Configuración del Laser")]
    public float duration = 4f;
    public float damagePerSecond = 10f;
    public float damageInterval = 0.1f;
    public float maxDistance = 15f;

    [Header("Knockback")]
    public float knockbackForce = 5f;

    [Header("Raycast / Obstáculos")]
    [Tooltip("Capas que cortan el láser (paredes, props, etc.). Si lo dejás vacío, corta con cualquier collider.")]
    [SerializeField] private LayerMask obstacleMask;

    [Header("Visual A1 (Sprite estirable)")]
    [SerializeField] private Transform visualRoot;            // Child "Visual"
    [SerializeField] private SpriteRenderer spriteRenderer;   // Puede estar en Visual
    [Tooltip("Si tu sprite tiene pivot CENTRADO, dejalo en true. Si el pivot es Left, ponelo en false.")]
    [SerializeField] private bool centerPivotCompensation = true;
    [SerializeField] private float yOffset = 0f;

    [Header("Origen del láser")]
    [Tooltip("Empuja el inicio del rayo hacia adelante para que no atraviese al jugador.")]
    [SerializeField] private float forwardStartOffset = 0.3f;

    private Transform playerTransform;
    private float timer;
    private float damageTimer;
    private bool isActive;

    private readonly HashSet<EnemyHealth> damagedThisTick = new HashSet<EnemyHealth>();

    public void Initialize(Transform player)
    {
        playerTransform = player;
        timer = duration;
        damageTimer = 0f;
        isActive = true;

        if (visualRoot == null)
        {
            var v = transform.Find("Visual");
            if (v != null) visualRoot = v;
        }
        if (spriteRenderer == null && visualRoot != null)
            spriteRenderer = visualRoot.GetComponentInChildren<SpriteRenderer>();

        UpdateLaser();
    }

    private void Update()
    {
        if (!isActive || playerTransform == null)
        {
            Deactivate();
            return;
        }

        timer -= Time.deltaTime;
        damageTimer += Time.deltaTime;

        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;
            ApplyDamage();
        }

        UpdateLaser();

        if (timer <= 0f)
            Deactivate();
    }

    private void UpdateLaser()
    {
        Vector2 playerPos = (Vector2)playerTransform.position;
        Vector2 mouse = GetMouseWorldPosition();
        Vector2 dir = (mouse - playerPos).normalized;

        // ✅ ORIGEN ADELANTADO (clave para que no atraviese al jugador)
        Vector2 origin = playerPos + dir * forwardStartOffset;

        float distance = ComputeDistance(origin, dir);
        UpdateVisual(origin, dir, distance);
    }

    private float ComputeDistance(Vector2 origin, Vector2 dir)
    {
        RaycastHit2D hit = (obstacleMask.value == 0)
            ? Physics2D.Raycast(origin, dir, maxDistance)
            : Physics2D.Raycast(origin, dir, maxDistance, obstacleMask);

        if (hit.collider != null && !hit.collider.CompareTag("Player"))
            return hit.distance;

        return maxDistance;
    }

    private void ApplyDamage()
    {
        Vector2 playerPos = (Vector2)playerTransform.position;
        Vector2 mouse = GetMouseWorldPosition();
        Vector2 dir = (mouse - playerPos).normalized;

        // ✅ mismo origen que el visual
        Vector2 origin = playerPos + dir * forwardStartOffset;

        float distance = ComputeDistance(origin, dir);

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, dir, distance);

        damagedThisTick.Clear();
        int damage = Mathf.CeilToInt(damagePerSecond * damageInterval);

        foreach (var h in hits)
        {
            if (h.collider == null) continue;
            if (!h.collider.CompareTag("Enemy")) continue;

            EnemyHealth enemy = h.collider.GetComponentInParent<EnemyHealth>();
            if (enemy == null) continue;

            if (damagedThisTick.Contains(enemy)) continue;
            damagedThisTick.Add(enemy);

            enemy.TakeDamage(damage, dir, knockbackForce);
        }
    }

    private void UpdateVisual(Vector2 origin, Vector2 dir, float distance)
    {
        if (visualRoot == null) return;

        // Root en el ORIGEN real del láser (adelantado)
        transform.position = origin;

        // Rotación 2D (Z)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        visualRoot.rotation = Quaternion.Euler(0f, 0f, angle);

        // Largo del sprite en unidades mundo
        float spriteWorldLength = 1f;
        if (spriteRenderer != null && spriteRenderer.sprite != null)
            spriteWorldLength = spriteRenderer.sprite.bounds.size.x;

        // Escala X para que el beam mida "distance"
        Vector3 s = visualRoot.localScale;
        s.x = distance / Mathf.Max(0.0001f, spriteWorldLength);
        visualRoot.localScale = s;

        //  POSICIÓN del visual:
        // - Si pivot centrado: mover a distancia/2 para que arranque en el origen
        // - Si pivot left: dejarlo en 0
        if (centerPivotCompensation)
            visualRoot.localPosition = new Vector3(distance * 0.5f, yOffset, 0f);
        else
            visualRoot.localPosition = new Vector3(0f, yOffset, 0f);
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector3 mouseScreen = Input.mousePosition;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        return new Vector2(mouseWorld.x, mouseWorld.y);
    }

    private void Deactivate()
    {
        isActive = false;
        Destroy(gameObject, 0.05f);
    }
}



//using UnityEngine;

//public class LaserBeam : MonoBehaviour
//{
//    [Header("Configuración del Laser")]
//    public float duration = 4f;
//    public float damagePerSecond = 10f;
//    public float damageInterval = 0.1f; // Más frecuente para mejor sensación
//    public float maxDistance = 15f; // Distancia máxima del laser

//    [Header("Knockback")]
//    public float knockbackForce = 5f;
//    public float knockbackDuration = 0.2f;

//    [Header("Visual")]
//    public LineRenderer lineRenderer;
//    public float lineWidth = 0.1f;
//    public Gradient laserColor;
//    public float pulseSpeed = 5f;
//    public float pulseIntensity = 0.2f;

//    private Transform playerTransform;
//    private float timer;
//    private float damageTimer;
//    private bool isActive = false;
//    private float baseWidth;
//    private float startTime;

//    private void Start()
//    {
//        if (lineRenderer == null)
//        {
//            lineRenderer = gameObject.AddComponent<LineRenderer>();
//        }

//        SetupLineRenderer();
//        timer = duration;
//        baseWidth = lineWidth;
//        startTime = Time.time;
//        isActive = true;
//    }

//    private void SetupLineRenderer()
//    {
//        lineRenderer.useWorldSpace = true;
//        lineRenderer.startWidth = lineWidth;
//        lineRenderer.endWidth = lineWidth;
//        lineRenderer.colorGradient = laserColor;
//        lineRenderer.positionCount = 2;
//        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

//        // Configurar para pixel art
//        lineRenderer.alignment = LineAlignment.View;
//        lineRenderer.textureMode = LineTextureMode.Tile;
//    }

//    public void Initialize(Transform player)
//    {
//        playerTransform = player;
//        UpdateLaserVisual();
//    }

//    private void Update()
//    {
//        if (!isActive || playerTransform == null)
//        {
//            if (playerTransform == null) Deactivate();
//            return;
//        }

//        timer -= Time.deltaTime;
//        damageTimer += Time.deltaTime;

//        // Animación de pulso del laser
//        float pulse = Mathf.Sin((Time.time - startTime) * pulseSpeed) * pulseIntensity + 1f;
//        lineRenderer.startWidth = baseWidth * pulse;
//        lineRenderer.endWidth = baseWidth * pulse;

//        // Aplicar daño a enemigos en la línea
//        if (damageTimer >= damageInterval)
//        {
//            damageTimer = 0f;
//            ApplyDamageToEnemies();
//        }

//        // Actualizar visual del laser (seguir al jugador y apuntar al mouse)
//        UpdateLaserVisual();

//        if (timer <= 0f)
//        {
//            Deactivate();
//        }
//    }

//    private void UpdateLaserVisual()
//    {
//        if (lineRenderer != null && playerTransform != null)
//        {
//            Vector2 playerPosition = playerTransform.position;
//            Vector2 mousePosition = GetMouseWorldPosition();
//            Vector2 direction = (mousePosition - playerPosition).normalized;

//            // Calcular punto final del laser (hasta maxDistance o hasta el primer obstáculo)
//            Vector2 endPoint = playerPosition + (direction * maxDistance);

//            // Raycast para ver si hay obstáculos
//            RaycastHit2D hit = Physics2D.Raycast(playerPosition, direction, maxDistance);
//            if (hit.collider != null && !hit.collider.CompareTag("Player"))
//            {
//                endPoint = hit.point;
//            }

//            lineRenderer.SetPosition(0, playerPosition);
//            lineRenderer.SetPosition(1, endPoint);
//        }
//    }

//    private void ApplyDamageToEnemies()
//    {
//        if (playerTransform == null) return;

//        Vector2 playerPosition = playerTransform.position;
//        Vector2 mousePosition = GetMouseWorldPosition();
//        Vector2 direction = (mousePosition - playerPosition).normalized;
//        float distance = maxDistance;

//        // Raycast para encontrar el punto final real
//        RaycastHit2D hit = Physics2D.Raycast(playerPosition, direction, maxDistance);
//        if (hit.collider != null && !hit.collider.CompareTag("Player"))
//        {
//            distance = Vector2.Distance(playerPosition, hit.point);
//        }

//        RaycastHit2D[] hits = Physics2D.RaycastAll(playerPosition, direction, distance);
//        foreach (RaycastHit2D rayHit in hits)
//        {
//            if (rayHit.collider != null && rayHit.collider.CompareTag("Enemy"))
//            {
//                EnemyHealth enemy = rayHit.collider.GetComponent<EnemyHealth>();
//                if (enemy != null)
//                {
//                    // Calcular daño por intervalo
//                    int damage = Mathf.CeilToInt(damagePerSecond * damageInterval);

//                    // Aplicar knockback similar a los ataques normales
//                    Vector2 knockbackDir = direction;
//                    float knockbackForceActual = knockbackForce;

//                    enemy.TakeDamage(damage, knockbackDir, knockbackForceActual);

//                    // Opcional: aplicar knockback continuo mientras el laser está activo
//                    ApplyContinuousKnockback(enemy, knockbackDir);
//                }
//            }
//        }
//    }

//    private void ApplyContinuousKnockback(EnemyHealth enemy, Vector2 direction)
//    {
//        // Opcional: aplicar una pequeña fuerza continua mientras el enemigo está en el laser
//        // Esto crea un efecto de empuje constante
//        /*
//        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
//        if (enemyRb != null)
//        {
//            enemyRb.AddForce(direction * (knockbackForce * 0.1f), ForceMode2D.Force);
//        }
//        */
//    }

//    private Vector2 GetMouseWorldPosition()
//    {
//        Vector3 mouseScreenPosition = Input.mousePosition;
//        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
//        return new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);
//    }

//    private void Deactivate()
//    {
//        isActive = false;
//        if (lineRenderer != null)
//        {
//            lineRenderer.enabled = false;
//        }
//        Destroy(gameObject, 0.1f);
//    }
//}