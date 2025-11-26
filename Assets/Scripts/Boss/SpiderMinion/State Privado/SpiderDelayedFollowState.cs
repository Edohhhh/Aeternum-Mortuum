using UnityEngine;

public class SpiderDelayedFollowState : State<EnemyInputs>
{
    private readonly SpiderController controller;
    private readonly Transform player;
    private readonly float speed;
    private readonly float delay;
    private readonly float arriveThreshold;

    private float timer;
    private Vector2 targetPos;
    private Phase currentPhase;

    private enum Phase
    {
        Delay,
        Move
    }

    public SpiderDelayedFollowState(
        SpiderController controller,
        Transform player,
        float speed,
        float delay,
        float arriveThreshold)
    {
        this.controller = controller;
        this.player = player;
        this.speed = speed;
        this.delay = delay;
        this.arriveThreshold = arriveThreshold;
    }

    public override void Awake()
    {
        base.Awake();
        timer = 0f;
        currentPhase = Phase.Delay;

        // Al entrar al estado se marca como activada y se congela la posición objetivo
        controller.MarkActivated();
        if (player != null)
            targetPos = player.position;
        else
            targetPos = controller.transform.position;
    }

    public override void Execute()
    {
        switch (currentPhase)
        {
            case Phase.Delay:
                HandleDelay();
                break;
            case Phase.Move:
                HandleMove();
                break;
        }
    }

    private void HandleDelay()
    {
        timer += Time.deltaTime;
        if (timer >= delay)
        {
            currentPhase = Phase.Move;
        }
    }

    private void HandleMove()
    {
        Vector2 pos = controller.transform.position;
        Vector2 toTarget = targetPos - pos;
        float sqrDist = toTarget.sqrMagnitude;

        if (sqrDist <= arriveThreshold * arriveThreshold)
        {
            // Llegó a destino: deposita telaraña y pasa a Idle
            controller.SpawnWebAtFeet();
            controller.Transition(EnemyInputs.LostPlayer);
            return;
        }

        toTarget.Normalize();
        Vector2 newPos = pos + toTarget * speed * Time.deltaTime;
        controller.transform.position = newPos;
    }

    public override void Sleep()
    {
        base.Sleep();
        // Si quisieras frenar el rigidbody o algo así, podés hacerlo acá.
    }
}
