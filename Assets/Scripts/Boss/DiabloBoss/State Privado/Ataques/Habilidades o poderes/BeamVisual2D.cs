using UnityEngine;

public class BeamVisual2D : MonoBehaviour
{
    [Header("Refs (arrastrá si no los encuentra)")]
    [SerializeField] private Transform visualRoot;          // el objeto que escalás (puede ser el mismo GO)
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private BoxCollider2D hitbox;
    [SerializeField] private DevilBeamDamage damage;        // opcional

    [Header("Collider tuning")]
    [SerializeField] private Vector2 colliderSizeMul = Vector2.one; // multiplica (len,width)
    [SerializeField] private Vector2 colliderOffset = Vector2.zero; // offset local

    private float _lastLen = -1f;
    private float _lastWidth = -1f;
    private bool _lastColliderOn = false;

    private void Reset() => Cache();
    private void Awake() => Cache();

    private void Cache()
    {
        if (!visualRoot) visualRoot = transform;
        if (!sprite) sprite = GetComponentInChildren<SpriteRenderer>(true);

        // Si tu BoxCollider2D está en un hijo, cambiá por GetComponentInChildren<BoxCollider2D>(true)
        if (!hitbox) hitbox = GetComponent<BoxCollider2D>();

        if (!damage) damage = GetComponent<DevilBeamDamage>();
    }

    // ========================= CONFIG "WORLD" (Attack 1) =========================

    /// <summary>
    /// Configura el beam entre A y B (posición/rotación en mundo).
    /// El collider se setea por size/offset (NO por escala).
    /// </summary>
    public void ConfigureBetween(Vector3 a, Vector3 b, float width, Color color, bool damageOn, bool colliderOn)
    {
        Vector3 dir = (b - a);
        float len = dir.magnitude;
        if (len < 0.001f) return;

        // Pos + rotación
        transform.position = (a + b) * 0.5f;
        transform.rotation = Quaternion.FromToRotation(Vector2.right, dir.normalized);

        ApplyVisualScale(len, width, color);

        // guardar para tuning en vivo
        _lastLen = len;
        _lastWidth = width;
        _lastColliderOn = colliderOn;

        ApplyCollider();

        if (damage) damage.enabled = damageOn;
    }

    // ========================= CONFIG "LOCAL" (Attack 5 RotatingX) =========================

    /// <summary>
    /// Configura el beam SOLO en local scale + color.
    /// Ideal para beams hijos de un pivot que rota.
    /// El GO ya debe estar posicionado/rotado (localRotation) donde quieras.
    /// </summary>
    public void ConfigureLocal(float length, float width, Color color, bool damageOn, bool colliderOn)
    {
        length = Mathf.Max(0.01f, length);
        width = Mathf.Max(0.01f, width);

        ApplyVisualScale(length, width, color);

        // guardar para tuning en vivo
        _lastLen = length;
        _lastWidth = width;
        _lastColliderOn = colliderOn;

        ApplyCollider();

        if (damage) damage.enabled = damageOn;
    }

    // ========================= VISUAL =========================

    private void ApplyVisualScale(float length, float width, Color color)
    {
        if (sprite && sprite.sprite)
        {
            // Para evitar tiling raro si estaba en Tiled/Sliced
            sprite.drawMode = SpriteDrawMode.Simple;

            Vector2 spriteSize = sprite.sprite.bounds.size;
            float sx = length / Mathf.Max(0.0001f, spriteSize.x);
            float sy = width / Mathf.Max(0.0001f, spriteSize.y);

            if (visualRoot) visualRoot.localScale = new Vector3(sx, sy, 1f);
            else transform.localScale = new Vector3(sx, sy, 1f);

            sprite.color = color;
        }
        else
        {
            if (visualRoot) visualRoot.localScale = new Vector3(length, width, 1f);
            else transform.localScale = new Vector3(length, width, 1f);
        }
    }

    // ========================= COLLIDER =========================

    private void ApplyCollider()
    {
        if (!hitbox) return;
        if (_lastLen <= 0f || _lastWidth <= 0f) return;

        hitbox.offset = colliderOffset;
        hitbox.size = new Vector2(_lastLen * colliderSizeMul.x, _lastWidth * colliderSizeMul.y);
        hitbox.enabled = _lastColliderOn;
    }

    public void SetColliderEnabled(bool on)
    {
        if (!hitbox) return;
        _lastColliderOn = on;
        hitbox.enabled = on;
    }

    public void SetColliderTuning(Vector2 sizeMul, Vector2 offset)
    {
        colliderSizeMul = sizeMul;
        colliderOffset = offset;

        ApplyCollider(); // se refleja al instante si ya fue configurado
    }

    public void RefreshColliderNow()
    {
        ApplyCollider();
    }

    // ========================= EXTRAS =========================

    public void SetAlpha(float a)
    {
        if (!sprite) return;
        Color c = sprite.color;
        c.a = a;
        sprite.color = c;
    }

    public void SetDamageEnabled(bool on)
    {
        if (damage) damage.enabled = on;
    }
}

//using UnityEngine;

//public class BeamVisual2D : MonoBehaviour
//{
//    [Header("Refs (arrastrá si no los encuentra)")]
//    [SerializeField] private Transform visualRoot;          // el objeto que escalás (puede ser el mismo GO)
//    [SerializeField] private SpriteRenderer sprite;
//    [SerializeField] private BoxCollider2D hitbox;
//    [SerializeField] private DevilBeamDamage damage;        // opcional

//    [Header("Collider tuning")]
//    [SerializeField] private Vector2 colliderSizeMul = Vector2.one; // multiplica (len,width)
//    [SerializeField] private Vector2 colliderOffset = Vector2.zero; // offset local


//    private float _lastLen = -1f;
//    private float _lastWidth = -1f;
//    private bool _lastColliderOn = false;


//    private void Reset()
//    {
//        Cache();
//    }

//    private void Awake()
//    {
//        Cache();
//    }

//    private void Cache()
//    {
//        if (!visualRoot) visualRoot = transform;
//        if (!sprite) sprite = GetComponentInChildren<SpriteRenderer>(true);

//        // IMPORTANTE: si el collider está en un hijo, cambiá a GetComponentInChildren<BoxCollider2D>(true)
//        if (!hitbox) hitbox = GetComponent<BoxCollider2D>();
//        if (!damage) damage = GetComponent<DevilBeamDamage>();
//    }

//    /// <summary>
//    /// Configura el beam entre A y B, con ancho visual y color.
//    /// El collider se setea por size/offset (NO por escala).
//    /// </summary>
//    public void ConfigureBetween(Vector3 a, Vector3 b, float width, Color color, bool damageOn, bool colliderOn)
//    {
//        Vector3 dir = (b - a);
//        float len = dir.magnitude;
//        if (len < 0.001f) return;

//        // Pos + rotación
//        transform.position = (a + b) * 0.5f;
//        transform.rotation = Quaternion.FromToRotation(Vector2.right, dir.normalized);

//        // Escala visual (sprite)
//        if (sprite && sprite.sprite)
//        {
//            Vector2 spriteSize = sprite.sprite.bounds.size;
//            float sx = len / Mathf.Max(0.0001f, spriteSize.x);
//            float sy = width / Mathf.Max(0.0001f, spriteSize.y);

//            if (visualRoot) visualRoot.localScale = new Vector3(sx, sy, 1f);
//            else transform.localScale = new Vector3(sx, sy, 1f);

//            sprite.color = color;
//        }
//        else
//        {
//            // fallback si no hay sprite
//            if (visualRoot) visualRoot.localScale = new Vector3(len, width, 1f);
//            else transform.localScale = new Vector3(len, width, 1f);
//        }

//        // ===================== NUEVO: guardamos base para tuning en vivo =====================
//        _lastLen = len;
//        _lastWidth = width;
//        _lastColliderOn = colliderOn;
//        // =====================================================================================

//        // Collider independiente de la escala visual
//        ApplyCollider(); // <-- NUEVO (en vez de setearlo inline)

//        // Daño
//        if (damage)
//            damage.enabled = damageOn;
//    }


//    private void ApplyCollider()
//    {
//        if (!hitbox) return;
//        if (_lastLen <= 0f || _lastWidth <= 0f) return; // todavía no fue configurado

//        hitbox.offset = colliderOffset;
//        hitbox.size = new Vector2(_lastLen * colliderSizeMul.x, _lastWidth * colliderSizeMul.y);
//        hitbox.enabled = _lastColliderOn;
//    }


//    public void SetColliderEnabled(bool on)
//    {
//        if (!hitbox) return;
//        _lastColliderOn = on;   // <-- NUEVO: que quede guardado
//        hitbox.enabled = on;
//    }

//    public void SetAlpha(float a)
//    {
//        if (sprite)
//        {
//            Color c = sprite.color;
//            c.a = a;
//            sprite.color = c;
//        }
//    }

//    // Si querés tunear collider desde el Inspector por ataque/prefab:
//    public void SetColliderTuning(Vector2 sizeMul, Vector2 offset)
//    {
//        colliderSizeMul = sizeMul;
//        colliderOffset = offset;

//        ApplyCollider(); // <-- NUEVO: esto hace que se vea en el momento
//    }

//    public void SetDamageEnabled(bool on)
//    {
//        if (damage) damage.enabled = on;
//    }

//    // Para forzar que re-aplique el collider (por si cambiaste tuning en runtime)
//    public void RefreshColliderNow()
//    {
//        // esto existe en tu versión nueva: ApplyCollider();
//        // si no lo tenés público, dejalo así:
//        // (en tu versión anterior, ApplyCollider era private)
//        // Solución simple: llamamos SetColliderTuning con lo mismo, que ya hace ApplyCollider().
//        SetColliderTuning(colliderSizeMul, colliderOffset);
//    }
//}
