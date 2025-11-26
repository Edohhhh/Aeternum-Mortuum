using UnityEngine;

public class SpiderIdleState : State<EnemyInputs>
{
    private readonly SpiderController controller;
    private readonly float idleDuration;
    private float timer;

    private RigidbodyConstraints2D savedConstraints;
    private bool hadBody;

    public SpiderIdleState(SpiderController controller, float idleDuration)
    {
        this.controller = controller;
        this.idleDuration = idleDuration;
    }

    public override void Awake()
    {
        base.Awake();
        timer = 0f;

        // Congelar completamente el rigidbody mientras está en Idle
        var body = controller.Body;
        hadBody = body != null;

        if (hadBody)
        {
            savedConstraints = body.constraints;
            body.linearVelocity = Vector2.zero;
            body.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
        }
    }

    public override void Execute()
    {
        // Antes de activarse por primera vez, solo se queda quieta mirando
        if (!controller.IsActivated)
            return;

        timer += Time.deltaTime;
        if (timer >= idleDuration)
        {
            controller.Transition(EnemyInputs.SeePlayer);
        }
    }

    public override void Sleep()
    {
        base.Sleep();

        // Restaurar constraints cuando sale del Idle (para que pueda volver a moverse)
        if (hadBody && controller.Body != null)
        {
            controller.Body.constraints = savedConstraints;
        }
    }
}

