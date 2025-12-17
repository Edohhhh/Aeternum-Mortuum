using UnityEngine;

public class DiabloAttack_XPlus : IDiabloAttack
{
    private enum Phase { WarnX, FireX, Gap, WarnPlus, FirePlus, End }

    private Phase phase;
    private float t;

    private BeamSegment[] beams = null;
    public bool IsFinished { get; private set; }

    private class BeamSegment
    {
        public GameObject go;
        public BeamVisual2D beam;
    }

    private BeamSegment SpawnBeamBetween(
        DiabloController ctrl,
        Transform from,
        Transform to,
        float width,
        Color color,
        bool damageOn,
        bool colliderOn,
        GameObject prefab
    )
    {
        if (!prefab || !from || !to) return null;

        var go = Object.Instantiate(prefab, ctrl.transform);

        var beam = go.GetComponent<BeamVisual2D>();
        if (!beam)
        {
            Object.Destroy(go);
            Debug.LogWarning("[XPlus] El prefab no tiene BeamVisual2D.");
            return null;
        }

        // Tuning de collider (si tu BeamVisual2D lo usa)
        beam.SetColliderTuning(ctrl.A1_ColliderSizeMul, ctrl.A1_ColliderOffset);

        beam.ConfigureBetween(from.position, to.position, width, color, damageOn, colliderOn);

        return new BeamSegment { go = go, beam = beam };
    }

    private void DestroyBeams()
    {
        if (beams == null) return;

        foreach (var b in beams)
            if (b != null && b.go) Object.Destroy(b.go);

        beams = null;
    }

    public void Start(DiabloController ctrl)
    {
        IsFinished = false;
        t = 0f;
        phase = Phase.WarnX;

        ctrl.SpawnExtraEnemiesForAttack(0);

        // ===== WARN X (diagonales) =====
        beams = new[]
        {
            SpawnBeamBetween(ctrl, ctrl.A1_TopLeft,  ctrl.A1_BottomRight, ctrl.A1_WarnWidth, ctrl.A1_WarnColor,
                damageOn:false, colliderOn:false, prefab: ctrl.A1_WarnBeamPrefab),

            SpawnBeamBetween(ctrl, ctrl.A1_TopRight, ctrl.A1_BottomLeft,  ctrl.A1_WarnWidth, ctrl.A1_WarnColor,
                damageOn:false, colliderOn:false, prefab: ctrl.A1_WarnBeamPrefab),
        };
    }

    public void Tick(DiabloController ctrl)
    {
        if (IsFinished) return;

        // ===== PARA QUE SE VEA EN VIVO EL CAMBIO DE COLLIDER TUNING =====
        // (si modificás A1_ColliderSizeMul / Offset en Play Mode)
        if (beams != null)
        {
            for (int i = 0; i < beams.Length; i++)
                if (beams[i] != null && beams[i].beam != null)
                    beams[i].beam.SetColliderTuning(ctrl.A1_ColliderSizeMul, ctrl.A1_ColliderOffset);
        }

        t += Time.deltaTime;

        switch (phase)
        {
            case Phase.WarnX:
                if (t >= ctrl.A1_WarnTime)
                {
                    // ===== FIRE X (diagonales con daño) =====
                    DestroyBeams();
                    beams = new[]
                    {
                        SpawnBeamBetween(ctrl, ctrl.A1_TopLeft,  ctrl.A1_BottomRight, ctrl.A1_FireWidth, ctrl.A1_FireColor,
                            damageOn:true, colliderOn:true, prefab: ctrl.A1_FireBeamPrefab),

                        SpawnBeamBetween(ctrl, ctrl.A1_TopRight, ctrl.A1_BottomLeft,  ctrl.A1_FireWidth, ctrl.A1_FireColor,
                            damageOn:true, colliderOn:true, prefab: ctrl.A1_FireBeamPrefab),
                    };

                    t = 0f;
                    phase = Phase.FireX;
                }
                break;

            case Phase.FireX:
                if (t >= ctrl.A1_FireTime)
                {
                    DestroyBeams();
                    t = 0f;
                    phase = Phase.Gap;
                }
                break;

            case Phase.Gap:
                if (t >= ctrl.A1_GapAfterX)
                {
                    // ===== WARN PLUS (vertical + horizontal) =====
                    DestroyBeams();
                    beams = new[]
                    {
                        SpawnBeamBetween(ctrl, ctrl.A1_Top,  ctrl.A1_Bottom, ctrl.A1_WarnWidth, ctrl.A1_WarnColor,
                            damageOn:false, colliderOn:false, prefab: ctrl.A1_WarnBeamPrefab),

                        SpawnBeamBetween(ctrl, ctrl.A1_Left, ctrl.A1_Right,  ctrl.A1_WarnWidth, ctrl.A1_WarnColor,
                            damageOn:false, colliderOn:false, prefab: ctrl.A1_WarnBeamPrefab),
                    };

                    t = 0f;
                    phase = Phase.WarnPlus;
                }
                break;

            case Phase.WarnPlus:
                if (t >= ctrl.A1_WarnTime)
                {
                    // ===== FIRE PLUS (vertical + horizontal con daño) =====
                    DestroyBeams();
                    beams = new[]
                    {
                        SpawnBeamBetween(ctrl, ctrl.A1_Top,  ctrl.A1_Bottom, ctrl.A1_FireWidth, ctrl.A1_FireColor,
                            damageOn:true, colliderOn:true, prefab: ctrl.A1_FireBeamPrefab),

                        SpawnBeamBetween(ctrl, ctrl.A1_Left, ctrl.A1_Right,  ctrl.A1_FireWidth, ctrl.A1_FireColor,
                            damageOn:true, colliderOn:true, prefab: ctrl.A1_FireBeamPrefab),
                    };

                    t = 0f;
                    phase = Phase.FirePlus;
                }
                break;

            case Phase.FirePlus:
                if (t >= ctrl.A1_FireTime)
                {
                    DestroyBeams();
                    phase = Phase.End;
                    IsFinished = true;
                }
                break;
        }
    }

    public void Stop(DiabloController ctrl)
    {
        DestroyBeams();
        IsFinished = true;
    }
}
