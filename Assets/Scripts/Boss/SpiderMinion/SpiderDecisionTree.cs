using UnityEngine;

public class SpiderDecisionTree : MonoBehaviour
{
    private IDesitionNode root;
    private SpiderController spider;

    private void Start()
    {
        spider = GetComponent<SpiderController>();
        Build();
    }

    private void Update()
    {
        if (spider == null || root == null)
            return;

        root.Execute();
    }

    private void Build()
    {
        // Acción: morir
        var die = new ActionNode(() => spider.Transition(EnemyInputs.Die));

        // Primera activación: solo si todavía no se activó y ve al jugador
        var activate = new ActionNode(() =>
        {
            if (!spider.IsActivated)
            {
                spider.Transition(EnemyInputs.SeePlayer);
            }
        });

        // Nodo que no hace nada (fallback)
        var doNothing = new ActionNode(() => { });

        // Pregunta: ¿puede activarse (ve al jugador y todavía no se activó)?
        var canActivate = new QuestionNode(activate, doNothing, CanActivate);

        // Pregunta root: ¿está muerto?
        var isDead = new QuestionNode(die, canActivate, IsDead);

        root = isDead;
    }

    private bool IsDead()
    {
        return spider.GetCurrentHealth() <= 0f;
    }

    private bool CanActivate()
    {
        return !spider.IsActivated && CanSeePlayer();
    }

    private bool CanSeePlayer()
    {
        var p = spider.GetPlayer();
        return p != null &&
               Vector2.Distance(spider.transform.position, p.position) <= spider.GetDetectionRadius();
    }
}
