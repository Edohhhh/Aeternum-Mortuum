using UnityEngine;

public class DiabloDeathState : State<EnemyInputs>
{
    private readonly MonoBehaviour enemy;
    private readonly float fallbackDuration;
    private readonly float destroyDelay;
    private float timer;

    private Animator animator;
    private Rigidbody2D rb2d;
    private Collider2D[] colliders;

    private RigidbodyConstraints2D savedConstraints;
    private float savedGravity;
    private bool hadRb;

    public DiabloDeathState(MonoBehaviour enemy, float fallbackDuration = 1.0f, float destroyDelay = 15.0f)
    {
        this.enemy = enemy;
        this.fallbackDuration = Mathf.Max(0.1f, fallbackDuration);
        this.destroyDelay = Mathf.Max(0f, destroyDelay);
    }

    public override void Awake()
    {
        base.Awake();

        animator = enemy.GetComponent<Animator>();
        rb2d = enemy.GetComponent<Rigidbody2D>();
        colliders = enemy.GetComponentsInChildren<Collider2D>(true);

        if (rb2d != null)
        {
            hadRb = true;
            savedConstraints = rb2d.constraints;
            savedGravity = rb2d.gravityScale;

            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
            rb2d.gravityScale = 0f;
            rb2d.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
        }

        if (colliders != null)
        {
            for (int i = 0; i < colliders.Length; i++)
                colliders[i].enabled = false;
        }

        if (animator != null)
        {
            animator.ResetTrigger("Die");
            animator.SetTrigger("Die");
        }

        // Registrar el estado en el DiabloController para que el Animation Event lo pueda avisar
        (enemy as Component).GetComponent<DiabloController>()?.RegisterDeathState(this);

        timer = 0f;
    }

    public override void Execute()
    {
        timer += Time.deltaTime;

        // Fallback por si no existe anim/evento
        if (animator == null || timer >= fallbackDuration * 3f)
        {
            SafeDestroy();
        }
    }

    public override void Sleep()
    {
        if (hadRb && rb2d != null)
        {
            rb2d.constraints = savedConstraints;
            rb2d.gravityScale = savedGravity;
            rb2d.linearVelocity = Vector2.zero;
        }
        base.Sleep();
    }

    public void OnDeathAnimFinished()
    {
        SafeDestroy();
    }

    private void SafeDestroy()
    {
        if (EnemyManager.Instance)
            EnemyManager.Instance.UnregisterEnemy();

        if (destroyDelay > 0f)
            Object.Destroy(enemy.gameObject, destroyDelay);
        else
            Object.Destroy(enemy.gameObject);
    }
}


//using UnityEngine;

//public class DiabloDeathState : State<EnemyInputs>
//{
//    private readonly DiabloController boss;
//    private readonly float fallbackDuration;
//    private float timer;

//    private Animator animator;
//    private Rigidbody2D rb2d;
//    private Collider2D[] colliders;

//    private RigidbodyConstraints2D savedConstraints;
//    private float savedGravity;
//    private bool hadRb;
//    private bool finished;

//    public DiabloDeathState(DiabloController boss, float fallbackDuration = 1.0f)
//    {
//        this.boss = boss;
//        this.fallbackDuration = Mathf.Max(0.1f, fallbackDuration);
//    }

//    public override void Awake()
//    {
//        base.Awake();

//        animator = boss.GetComponent<Animator>();
//        rb2d = boss.GetComponent<Rigidbody2D>();
//        colliders = boss.GetComponentsInChildren<Collider2D>(true);

//        // Congelar física
//        if (rb2d != null)
//        {
//            hadRb = true;
//            savedConstraints = rb2d.constraints;
//            savedGravity = rb2d.gravityScale;
//            rb2d.linearVelocity = Vector2.zero;
//            rb2d.angularVelocity = 0f;
//            rb2d.gravityScale = 0f;
//            rb2d.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
//        }

//        // Apagar colliders
//        if (colliders != null)
//        {
//            for (int i = 0; i < colliders.Length; i++)
//                colliders[i].enabled = false;
//        }

//        // Lanzar animación "Die"
//        if (animator != null)
//        {
//            animator.ResetTrigger("Die");
//            animator.SetTrigger("Die");
//        }

//        // Registrar este estado en el Diablo para el Animation Event
//       // boss.RegisterDeathState(this);

//        timer = 0f;
//        finished = false;
//    }

//    public override void Execute()
//    {
//        if (finished) return;

//        timer += Time.deltaTime;

//        // Si por algún motivo nunca llega el AnimationEvent,
//        // usamos un fallback (un poco más largo que la duración aproximada).
//        if (animator == null || timer >= fallbackDuration * 3f)
//        {
//            SafeFinish();
//        }
//    }

//    // Animation Event desde el clip de muerte
//    public void OnDeathAnimFinished()
//    {
//        SafeFinish();
//    }

//    private void SafeFinish()
//    {
//        if (finished) return;
//        finished = true;

//        // Opcional: lo sacamos del EnemyManager para que deje de contar como enemigo vivo
//        if (EnemyManager.Instance) EnemyManager.Instance.UnregisterEnemy();

//        // NO destruimos el GameObject del Diablo:
//        // nada de Destroy(boss.gameObject);
//        Debug.Log("[DIABLO] Muerte completada (sin Destroy).");
//    }
//}
