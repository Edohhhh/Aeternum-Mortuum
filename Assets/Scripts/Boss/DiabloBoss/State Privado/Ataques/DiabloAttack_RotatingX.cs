using UnityEngine;

public class DiabloAttack_RotatingX : IDiabloAttack
{
    private enum Phase { Warning, Spinning }

    public bool IsFinished { get; private set; }

    private DiabloController ctrl;
    private Phase phase;

    private Transform pivot;
    private GameObject beam1GO;
    private GameObject beam2GO;

    private BeamVisual2D beam1;
    private BeamVisual2D beam2;

    private float warnTimer;
    private float spinTimer;

    public void Start(DiabloController c)
    {
        ctrl = c;
        IsFinished = false;
        phase = Phase.Warning;
        warnTimer = 0f;
        spinTimer = 0f;

        ctrl.SpawnExtraEnemiesForAttack(5);

        if (!ctrl || ctrl.A5_BeamPrefab == null)
        {
            Debug.LogError("[DIABLO/RotatingX] Falta ctrl o A5_BeamPrefab.");
            IsFinished = true;
            return;
        }

        // ----- Centro de giro -----
        Vector3 center = (ctrl.A5_Center != null)
            ? ctrl.A5_Center.position
            : (ctrl.transform.position + (Vector3)ctrl.A5_CenterOffset);

        // Pivot vacío
        var pivotGO = new GameObject("RotatingX_Pivot");
        pivot = pivotGO.transform;
        pivot.position = center;

        // ----- Instanciar beams -----
        beam1GO = Object.Instantiate(ctrl.A5_BeamPrefab, pivot);
        beam2GO = Object.Instantiate(ctrl.A5_BeamPrefab, pivot);

        beam1 = beam1GO.GetComponent<BeamVisual2D>();
        beam2 = beam2GO.GetComponent<BeamVisual2D>();

        if (!beam1 || !beam2)
        {
            Debug.LogError("[DIABLO/RotatingX] El prefab A5_BeamPrefab necesita BeamVisual2D.");
            EndAttack();
            return;
        }

        // Ambos salen del centro
        beam1GO.transform.localPosition = Vector3.zero;
        beam2GO.transform.localPosition = Vector3.zero;

        // Orientación X: +45° y -45°
        beam1GO.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        beam2GO.transform.localRotation = Quaternion.Euler(0f, 0f, -45f);

        float len = Mathf.Max(0.1f, ctrl.A5_BeamLength);
        float w = Mathf.Max(0.01f, ctrl.A5_BeamWidth);

        // Tuning de collider desde el controller
        beam1.SetColliderTuning(ctrl.A5_ColliderSizeMul, ctrl.A5_ColliderOffset);
        beam2.SetColliderTuning(ctrl.A5_ColliderSizeMul, ctrl.A5_ColliderOffset);

        // WARNING: sin daño, sin collider, alfa baja
        Color warnCol = new Color(1f, 0f, 0f, 0.30f);
        beam1.ConfigureLocal(len, w, warnCol, damageOn: false, colliderOn: false);
        beam2.ConfigureLocal(len, w, warnCol, damageOn: false, colliderOn: false);
    }

    public void Tick(DiabloController c)
    {
        if (IsFinished || pivot == null) return;

        float dt = Time.deltaTime;

        // Para que puedas editar en vivo A5_ColliderSizeMul/Offset durante Play
        if (beam1) { beam1.SetColliderTuning(ctrl.A5_ColliderSizeMul, ctrl.A5_ColliderOffset); beam1.RefreshColliderNow(); }
        if (beam2) { beam2.SetColliderTuning(ctrl.A5_ColliderSizeMul, ctrl.A5_ColliderOffset); beam2.RefreshColliderNow(); }

        switch (phase)
        {
            case Phase.Warning:
                warnTimer += dt;
                RotatePivot(dt);

                if (warnTimer >= ctrl.A5_WarnTime)
                {
                    float len = Mathf.Max(0.1f, ctrl.A5_BeamLength);
                    float w = Mathf.Max(0.01f, ctrl.A5_BeamWidth);

                    // FIRE: daño ON + collider ON, alfa 1
                    Color fireCol = new Color(1f, 0f, 0f, 1f);
                    beam1.ConfigureLocal(len, w, fireCol, damageOn: true, colliderOn: true);
                    beam2.ConfigureLocal(len, w, fireCol, damageOn: true, colliderOn: true);

                    spinTimer = 0f;
                    phase = Phase.Spinning;
                }
                break;

            case Phase.Spinning:
                spinTimer += dt;
                RotatePivot(dt);

                if (spinTimer >= ctrl.A5_SpinTime)
                    EndAttack();
                break;
        }
    }

    public void Stop(DiabloController c) => EndAttack();

    private void RotatePivot(float dt)
    {
        float speed = ctrl.A5_SpinSpeed;
        pivot.Rotate(0f, 0f, -speed * dt);
    }

    private void EndAttack()
    {
        if (IsFinished) return;
        IsFinished = true;

        if (beam1GO) Object.Destroy(beam1GO);
        if (beam2GO) Object.Destroy(beam2GO);
        if (pivot) Object.Destroy(pivot.gameObject);
    }
}


//using UnityEngine;

//public class DiabloAttack_RotatingX : IDiabloAttack
//{
//    private enum Phase { Warning, Spinning }

//    public bool IsFinished { get; private set; }

//    private DiabloController ctrl;
//    private Phase phase;

//    private Transform pivot;
//    private GameObject beam1;
//    private GameObject beam2;

//    private float warnTimer;
//    private float spinTimer;

//    public void Start(DiabloController c)
//    {
//        ctrl = c;
//        IsFinished = false;
//        phase = Phase.Warning;
//        warnTimer = 0f;
//        spinTimer = 0f;

//        ctrl.SpawnExtraEnemiesForAttack(5);

//        if (ctrl == null)
//        {
//            Debug.LogError("[DIABLO/RotatingX] ctrl es NULL en Start.");
//            IsFinished = true;
//            return;
//        }

//        if (ctrl.A5_BeamPrefab == null)
//        {
//            Debug.LogError("[DIABLO/RotatingX] Falta A5_beamPrefab en el Diablo.");
//            IsFinished = true;
//            return;
//        }

//        // ----- Centro de giro -----
//        Vector3 center;
//        if (ctrl.A5_Center != null)
//        {
//            center = ctrl.A5_Center.position;
//        }
//        else
//        {
//            center = ctrl.transform.position + (Vector3)ctrl.A5_CenterOffset;
//            Debug.LogWarning("[DIABLO/RotatingX] A5_Center está vacío, uso posición del Diablo + offset.");
//        }

//        // Pivot vacío
//        var pivotGO = new GameObject("RotatingX_Pivot");
//        pivot = pivotGO.transform;
//        pivot.position = center;

//        // ----- Instanciar beams -----
//        beam1 = Object.Instantiate(ctrl.A5_BeamPrefab, pivot);
//        beam2 = Object.Instantiate(ctrl.A5_BeamPrefab, pivot);

//        if (beam1 == null || beam2 == null)
//        {
//            Debug.LogError("[DIABLO/RotatingX] Falló Instantiate de los beams.");
//            IsFinished = true;
//            return;
//        }

//        beam1.transform.localPosition = Vector3.zero;
//        beam2.transform.localPosition = Vector3.zero;

//        // X: +45° y -45°
//        beam1.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
//        beam2.transform.localRotation = Quaternion.Euler(0f, 0f, -45f);

//        // Escala
//        float len = Mathf.Max(0.1f, ctrl.A5_BeamLength);
//        float w = Mathf.Max(0.01f, ctrl.A5_BeamWidth);
//        ScaleBeam(beam1.transform, len, w);
//        ScaleBeam(beam2.transform, len, w);

//        // Aviso: sin daño, alfa baja
//        SetBeamDamage(beam1, false);
//        SetBeamDamage(beam2, false);
//        SetBeamAlpha(beam1, 0.3f);
//        SetBeamAlpha(beam2, 0.3f);
//    }

//    public void Tick(DiabloController c)
//    {
//        if (IsFinished || pivot == null)
//            return;

//        float dt = Time.deltaTime;

//        switch (phase)
//        {
//            case Phase.Warning:
//                warnTimer += dt;
//                RotatePivot(dt); // ya gira en el aviso

//                if (warnTimer >= ctrl.A5_WarnTime)
//                {
//                    SetBeamDamage(beam1, true);
//                    SetBeamDamage(beam2, true);
//                    SetBeamAlpha(beam1, 1f);
//                    SetBeamAlpha(beam2, 1f);

//                    spinTimer = 0f;
//                    phase = Phase.Spinning;
//                }
//                break;

//            case Phase.Spinning:
//                spinTimer += dt;
//                RotatePivot(dt);

//                if (spinTimer >= ctrl.A5_SpinTime)
//                {
//                    EndAttack();
//                }
//                break;
//        }
//    }

//    public void Stop(DiabloController c)
//    {
//        EndAttack();
//    }

//    // ================ helpers =================

//    private void RotatePivot(float dt)
//    {
//        float speed = ctrl.A5_SpinSpeed;
//        // horario => negativo en Z
//        pivot.Rotate(0f, 0f, -speed * dt);
//    }

//    private void EndAttack()
//    {
//        if (IsFinished) return;

//        IsFinished = true;

//        if (beam1) Object.Destroy(beam1);
//        if (beam2) Object.Destroy(beam2);
//        if (pivot) Object.Destroy(pivot.gameObject);
//    }

//    private void ScaleBeam(Transform t, float length, float width)
//    {
//        if (t == null) return;
//        var s = t.localScale;
//        s.x = width;
//        s.y = length;
//        t.localScale = s;
//    }

//    private void SetBeamDamage(GameObject go, bool enabled)
//    {
//        if (!go) return;

//        foreach (var col in go.GetComponentsInChildren<Collider2D>())
//            col.enabled = enabled;
//    }

//    private void SetBeamAlpha(GameObject go, float alpha)
//    {
//        if (!go) return;

//        foreach (var sr in go.GetComponentsInChildren<SpriteRenderer>())
//        {
//            var c = sr.color;
//            c.a = alpha;
//            sr.color = c;
//        }
//    }
//}

