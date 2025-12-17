//using UnityEngine;

//public class DiabloRandomState : State<EnemyInputs>
//{
//    private readonly DiabloController ctrl;
//    public DiabloRandomState(DiabloController c) { ctrl = c; }

//    public override void Awake()
//    {
//        base.Awake();
//        int id = ctrl.DoRoll();          // 1..N (guarda en ctrl.Roll)
//        // Podés loguear si querés: Debug.Log($"[DIABLO] Roll={id}");
//        ctrl.Transition(EnemyInputs.SpecialAttack); //  Anim
//    }
//}
public class DiabloRandomState : State<EnemyInputs>
{
    private readonly DiabloController ctrl;
    private bool done;

    public DiabloRandomState(DiabloController c) { ctrl = c; }

    public override void Awake()
    {
        base.Awake();
        ctrl.DoRoll();   // solo decide el número
        done = false;
    }

    public override void Execute()
    {
        if (done) return;
        done = true;
        ctrl.Transition(EnemyInputs.SpecialAttack); // ahora sí
    }
}
