using UnityEngine;

public class SpiderController : MonoBehaviour, IEnemyDataProvider
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Stats")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float maxSpeed = 3f;

    [Header("Spider Behaviour")]
    [Tooltip("Prefab de la telaraña chica que enlentece (de Valado).")]
    [SerializeField] private GameObject smallWebPrefab;
    [Tooltip("Delay antes de empezar a moverse hacia la última posición del jugador.")]
    [SerializeField] private float followDelay = 1f;
    [Tooltip("Tiempo que la araña se queda en idle entre saltos/movimientos.")]
    [SerializeField] private float idleDuration = 2f;
    [Tooltip("Distancia a la que consideramos que llegó a su objetivo.")]
    [SerializeField] private float arriveThreshold = 0.1f;

    private FSM<EnemyInputs> fsm;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private EnemyHealth health;
    private Rigidbody2D rb;

    // Flag de "ya se activó al ver al jugador por primera vez"
    private bool activated = false;

    public bool IsActivated => activated;
    public void MarkActivated() => activated = true;

    private void Start()
    {
        EnemyManager.Instance.RegisterEnemy();

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        health = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody2D>();

        if (health != null)
            health.OnDeath += () => Transition(EnemyInputs.Die);

        if (rb != null)
            rb.constraints |= RigidbodyConstraints2D.FreezeRotation;

        // Estados
        var idle = new SpiderIdleState(this, idleDuration);
        var follow = new SpiderDelayedFollowState(this, player, maxSpeed, followDelay, arriveThreshold);
        var death = new EnemyDeathState(this);

        fsm = new FSM<EnemyInputs>(idle);

        // Transiciones básicas
        idle.AddTransition(EnemyInputs.SeePlayer, follow);
        idle.AddTransition(EnemyInputs.Die, death);

        follow.AddTransition(EnemyInputs.LostPlayer, idle);
        follow.AddTransition(EnemyInputs.Die, death);

    
    }

    private void Update()
    {
        if (fsm == null) return;

        fsm.Update();

        // Anim caminar
        if (animator != null)
            animator.SetBool("isWalking", fsm.GetCurrentState() is SpiderDelayedFollowState);

        // Flip hacia el jugador
        if (player != null && spriteRenderer != null)
        {
            Vector2 dir = player.position - transform.position;
            spriteRenderer.flipX = dir.x < 0f;
        }
    }

    public void Transition(EnemyInputs input) => fsm.Transition(input);

    // --- Helpers para estados ---
    public Transform Player => player;
    public Animator Anim => animator;
    public Rigidbody2D Body => rb;
    public SpriteRenderer SR => spriteRenderer;

    public void SpawnWebAtFeet()
    {
        if (smallWebPrefab == null) return;
        Instantiate(smallWebPrefab, transform.position, Quaternion.identity);
    }

    // IEnemyDataProvider
    public Transform GetPlayer() => player;
    public float GetDetectionRadius() => detectionRadius;
    public float GetAttackDistance() => detectionRadius;
    public float GetDamage() => 0f; // si usás otro sistema de daño, acá podés cambiarlo
    public float GetMaxSpeed() => maxSpeed;
    public float GetAcceleration() => 0f; // esta araña no usa aceleración suave
    public float GetCurrentHealth() => health != null ? health.GetCurrentHealth() : 0f;

    public bool IsIdle() => fsm.GetCurrentState() is SpiderIdleState;
    public bool IsFollowing() => fsm.GetCurrentState() is SpiderDelayedFollowState;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}

