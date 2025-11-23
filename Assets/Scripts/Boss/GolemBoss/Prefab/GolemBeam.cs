using UnityEngine;
using System.Collections.Generic;

public class GolemBeam : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] float duration = 3f;
    [SerializeField] float damagePerTick = 1f;
    [SerializeField] float tickInterval = 0.25f;
    [SerializeField] float maxDistance = 12f;
    [SerializeField] float thickness = 0.6f;
    [SerializeField] float knockbackImpulse = 6f;
    [SerializeField] LayerMask obstacleMask, playerMask;

    [Header("Warmup (preview)")]
    [SerializeField] float warmupTime = 1f;
    bool warmupActive = true;
    float warmupTimer = 0f;
    Color savedSpriteColor;
    Color savedLineColor;
    bool hasSpriteColor = false;
    bool hasLineColor = false;

    [Header("Tracking (delay + giro)")]
    [SerializeField] float followDelay = 0.5f;
    [SerializeField] float historyWindow = 2f;   // > followDelay
    [SerializeField] float turnRateDegPerSec = 120f;

    [Header("Visual (LineRenderer opcional)")]
    [SerializeField] LineRenderer line;
    [SerializeField] Material lineMaterial;      // si es null  Sprites/Default
    [SerializeField] float pulseSpeed = 5f, pulseIntensity = 0.2f;
    [SerializeField] bool disableLineRenderer = false;

    [Header("Sorting")]
    [SerializeField] string sortingLayerName = "Effects";
    [SerializeField] int sortingOrder = 100;

    [Header("Sprite FX (instanciado)")]
    [SerializeField] GameObject beamSpritePrefab; // Prefab con SpriteRenderer + Animator
    [SerializeField] int spriteOrderOffset = 1;   // dibujar por encima del line
    [SerializeField] float spriteHeight = 0.6f;   // alto visual; usá el mismo que thickness
    [SerializeField] bool useTiledSprite = false; // para 1 tramo continuo = false
    [SerializeField, Tooltip("+ sube, - baja (unidades mundo)")]
    float spritePerpOffset = 0.06f;               // desplaza el sprite “arriba”

    // Runtime
    Transform owner, target;
    float life, tick, baseWidth;
    bool active, didSetup;
    Vector2 aimDir = Vector2.right;

    // sprite instanciado
    Transform spriteTf;
    SpriteRenderer spriteSR;

    struct Sample { public float t; public Vector2 p; public Sample(float t, Vector2 p) { this.t = t; this.p = p; } }
    readonly List<Sample> samples = new List<Sample>(128);

    // Inicializa desde el estado
    public void Initialize(Transform owner, Transform target, float durationOverride,
                           float dmgPerTick, float interval, float range,
                           float width, float knockback, LayerMask playerMask, LayerMask obstacleMask)
    {
        this.owner = owner; this.target = target;
        duration = durationOverride; damagePerTick = dmgPerTick; tickInterval = interval;
        maxDistance = range; thickness = width; knockbackImpulse = knockback;
        this.playerMask = playerMask; this.obstacleMask = obstacleMask;
        Setup();
    }

    void Awake()
    {
        if (!line) line = gameObject.AddComponent<LineRenderer>();
        // NO llamamos Setup aquí
    }

    void Setup()
    {
        if (didSetup) return;
        didSetup = true;

        // ---------- LineRenderer ----------
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = line.endWidth = thickness;
        line.alignment = LineAlignment.TransformZ;
        line.numCapVertices = 6; line.numCornerVertices = 3;
        line.textureMode = LineTextureMode.Stretch;

        if (lineMaterial) line.material = lineMaterial;
        else line.material = new Material(Shader.Find("Sprites/Default")) { renderQueue = 3000 };

        line.sortingLayerName = sortingLayerName;
        line.sortingOrder = sortingOrder;
        line.enabled = !disableLineRenderer;

        // ---------- Sprite FX (instanciado UNA sola vez) ----------
        EnsureSpriteFX();

        // ---------- estado inicial ----------
        baseWidth = thickness;
        life = duration; tick = 0f; active = true;

        if (owner && target)
        {
            var d = (Vector2)target.position - (Vector2)owner.position;
            if (d.sqrMagnitude > 0.0001f) aimDir = d.normalized;
        }

        samples.Clear();
        if (target) samples.Add(new Sample(Time.time, target.position));

        // ---------- warmup visual ----------
        warmupActive = true;
        warmupTimer = 0f;

        if (spriteSR)
        {
            savedSpriteColor = spriteSR.color;
            hasSpriteColor = true;

            var c = spriteSR.color;
            c.a *= 0.35f;   // más transparente para preview
            spriteSR.color = c;
        }

        if (line && line.material)
        {
            savedLineColor = line.material.color;
            hasLineColor = true;

            var c = line.material.color;
            c.a *= 0.35f;
            line.material.color = c;
        }
    }

    void EnsureSpriteFX()
    {
        // Reusar si ya existe (por si el prefab lo trae de fábrica)
        var existing = transform.Find("LaserSpriteFX");
        if (existing) spriteTf = existing;
        else if (beamSpritePrefab) spriteTf = Instantiate(beamSpritePrefab, transform).transform;

        if (spriteTf)
        {
            spriteTf.name = "LaserSpriteFX";
            spriteSR = spriteTf.GetComponentInChildren<SpriteRenderer>(true);
            if (spriteSR)
            {
                spriteSR.sortingLayerName = sortingLayerName;
                spriteSR.sortingOrder = sortingOrder + spriteOrderOffset;
                spriteSR.drawMode = useTiledSprite ? SpriteDrawMode.Tiled : SpriteDrawMode.Simple;
            }
        }
    }

    void Update()
    {
        if (!active) return;
        if (!owner || !target) { Kill(); return; }

        float dt = Time.deltaTime, now = Time.time;
        life -= dt; tick += dt;

        // ---------- warmup timer ----------
        if (warmupActive)
        {
            warmupTimer += dt;
            if (warmupTimer >= warmupTime)
            {
                warmupActive = false;

                if (hasSpriteColor && spriteSR)
                    spriteSR.color = savedSpriteColor;

                if (hasLineColor && line && line.material)
                    line.material.color = savedLineColor;
            }
        }

        // historial target
        samples.Add(new Sample(now, target.position));
        float oldest = now - Mathf.Max(historyWindow, followDelay) - 0.1f;
        while (samples.Count > 1 && samples[0].t < oldest) samples.RemoveAt(0);

        // dirección con delay
        Vector2 delayedPos = (followDelay > 0.001f) ? GetDelayedPosition(now - followDelay)
                                                    : (Vector2)target.position;
        Vector2 origin = owner.position;
        Vector2 desiredDir = delayedPos - origin;
        desiredDir = (desiredDir.sqrMagnitude > 0.0001f) ? desiredDir.normalized : aimDir;

        // giro suave
        float maxRad = turnRateDegPerSec * Mathf.Deg2Rad * dt;
        Vector3 rot3 = Vector3.RotateTowards(new Vector3(aimDir.x, aimDir.y, 0f),
                                             new Vector3(desiredDir.x, desiredDir.y, 0f),
                                             maxRad, 0f);
        aimDir = new Vector2(rot3.x, rot3.y).normalized;

        // largo por raycast
        float length = maxDistance;
        var hit = Physics2D.Raycast(origin, aimDir, maxDistance, obstacleMask);
        if (hit.collider) length = hit.distance;

        Vector2 end = origin + aimDir * length;

        // ---------- LineRenderer ----------
        if (line && line.enabled)
        {
            float pulse = Mathf.Sin(now * pulseSpeed) * pulseIntensity + 1f;
            line.startWidth = line.endWidth = baseWidth * pulse;
            line.SetPosition(0, origin);
            line.SetPosition(1, end);
        }

        // ---------- Sprite FX (un solo objeto) ----------
        if (spriteSR)
        {
            float h = (spriteHeight > 0f) ? spriteHeight : thickness;

            // centro del rayo y rotación
            Vector2 mid = (origin + end) * 0.5f;
            float ang = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

            // offset perpendicular (+ sube, - baja)
            Vector2 perp = new Vector2(-aimDir.y, aimDir.x);
            Vector2 worldOffset = perp.normalized * spritePerpOffset;

            spriteTf.position = mid + worldOffset;
            spriteTf.rotation = Quaternion.Euler(0f, 0f, ang);

            if (useTiledSprite)
            {
                spriteSR.size = new Vector2(length, h);
            }
            else
            {
                float spriteW = spriteSR.sprite ? spriteSR.sprite.bounds.size.x : 1f;
                float spriteH = spriteSR.sprite ? spriteSR.sprite.bounds.size.y : 1f;
                spriteTf.localScale = new Vector3(
                    Mathf.Max(0.0001f, length / Mathf.Max(0.0001f, spriteW)),
                    h / Mathf.Max(0.0001f, spriteH),
                    1f
                );
            }
        }

        // daño y vida
        if (tick >= tickInterval)
        {
            tick = 0f;
            DoDamage(origin, aimDir, length);
        }
        if (life <= 0f) Kill();
    }

    Vector2 GetDelayedPosition(float t)
    {
        int n = samples.Count;
        if (n == 0) return target ? (Vector2)target.position : Vector2.zero;
        if (n == 1 || t <= samples[0].t) return samples[0].p;
        if (t >= samples[n - 1].t) return samples[n - 1].p;

        int lo = 0, hi = n - 1;
        while (lo < hi)
        {
            int mid = (lo + hi) >> 1;
            if (samples[mid].t < t) lo = mid + 1; else hi = mid;
        }
        int i1 = Mathf.Clamp(lo, 1, n - 1), i0 = i1 - 1;
        float t0 = samples[i0].t, t1 = samples[i1].t, a = Mathf.InverseLerp(t0, t1, t);
        return Vector2.LerpUnclamped(samples[i0].p, samples[i1].p, a);
    }

    void DoDamage(Vector2 origin, Vector2 dir, float length)
    {
        // Durante el warmup no hay daño ni knockback
        if (warmupActive) return;

        Vector2 center = origin + dir * (length * 0.5f);
        var hits = Physics2D.OverlapBoxAll(center, new Vector2(length, thickness * 1.2f),
                                           Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, playerMask);
        foreach (var h in hits)
        {
            var hp = h.GetComponent<PlayerHealth>() ?? h.GetComponentInParent<PlayerHealth>();
            if (!hp) continue;
            hp.TakeDamage(damagePerTick, center);
            var rb = h.attachedRigidbody;
            if (rb && knockbackImpulse > 0f) rb.AddForce(dir * knockbackImpulse, ForceMode2D.Impulse);
        }
    }

    void Kill()
    {
        active = false;
        if (line) line.enabled = false;
        Destroy(gameObject); // destruye también el sprite hijo
    }

    public void StopNow(bool destroyImmediately = true)
    {
        active = false;
        if (line) line.enabled = false;
        if (destroyImmediately) Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (!owner) return;
        Vector2 origin = owner.position, dir = (aimDir.sqrMagnitude > 0.0001f ? aimDir : Vector2.right);
        float length = maxDistance, ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(origin + dir * (length * 0.5f), Quaternion.Euler(0, 0, ang), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector2(length, thickness * 1.2f));
        Gizmos.matrix = Matrix4x4.identity;
    }
}
//using UnityEngine;
//using System.Collections.Generic;

//public class GolemBeam : MonoBehaviour
//{
//    [Header("Config")]
//    [SerializeField] float duration = 3f;
//    [SerializeField] float damagePerTick = 1f;
//    [SerializeField] float tickInterval = 0.25f;
//    [SerializeField] float maxDistance = 12f;
//    [SerializeField] float thickness = 0.6f;
//    [SerializeField] float knockbackImpulse = 6f;
//    [SerializeField] LayerMask obstacleMask, playerMask;

//    [Header("Warmup Settings")]
//    public float warmupTime = 1f;
//    private bool warmup = true;
//    private float warmupTimer = 0f;
//    private float originalAlpha = 1f;

//    [Header("Tracking (delay + giro)")]
//    [SerializeField] float followDelay = 0.5f;
//    [SerializeField] float historyWindow = 2f;   // > followDelay
//    [SerializeField] float turnRateDegPerSec = 120f;

//    [Header("Visual (LineRenderer opcional)")]
//    [SerializeField] LineRenderer line;
//    [SerializeField] Material lineMaterial;      // si es null  Sprites/Default
//    [SerializeField] float pulseSpeed = 5f, pulseIntensity = 0.2f;
//    [SerializeField] bool disableLineRenderer = false;

//    [Header("Sorting")]
//    [SerializeField] string sortingLayerName = "Effects";
//    [SerializeField] int sortingOrder = 100;

//    [Header("Sprite FX (instanciado)")]
//    [SerializeField] GameObject beamSpritePrefab; // Prefab con SpriteRenderer + Animator
//    [SerializeField] int spriteOrderOffset = 1;   // dibujar por encima del line
//    [SerializeField] float spriteHeight = 0.6f;   // alto visual; usá el mismo que thickness
//    [SerializeField] bool useTiledSprite = false; // para 1 tramo continuo = false
//    [SerializeField, Tooltip("+ sube, - baja (unidades mundo)")]
//    float spritePerpOffset = 0.06f;               // desplaza el sprite “arriba”

//    // Runtime
//    Transform owner, target;
//    float life, tick, baseWidth;
//    bool active, didSetup;
//    Vector2 aimDir = Vector2.right;

//    // sprite instanciado
//    Transform spriteTf;
//    SpriteRenderer spriteSR;

//    struct Sample { public float t; public Vector2 p; public Sample(float t, Vector2 p) { this.t = t; this.p = p; } }
//    readonly List<Sample> samples = new List<Sample>(128);

//    // Inicializa desde el estado
//    public void Initialize(Transform owner, Transform target, float durationOverride,
//                           float dmgPerTick, float interval, float range,
//                           float width, float knockback, LayerMask playerMask, LayerMask obstacleMask)
//    {
//        this.owner = owner; this.target = target;
//        duration = durationOverride; damagePerTick = dmgPerTick; tickInterval = interval;
//        maxDistance = range; thickness = width; knockbackImpulse = knockback;
//        this.playerMask = playerMask; this.obstacleMask = obstacleMask;
//        Setup();
//    }

//    void Awake()
//    {
//        if (!line) line = gameObject.AddComponent<LineRenderer>();
//        // NO llamamos Setup aquí
//    }

//    void Setup()
//    {
//        if (didSetup) return;
//        didSetup = true;

//        // ---------- LineRenderer ----------
//        line.useWorldSpace = true;
//        line.positionCount = 2;
//        line.startWidth = line.endWidth = thickness;
//        line.alignment = LineAlignment.TransformZ;
//        line.numCapVertices = 6; line.numCornerVertices = 3;
//        line.textureMode = LineTextureMode.Stretch;

//        if (lineMaterial) line.material = lineMaterial;
//        else line.material = new Material(Shader.Find("Sprites/Default")) { renderQueue = 3000 };

//        line.sortingLayerName = sortingLayerName;
//        line.sortingOrder = sortingOrder;
//        line.enabled = !disableLineRenderer;

//        // ---------- Sprite FX (instanciado UNA sola vez) ----------
//        EnsureSpriteFX();

//        // ---------- estado inicial ----------
//        baseWidth = thickness;
//        life = duration; tick = 0f; active = true;

//        if (owner && target)
//        {
//            var d = (Vector2)target.position - (Vector2)owner.position;
//            if (d.sqrMagnitude > 0.0001f) aimDir = d.normalized;
//        }

//        samples.Clear();
//        if (target) samples.Add(new Sample(Time.time, target.position));

//        if (line && line.material)
//        {
//            originalAlpha = line.material.color.a;
//            var c = line.material.color;
//            c.a = 0.3f;             // aviso transparente
//            line.material.color = c;
//        }


//    }

//    void EnsureSpriteFX()
//    {
//        // Reusar si ya existe (por si el prefab lo trae de fábrica)
//        var existing = transform.Find("LaserSpriteFX");
//        if (existing) spriteTf = existing;
//        else if (beamSpritePrefab) spriteTf = Instantiate(beamSpritePrefab, transform).transform;

//        if (spriteTf)
//        {
//            spriteTf.name = "LaserSpriteFX";
//            spriteSR = spriteTf.GetComponentInChildren<SpriteRenderer>(true);
//            if (spriteSR)
//            {
//                spriteSR.sortingLayerName = sortingLayerName;
//                spriteSR.sortingOrder = sortingOrder + spriteOrderOffset;
//                spriteSR.drawMode = useTiledSprite ? SpriteDrawMode.Tiled : SpriteDrawMode.Simple;
//            }
//        }
//    }

//    void Update()
//    {

//        if (warmup)
//        {
//            warmupTimer += Time.deltaTime;

//            // usar grosor reducido
//            float previewWidth = baseWidth * 0.4f;
//            line.startWidth = line.endWidth = previewWidth;

//            // apuntar hacia el jugador sin seguimiento avanzado
//            Vector2 origin = owner.position;
//            Vector2 dir = target ? (Vector2)(target.position - owner.position).normalized : aimDir;
//            float length = maxDistance;

//            var hitPreview = Physics2D.Raycast(origin, dir, maxDistance, obstacleMask);
//            if (hitPreview.collider) length = hitPreview.distance;

//            Vector2 endPreview = origin + dir * length;

//            // dibujar preview
//            line.SetPosition(0, origin);
//            line.SetPosition(1, endPreview);

//            // NO daño, NO spriteFX, NO pulso
//            if (spriteSR)
//            {
//                // esconderlo o semi-transparente
//                var cc = spriteSR.color;
//                cc.a = 0f;
//                spriteSR.color = cc;
//                return;
//            }

//            // salir hasta terminar warmup
//            if (warmupTimer < warmupTime)
//                return;

//            // activar laser real
//            warmup = false;

//            // restaurar alfa original
//            if (line && line.material)
//            {
//                var c2 = line.material.color;
//                c2.a = originalAlpha;
//                line.material.color = c2;
//            }

//            // restaurar grosor normal
//            line.startWidth = line.endWidth = baseWidth;
//        }

//        if (!active) return;
//        if (!owner || !target) { Kill(); return; }

//        float dt = Time.deltaTime, now = Time.time;
//        life -= dt; tick += dt;

//        // historial target
//        samples.Add(new Sample(now, target.position));
//        float oldest = now - Mathf.Max(historyWindow, followDelay) - 0.1f;
//        while (samples.Count > 1 && samples[0].t < oldest) samples.RemoveAt(0);

//        // dirección con delay
//        Vector2 delayedPos = (followDelay > 0.001f) ? GetDelayedPosition(now - followDelay)
//                                                    : (Vector2)target.position;
//        Vector2 origin = owner.position;
//        Vector2 desiredDir = delayedPos - origin;
//        desiredDir = (desiredDir.sqrMagnitude > 0.0001f) ? desiredDir.normalized : aimDir;

//        // giro suave
//        float maxRad = turnRateDegPerSec * Mathf.Deg2Rad * dt;
//        Vector3 rot3 = Vector3.RotateTowards(new Vector3(aimDir.x, aimDir.y, 0f),
//                                             new Vector3(desiredDir.x, desiredDir.y, 0f),
//                                             maxRad, 0f);
//        aimDir = new Vector2(rot3.x, rot3.y).normalized;

//        // largo por raycast
//        float length = maxDistance;
//        var hit = Physics2D.Raycast(origin, aimDir, maxDistance, obstacleMask);
//        if (hit.collider) length = hit.distance;

//        Vector2 end = origin + aimDir * length;

//        // ---------- LineRenderer ----------
//        if (line && line.enabled)
//        {
//            float pulse = Mathf.Sin(now * pulseSpeed) * pulseIntensity + 1f;
//            line.startWidth = line.endWidth = baseWidth * pulse;
//            line.SetPosition(0, origin);
//            line.SetPosition(1, end);
//        }

//        // ---------- Sprite FX (un solo objeto) ----------
//        if (spriteSR)
//        {
//            float h = (spriteHeight > 0f) ? spriteHeight : thickness;

//            // centro del rayo y rotación
//            Vector2 mid = (origin + end) * 0.5f;
//            float ang = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

//            // offset perpendicular (+ sube, - baja)
//            Vector2 perp = new Vector2(-aimDir.y, aimDir.x);
//            Vector2 worldOffset = perp.normalized * spritePerpOffset;

//            spriteTf.position = mid + worldOffset;
//            spriteTf.rotation = Quaternion.Euler(0f, 0f, ang);

//            if (useTiledSprite)
//            {
//                spriteSR.size = new Vector2(length, h);
//            }
//            else
//            {
//                float spriteW = spriteSR.sprite ? spriteSR.sprite.bounds.size.x : 1f;
//                float spriteH = spriteSR.sprite ? spriteSR.sprite.bounds.size.y : 1f;
//                spriteTf.localScale = new Vector3(
//                    Mathf.Max(0.0001f, length / Mathf.Max(0.0001f, spriteW)),
//                    h / Mathf.Max(0.0001f, spriteH),
//                    1f
//                );
//            }
//        }

//        // daño y vida
//        if (tick >= tickInterval) { tick = 0f; DoDamage(origin, aimDir, length); }
//        if (life <= 0f) Kill();
//    }

//    Vector2 GetDelayedPosition(float t)
//    {
//        int n = samples.Count;
//        if (n == 0) return target ? (Vector2)target.position : Vector2.zero;
//        if (n == 1 || t <= samples[0].t) return samples[0].p;
//        if (t >= samples[n - 1].t) return samples[n - 1].p;

//        int lo = 0, hi = n - 1;
//        while (lo < hi)
//        {
//            int mid = (lo + hi) >> 1;
//            if (samples[mid].t < t) lo = mid + 1; else hi = mid;
//        }
//        int i1 = Mathf.Clamp(lo, 1, n - 1), i0 = i1 - 1;
//        float t0 = samples[i0].t, t1 = samples[i1].t, a = Mathf.InverseLerp(t0, t1, t);
//        return Vector2.LerpUnclamped(samples[i0].p, samples[i1].p, a);
//    }

//    void DoDamage(Vector2 origin, Vector2 dir, float length)
//    {
//        if (warmup) return;

//        Vector2 center = origin + dir * (length * 0.5f);
//        var hits = Physics2D.OverlapBoxAll(center, new Vector2(length, thickness * 1.2f),
//                                           Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, playerMask);
//        foreach (var h in hits)
//        {
//            var hp = h.GetComponent<PlayerHealth>() ?? h.GetComponentInParent<PlayerHealth>();
//            if (!hp) continue;
//            hp.TakeDamage(damagePerTick, center);
//            var rb = h.attachedRigidbody;
//            if (rb && knockbackImpulse > 0f) rb.AddForce(dir * knockbackImpulse, ForceMode2D.Impulse);
//        }
//    }

//    void Kill()
//    {
//        active = false;
//        if (line) line.enabled = false;
//        Destroy(gameObject); // destruye también el sprite hijo
//    }

//    public void StopNow(bool destroyImmediately = true)
//    {
//        active = false;
//        if (line) line.enabled = false;
//        if (destroyImmediately) Destroy(gameObject);
//    }

//    void OnDrawGizmosSelected()
//    {
//        if (!owner) return;
//        Vector2 origin = owner.position, dir = (aimDir.sqrMagnitude > 0.0001f ? aimDir : Vector2.right);
//        float length = maxDistance, ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
//        Gizmos.color = Color.red;
//        Gizmos.matrix = Matrix4x4.TRS(origin + dir * (length * 0.5f), Quaternion.Euler(0, 0, ang), Vector3.one);
//        Gizmos.DrawWireCube(Vector3.zero, new Vector2(length, thickness * 1.2f));
//        Gizmos.matrix = Matrix4x4.identity;
//    }
//}

