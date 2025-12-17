using UnityEngine;

public class DiabloIdleState : State<EnemyInputs>
{
    private readonly DiabloController ctrl;
    private float timer;

    public DiabloIdleState(DiabloController c) { ctrl = c; }

    public override void Awake()
    {
        base.Awake();
        timer = ctrl.IdleSeconds;

        if (ctrl.Anim)
        {
            //ctrl.Anim.ResetTrigger("Idle");
            //ctrl.Anim.SetTrigger("Idle");
            for (int i = 1; i <= ctrl.AnimCount; i++)
                ctrl.Anim.SetBool($"Anim{i}", false);
            ctrl.Anim.Play("Idle", 0, 0f);
        }

        if (ctrl.Body)
        {
            ctrl.Body.linearVelocity = Vector2.zero;
            ctrl.Body.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
        }
    }

    public override void Execute()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
            ctrl.Transition(EnemyInputs.SpecialAttack); //  Random
    }

    public override void Sleep()
    {
        if (ctrl.Body)
            ctrl.Body.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}
