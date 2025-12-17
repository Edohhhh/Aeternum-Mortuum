using UnityEngine;

public class DiabloController : MonoBehaviour, IEnemyDataProvider
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Ciclo")]
    [SerializeField, Tooltip("Duración de la pausa/idle entre ataques")]
    private float idleSeconds = 5f;

    [SerializeField, Tooltip("Cantidad de animaciones/ataques (1..N)")]
    private int animCount = 6;

    [Header("Ruleta de ataques")]
    [Tooltip("Peso base de cada ataque (1..AnimCount). Si faltan, se asume 1.")]
    [SerializeField] private float[] baseAttackWeights = new float[] { 1, 1, 1, 1, 1, 1 };

    [Tooltip("Multiplicador para el último ataque usado (para evitar repetición). 0.3 = 70% menos probabilidad.")]
    [SerializeField, Range(0f, 1f)]
    private float repeatPenalty = 0.3f;   // ajustable desde el inspector

    private int lastRoll = -1;

    [Header("Vida")]
    [SerializeField] private float detectionRadius = 4f; // no se usa para movimiento, solo por interfaz IEnemyDataProvider
    private EnemyHealth health;

    // ==== Attack 1 — X + Plus (visual, daño y centro) ====
    [Header("Attack 1 — X + Plus")]
    //[SerializeField] private GameObject a1_beamPrefab;
    [SerializeField] private GameObject a1_warnBeamPrefab; // aviso (sprite que quieras)
    [SerializeField] private GameObject a1_fireBeamPrefab; // daño (tu LaserXY)
    [SerializeField] private Transform[] a1_waypointsX = new Transform[4]; // Waypoints para X (4 direcciones diagonales)
    [SerializeField] private Transform[] a1_waypointsPlus = new Transform[4]; // Waypoints para + (4 direcciones ortogonales)

    [SerializeField] private Transform a1_top;
    [SerializeField] private Transform a1_bottom;
    [SerializeField] private Transform a1_left;
    [SerializeField] private Transform a1_right;

    [SerializeField] private Transform a1_topLeft;
    [SerializeField] private Transform a1_topRight;
    [SerializeField] private Transform a1_bottomLeft;
    [SerializeField] private Transform a1_bottomRight;


    [SerializeField] private float a1_warnTime = 0.75f;
    [SerializeField] private float a1_fireTime = 0.85f;
    [SerializeField] private float a1_gapAfterX = 0.20f;

    [Tooltip("Largo total del rayo (en unidades de mundo)")]
    [SerializeField] private float a1_beamLength = 18f;
    [SerializeField] private float a1_warnWidth = 0.12f;
    [SerializeField] private float a1_fireWidth = 0.60f;

    [Header("Attack 1 — Collider tuning")]
    [SerializeField] private Vector2 a1_colliderSizeMul = new Vector2(1f, 1f);
    [SerializeField] private Vector2 a1_colliderOffset = Vector2.zero;

    [Tooltip("Offset LOCAL respecto al Diablo para el origen de los rayos")]
    [SerializeField] private Vector2 a1_centerOffset = Vector2.zero;

    [Tooltip("Color de AVISO (con alfa bajo)")]
    [SerializeField] private Color a1_warnColor = new Color(1f, 0f, 0f, 0.35f);
    [Tooltip("Color de FUEGO (rojo fuerte)")]
    [SerializeField] private Color a1_fireColor = new Color(1f, 0f, 0f, 1f);

    [SerializeField] private Material a1_lineMaterial;

    // Daño (estilo SpikeTrap)
    [Header("Attack 1 — Daño")]
    [SerializeField] private float a1_damagePerSecond = 5f;
    [SerializeField] private float a1_damageInterval = 0.25f;
    [SerializeField] private LayerMask a1_playerMask;

    [Header("Attack 2 — Chess (Grid)")]

    [Header("Prefabs (Chess)")]
    [SerializeField] private GameObject chessWarnPrefab;
    [SerializeField] private GameObject chessFirePrefab;

    [SerializeField] private int ch_rows = 4;
    [SerializeField] private int ch_cols = 6;

    [SerializeField] private float ch_warnTime = 0.8f;     // tiempo de aviso
    [SerializeField] private float ch_fireTime = 0.45f;    // duración del rayo en la celda
    [SerializeField] private int ch_waves = 3;        // cantidad de oleadas
    [SerializeField] private float ch_waveGap = 0.5f;     // espera entre oleadas
    [SerializeField] private bool ch_alternateParity = true; // par/impar alterno

    // Visuales
    [SerializeField] private Color ch_warnColor = new Color(1f, 0.9f, 0.3f, 0.35f);
    [SerializeField] private Color ch_fireColor = new Color(1f, 0.2f, 0.1f, 0.95f);
    [SerializeField] private float ch_tilePadding = 0.06f;    // “marco” interno

    // Daño (usa DevilBeamDamage)
    [SerializeField] private float ch_damagePerSecond = 6f;
    [SerializeField] private float ch_damageInterval = 0.25f;
    [SerializeField] private LayerMask ch_playerMask;

    // Zona de la arena (elige una opción)
    [SerializeField] private bool ch_useArenaCollider = true;
    [SerializeField] private BoxCollider2D ch_arenaCollider;  // si usás collider de sala
    [SerializeField] private Vector2 ch_manualCenter;         // si no usás collider
    [SerializeField] private Vector2 ch_manualSize = new Vector2(9, 6);

    [SerializeField] private bool ch_reWarnEachWave = true;
    [SerializeField] private float ch_reWarnTime = 0.25f;

    // ==== Attack 3 — Spawn + Tornado ====
    [Header("Attack 3 — Spawn + Tornado")]
    [SerializeField] private GameObject a3_minionPrefab;     // enemigo a spawnear
    [SerializeField] private Transform[] a3_enemySpawns;     // 6 waypoints en la arena
    [SerializeField] private GameObject a3_tornadoPrefab;    // prefab del tornado
    [SerializeField] private Transform a3_tornadoCenter;     // centro del tornado
    [SerializeField] private float a3_tornadoLife = 4f;      // duración en segundos

    // ==== Attack 4 — Walls/Hands ====
    [Header("Attack 4 — Walls")]
    [SerializeField] private GameObject a4_leftWallPrefab;   // pared izquierda
    [SerializeField] private GameObject a4_rightWallPrefab;  // pared derecha

    [SerializeField] private Transform a4_leftSpawn;
    [SerializeField] private Transform a4_rightSpawn;

    [Header("Attack 4 — Wave scaling")]
    [SerializeField, Tooltip("Multiplicador de velocidad por ola. Ej: 1.2 => cada ola 20% más rápida")]
    private float a4_speedMulPerWave = 1.2f;

    [SerializeField, Tooltip("Multiplicador de tiempos por ola. Ej: 0.9 => cada ola 10% más corta")]
    private float a4_timeMulPerWave = 0.9f;


    [SerializeField] private Transform a4_leftMid;
    [SerializeField] private Transform a4_rightMid;

    [SerializeField] private float a4_warnTime = 0.6f;   // tiempo de aviso

    // velocidad lenta (hasta mitad y al volver)
    [SerializeField] private float a4_moveSpeed = 6f;

    [SerializeField] private float a4_fastMoveSpeed = 12f;

    [SerializeField] private float a4_holdTime = 0.4f;   // cuánto tiempo quedan apretando
    [SerializeField] private int a4_waves = 3;           // cuántas veces se repite
    [SerializeField] private float a4_waveGap = 1f;      // pausa entre ola y ola

    // ==== Attack 5 — Rotating X ====
    [Header("Attack 5 — Rotating X")]
    [SerializeField] private GameObject a5_beamPrefab; // podés usar la misma "Pared" o uno nuevo
    [SerializeField] private Transform a5_center; 
    [SerializeField] private float a5_warnTime = 0.7f;       // tiempo de aviso
    [SerializeField] private float a5_spinTime = 5f;         // cuánto dura girando con daño
    [SerializeField] private float a5_spinSpeed = 90f;       // grados por segundo (sentido horario)
    [SerializeField] private float a5_beamLength = 10f;      // largo de cada “pata” de la X (escala Y)
    [SerializeField] private float a5_beamWidth = 0.6f;      // ancho (escala X)
    [SerializeField] private Vector2 a5_colliderSizeMul = new Vector2(1f, 1f);
    [SerializeField] private Vector2 a5_colliderOffset = Vector2.zero;

    [SerializeField] private Vector2 a5_centerOffset = Vector2.zero;

    // ==== Attack 6 — Air Punch ====
    [Header("Attack 6 — Air Punch")]
    [SerializeField] private GameObject a6_targetPrefab;     // círculo de aviso (opcional)
    [SerializeField] private GameObject a6_punchPrefab;      // puño que cae

    [SerializeField] private float a6_warnTime = 1.0f;       // tiempo total de aviso
    [SerializeField] private float a6_sampleDelay = 0.35f;   // cuánto antes de terminar el aviso se "congela" la posición
    [SerializeField] private float a6_fallHeight = 8f;       // desde qué altura cae el puño (en unidades de mundo)
    [SerializeField] private float a6_gravity = 40f;         // "fuerza" de caída (aceleración)
    [SerializeField] private float a6_impactHoldTime = 0.2f; // cuánto tiempo queda apoyado en el suelo antes de subir
    [SerializeField] private float a6_riseSpeed = 12f;       // velocidad a la que sube
    [SerializeField] private float a6_riseDistance = 6f;     // cuánto sube antes de desaparecer

    [SerializeField] private int a6_waves = 3;             // cuántos golpes
    [SerializeField] private float a6_waveGap = 0.5f;        // pausa entre golpes

    [SerializeField] private float a6_damageRadius = 1.0f;   // radio de daño del impacto
    [SerializeField] private int a6_damage = 1;            // daño del impacto
    [SerializeField] private LayerMask a6_playerMask;        // capa del jugador


    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer sr;

    [System.Serializable]
    public class ExtraSpawn
    {
        public string name = "Extra Spawn";

        [Tooltip("Prefabs de enemigos que se pueden spawnear en este ataque")]
        public GameObject[] prefabs;

        [Tooltip("Puntos donde aparecerán los enemigos")]
        public Transform[] points;
    }
    [Header("Extra enemy spawns (por ataque)")]
    [SerializeField]
    private ExtraSpawn[] extraSpawns;
    public ExtraSpawn[] ExtraSpawns => extraSpawns;


    private float idleCharge = 0f;
    public bool IsIdling() => fsm.GetCurrentState() is EnemyIdleState || fsm.GetCurrentState() is DiabloIdleState;
    public bool IsAnimating() => fsm.GetCurrentState() is DiabloAnimState;
    public bool IsAttacking() => fsm.GetCurrentState() is DiabloAttackRouterState;

    public bool CanStartCycle() => idleCharge >= idleSeconds;
    public void MarkCycleStarted() => idleCharge = 0f;

    // Exponer por si el árbol quiere ver hp
    public float HP => GetCurrentHealth();

    // Runtime
    private FSM<EnemyInputs> fsm;
    //private Animator animator;
    private Rigidbody2D rb;
    //private SpriteRenderer sr;

    // “Número elegido” por Random (1..animCount)
    public int Roll { get; private set; } = -1;

    // Refs de estados para reenviar Animation Events
    private DiabloAnimState _animRef;
    private DiabloAttackState _attackRef;
    private DiabloDeathState _deathRef;

    // ========= Unity =========
    private void Awake()
    {
        //animator = GetComponent<Animator>();
        if (!animator) animator = GetComponentInChildren<Animator>(true);
        rb = GetComponent<Rigidbody2D>();
        //sr = GetComponent<SpriteRenderer>();
        if (!sr) sr = animator ? animator.GetComponent<SpriteRenderer>() : GetComponentInChildren<SpriteRenderer>(true);

        health = GetComponent<EnemyHealth>();

        if (rb) rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
    }

    private void Start()
    {
        EnemyManager.Instance.RegisterEnemy();
        if (health) health.OnDeath += () => Transition(EnemyInputs.Die);

        // --- Estados ---
        var idle = new DiabloIdleState(this);
        var rand = new DiabloRandomState(this);
        var animSt = new DiabloAnimState(this);
        var atk = new DiabloAttackRouterState(this);
        var death = new DiabloDeathState(this);

        fsm = new FSM<EnemyInputs>(idle);

        // --- Transiciones (simples y encadenadas por eventos) ---
        // Idle (timer)  Random
        idle.AddTransition(EnemyInputs.SpecialAttack, rand);
        idle.AddTransition(EnemyInputs.Die, death);

        // Random (elige número)  Anim
        rand.AddTransition(EnemyInputs.SpecialAttack, animSt);
        rand.AddTransition(EnemyInputs.Die, death);

        // Anim (evento OnAnimEnd) Attack
        animSt.AddTransition(EnemyInputs.SpecialAttack, atk);
        animSt.AddTransition(EnemyInputs.Die, death);

        // Attack (evento OnAttackEnd)  Idle
        atk.AddTransition(EnemyInputs.SeePlayer, idle); // usamos SeePlayer como “volver a Idle”
        atk.AddTransition(EnemyInputs.Die, death);

        RegisterDeathState(death);

        // (opcional) set de anim inicial
        if (animator) animator.Play("Idle", 0, 0f);
    }

    private void Update()
    {
        if (GetCurrentHealth() <= 0f) { Transition(EnemyInputs.Die); }
        fsm.Update();

        // carga de pausa
        if (IsIdling() && !IsAnimating() && !IsAttacking())
            idleCharge += Time.deltaTime;

        //if (player && sr)
        //{
        //    Vector2 dir = player.position - transform.position;
        //    sr.flipX = dir.x < 0;
        //}
    }

    private void FixedUpdate() => fsm.FixedUpdate();

    //public void Transition(EnemyInputs input) => fsm?.Transition(input);
    public void Transition(EnemyInputs input)
    {
        var cur = fsm != null ? fsm.GetCurrentState()?.GetType().Name : "null";
        Debug.Log($"[FSM] Transition({input}) requested from {cur}");
        fsm?.Transition(input);
    }

    // ========= API para estados =========
    public Animator Anim => animator;
    public Rigidbody2D Body => rb;
    public Transform Player => player;
    public float IdleSeconds => idleSeconds;
    public int AnimCount => Mathf.Max(1, animCount);

    // Elige número 1..animCount (sin animación)
    public int DoRoll()
    {
        int n = AnimCount;                 // cantidad de ataques (1..n)
        if (n <= 0) n = 1;

        // Aseguramos que haya algún peso base
        if (baseAttackWeights == null || baseAttackWeights.Length == 0)
        {
            baseAttackWeights = new float[n];
            for (int i = 0; i < n; i++)
                baseAttackWeights[i] = 1f;
        }

        // 1) Construimos la tabla de pesos para este roll
        //    = pesos base, pero penalizando el último ataque usado.
        float[] weights = new float[n];
        for (int i = 0; i < n; i++)
        {
            // si faltan entradas en el array, tomamos peso 1
            float w = (i < baseAttackWeights.Length) ? baseAttackWeights[i] : 1f;
            if (w < 0f) w = 0f;

            // penalizamos SOLO el último ataque usado
            if (lastRoll == (i + 1))
            {
                w *= repeatPenalty;        // por ej. 0.3 = 70% menos probabilidad
            }

            // que nunca quede exactamente en 0
            if (w <= 0f) w = 0.0001f;

            weights[i] = w;
        }

        // 2) Hacemos el random ponderado
        float total = 0f;
        for (int i = 0; i < n; i++)
            total += weights[i];

        float r = Random.Range(0f, total);
        int chosenIndex = 0;
        float accum = 0f;

        for (int i = 0; i < n; i++)
        {
            accum += weights[i];
            if (r <= accum)
            {
                chosenIndex = i;
                break;
            }
        }

        int result = chosenIndex + 1; // porque tus ataques son 1..AnimCount

        Roll = result;
        lastRoll = result;            // recordamos cuál salió para penalizarlo en el próximo roll

        return result;
    }

    // ========= Animation Events (desde clips) =========
    public void RegisterAnimState(DiabloAnimState s) => _animRef = s;
    public void RegisterAttackState(DiabloAttackState s) => _attackRef = s;
    public void RegisterDeathState(DiabloDeathState s) => _deathRef = s;

    // Llamar al final de cada clip de presentación (Anim1..Anim6)
    public void OnAnimEnd()
    {
        Debug.Log("[DIABLO] OnAnimEnd event");
        // Entra directo al AttackRouterState (Anim -> AttackRouter)
        //Transition(EnemyInputs.SpecialAttack);
        _animRef?.OnAnimFinished();
    }
    

    // Llamar al final del clip de ataque (Attack1..Attack6)
    public void OnAttackEnd() => _attackRef?.OnAttackFinished();

    // EnemyDeathState: Animation Event final de muerte
    public void OnDeathAnimFinished() => _deathRef?.OnDeathAnimFinished();

    // Getters Attack 1 

    //public GameObject A1_BeamPrefab => a1_beamPrefab;

    public GameObject A1_WarnBeamPrefab => a1_warnBeamPrefab;
    public GameObject A1_FireBeamPrefab => a1_fireBeamPrefab;

    public Transform[] A1_WaypointsX => a1_waypointsX;
    public Transform[] A1_WaypointsPlus => a1_waypointsPlus;

    public Transform A1_Top => a1_top;
    public Transform A1_Bottom => a1_bottom;
    public Transform A1_Left => a1_left;
    public Transform A1_Right => a1_right;

    public Transform A1_TopLeft => a1_topLeft;
    public Transform A1_TopRight => a1_topRight;
    public Transform A1_BottomLeft => a1_bottomLeft;
    public Transform A1_BottomRight => a1_bottomRight;
    public float A1_WarnTime => a1_warnTime;
    public float A1_FireTime => a1_fireTime;
    public float A1_GapAfterX => a1_gapAfterX;
    public float A1_BeamLength => a1_beamLength;
    public float A1_WarnWidth => a1_warnWidth;
    public float A1_FireWidth => a1_fireWidth;
    public Vector2 A1_CenterOffset => a1_centerOffset;

    public Vector2 A1_ColliderSizeMul => a1_colliderSizeMul;
    public Vector2 A1_ColliderOffset => a1_colliderOffset;

    public Color A1_WarnColor => a1_warnColor;
    public Color A1_FireColor => a1_fireColor;
    public Material A1_LineMaterial => a1_lineMaterial;

    public float A1_DamagePerSecond => a1_damagePerSecond;
    public float A1_DamageInterval => a1_damageInterval;
    public LayerMask A1_PlayerMask => a1_playerMask;

    // Getters Attack 2

    public GameObject ChessWarnPrefab => chessWarnPrefab;
    public GameObject ChessFirePrefab => chessFirePrefab;
    public int Ch_Rows => Mathf.Max(1, ch_rows);
    public int Ch_Cols => Mathf.Max(1, ch_cols);
    public float Ch_WarnTime => ch_warnTime;
    public float Ch_FireTime => ch_fireTime;
    public int Ch_Waves => Mathf.Max(1, ch_waves);
    public float Ch_WaveGap => ch_waveGap;
    public bool Ch_AlternateParity => ch_alternateParity;
    public Color Ch_WarnColor => ch_warnColor;
    public Color Ch_FireColor => ch_fireColor;
    public float Ch_TilePadding => ch_tilePadding;
    public float Ch_DamagePerSecond => ch_damagePerSecond;
    public float Ch_DamageInterval => ch_damageInterval;
    public LayerMask Ch_PlayerMask => ch_playerMask;

    public bool Ch_UseArenaCollider => ch_useArenaCollider;
    public BoxCollider2D Ch_ArenaCollider => ch_arenaCollider;
    public Vector2 Ch_ManualCenter => ch_manualCenter;
    public Vector2 Ch_ManualSize => ch_manualSize;

    public bool Ch_ReWarnEachWave => ch_reWarnEachWave;
    public float Ch_ReWarnTime => ch_reWarnTime;

    // Getters Attack 3
    public GameObject A3_MinionPrefab => a3_minionPrefab;
    public Transform[] A3_EnemySpawns => a3_enemySpawns;
    public GameObject A3_TornadoPrefab => a3_tornadoPrefab;
    public Transform A3_TornadoCenter => a3_tornadoCenter;
    public float A3_TornadoLife => a3_tornadoLife;

    // Getters Attack 4
    public GameObject A4_LeftWallPrefab => a4_leftWallPrefab;
    public GameObject A4_RightWallPrefab => a4_rightWallPrefab;

    public Transform A4_LeftSpawn => a4_leftSpawn;
    public Transform A4_RightSpawn => a4_rightSpawn;

    public Transform A4_LeftMid => a4_leftMid;
    public Transform A4_RightMid => a4_rightMid;

    public float A4_WarnTime => a4_warnTime;
    public float A4_MoveSpeed => a4_moveSpeed;
    public float A4_FastMoveSpeed => a4_fastMoveSpeed;
    public float A4_HoldTime => a4_holdTime;
    public int A4_Waves => a4_waves;
    public float A4_WaveGap => a4_waveGap;
    public float A4_SpeedMulPerWave => a4_speedMulPerWave;
    public float A4_TimeMulPerWave => a4_timeMulPerWave;

    // Getters Attack 5
    public GameObject A5_BeamPrefab => a5_beamPrefab;

    public Transform A5_Center => a5_center;
    public float A5_WarnTime => a5_warnTime;
    public float A5_SpinTime => a5_spinTime;
    public float A5_SpinSpeed => a5_spinSpeed;
    public float A5_BeamLength => a5_beamLength;
    public float A5_BeamWidth => a5_beamWidth;

    public Vector2 A5_ColliderSizeMul => a5_colliderSizeMul;
    public Vector2 A5_ColliderOffset => a5_colliderOffset;
    public Vector2 A5_CenterOffset => a5_centerOffset;

    // Getters Attack 6
    public GameObject A6_TargetPrefab => a6_targetPrefab;
    public GameObject A6_PunchPrefab => a6_punchPrefab;

    public float A6_WarnTime => a6_warnTime;
    public float A6_SampleDelay => a6_sampleDelay;
    public float A6_FallHeight => a6_fallHeight;
    public float A6_Gravity => a6_gravity;
    public float A6_ImpactHoldTime => a6_impactHoldTime;
    public float A6_RiseSpeed => a6_riseSpeed;
    public float A6_RiseDistance => a6_riseDistance;

    public int A6_Waves => Mathf.Max(1, a6_waves);
    public float A6_WaveGap => a6_waveGap;

    public float A6_DamageRadius => a6_damageRadius;
    public int A6_Damage => a6_damage;
    public LayerMask A6_PlayerMask => a6_playerMask;

    public void SpawnExtraEnemiesForAttack(int attackIndex)
    {
        if (extraSpawns == null) return;
        if (attackIndex < 0 || attackIndex >= extraSpawns.Length) return;

        var set = extraSpawns[attackIndex];
        if (set == null) return;

        if (set.prefabs == null || set.prefabs.Length == 0) return;
        if (set.points == null || set.points.Length == 0) return;

        for (int i = 0; i < set.points.Length; i++)
        {
            Transform p = set.points[i];
            if (!p) continue;

            // Elegís cómo seleccionar el prefab:
            GameObject prefab = set.prefabs[i % set.prefabs.Length];   // cíclico
                                                                       // GameObject prefab = set.prefabs[Random.Range(0, set.prefabs.Length)]; // aleatorio

            if (!prefab) continue;

            Object.Instantiate(prefab, p.position, p.rotation);
        }
    }

    // ========= IEnemyDataProvider (mínimo requerido) =========
    public Transform GetPlayer() => player;
    public float GetDetectionRadius() => detectionRadius;
    public float GetAttackDistance() => detectionRadius;
    public float GetDamage() => 0f;
    public float GetMaxSpeed() => 0f;          // es estático
    public float GetAcceleration() => 0f;      // es estático
    public float GetCurrentHealth() => health ? health.GetCurrentHealth() : 0f;
}