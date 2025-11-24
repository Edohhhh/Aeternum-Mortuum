using UnityEngine;

public class DiabloAttack_Walls : IDiabloAttack
{
    public bool IsFinished { get; private set; }

    private DiabloController ctrl;

    // Fases:
    //  - Warning: delay antes de que aparezcan
    //  - SlowIn: van desde spawn -> mid (lento)
    //  - BackOut: vuelven desde mid -> spawn (lento)
    //  - FastIn: van desde spawn -> centro (rápido, se frenan al chocar)
    //  - Holding: se quedan un rato apretando (acá hacemos fade out)
    //  - Gap: pausa entre olas
    private enum Phase { Warning, SlowIn, BackOut, FastIn, Holding, Gap }
    private Phase phase;

    private float timer;
    private int waveIndex;

    private GameObject leftInst;
    private GameObject rightInst;

    // Para fallback si no hay mid-points
    private bool useFancyMovement = false;

    // Fade
    private float fadeInDuration = 0.25f;
    private float fadeOutDuration = 0.25f;

    public void Start(DiabloController c)
    {
        ctrl = c;
        IsFinished = false;
        waveIndex = 0;
        StartWarning();

        // enemigos extra configurados para este ataque
        ctrl.SpawnExtraEnemiesForAttack(4);
    }

    public void Tick(DiabloController c)
    {
        if (IsFinished) return;

        float dt = Time.deltaTime;
        timer += dt;

        switch (phase)
        {
            case Phase.Warning:
                UpdateWarning();
                break;

            case Phase.SlowIn:
                UpdateSlowIn(dt);
                break;

            case Phase.BackOut:
                UpdateBackOut(dt);
                break;

            case Phase.FastIn:
                UpdateFastIn(dt);
                break;

            case Phase.Holding:
                UpdateHolding();
                break;

            case Phase.Gap:
                UpdateGap();
                break;
        }
    }

    public void Stop(DiabloController c)
    {
        DestroyWalls();
        IsFinished = true;
    }

    // ---------- Helpers de fade ----------

    private void SetAlpha(GameObject go, float alpha)
    {
        if (!go) return;
        var sr = go.GetComponentInChildren<SpriteRenderer>();
        if (!sr) return;

        var col = sr.color;
        col.a = alpha;
        sr.color = col;
    }

    private void SetBothAlpha(float alpha)
    {
        SetAlpha(leftInst, alpha);
        SetAlpha(rightInst, alpha);
    }

    // ---------- FASE WARNING ----------

    private void StartWarning()
    {
        phase = Phase.Warning;
        timer = 0f;
        Debug.Log($"[DIABLO/Walls] Wave {waveIndex + 1} WARNING");
    }

    private void UpdateWarning()
    {
        if (timer >= ctrl.A4_WarnTime)
        {
            StartWave();
        }
    }

    // ---------- SPAWN DE PAREDES ----------

    private void StartWave()
    {
        timer = 0f;

        if (ctrl.A4_LeftWallPrefab == null ||
            ctrl.A4_RightWallPrefab == null ||
            ctrl.A4_LeftSpawn == null ||
            ctrl.A4_RightSpawn == null)
        {
            Debug.LogWarning("[DIABLO/Walls] Faltan referencias. Cancelando ataque.");
            IsFinished = true;
            return;
        }

        leftInst = Object.Instantiate(
            ctrl.A4_LeftWallPrefab,
            ctrl.A4_LeftSpawn.position,
            ctrl.A4_LeftSpawn.rotation);

        rightInst = Object.Instantiate(
            ctrl.A4_RightWallPrefab,
            ctrl.A4_RightSpawn.position,
            ctrl.A4_RightSpawn.rotation);

        // Al spawnear: arrancan invisibles (alpha 0)
        SetBothAlpha(0f);

        useFancyMovement = (ctrl.A4_LeftMid && ctrl.A4_RightMid);

        if (useFancyMovement)
        {
            phase = Phase.SlowIn;
        }
        else
        {
            // si no hay mid-points, vamos directo al comportamiento clásico
            phase = Phase.FastIn;
        }

        Debug.Log($"[DIABLO/Walls] Spawn walls L={leftInst.transform.position} R={rightInst.transform.position}");
    }

    // ---------- MOVIMIENTO LENTO HASTA LA MITAD ----------

    private void UpdateSlowIn(float dt)
    {
        if (!leftInst || !rightInst)
        {
            StartGapAfterError();
            return;
        }

        float speed = ctrl.A4_MoveSpeed;

        // ir desde spawn -> mid
        Vector3 lTarget = ctrl.A4_LeftMid.position;
        Vector3 rTarget = ctrl.A4_RightMid.position;

        leftInst.transform.position =
            Vector3.MoveTowards(leftInst.transform.position, lTarget, speed * dt);

        rightInst.transform.position =
            Vector3.MoveTowards(rightInst.transform.position, rTarget, speed * dt);

        // FADE IN mientras están en SlowIn
        float tFade = Mathf.Clamp01(timer / fadeInDuration);
        SetBothAlpha(tFade);

        bool leftReached = Vector3.Distance(leftInst.transform.position, lTarget) < 0.01f;
        bool rightReached = Vector3.Distance(rightInst.transform.position, rTarget) < 0.01f;

        // cuando ambas llegaron a mitad, pasamos a BackOut
        if (leftReached && rightReached)
        {
            phase = Phase.BackOut;
            timer = 0f;
        }
    }

    // ---------- VUELTA HACIA ATRÁS (MID -> SPAWN) ----------

    private void UpdateBackOut(float dt)
    {
        if (!leftInst || !rightInst)
        {
            StartGapAfterError();
            return;
        }

        float speed = ctrl.A4_MoveSpeed;

        Vector3 lTarget = ctrl.A4_LeftSpawn.position;
        Vector3 rTarget = ctrl.A4_RightSpawn.position;

        leftInst.transform.position =
            Vector3.MoveTowards(leftInst.transform.position, lTarget, speed * dt);

        rightInst.transform.position =
            Vector3.MoveTowards(rightInst.transform.position, rTarget, speed * dt);

        // En BackOut ya están full alpha (1)
        SetBothAlpha(1f);

        bool leftReached = Vector3.Distance(leftInst.transform.position, lTarget) < 0.01f;
        bool rightReached = Vector3.Distance(rightInst.transform.position, rTarget) < 0.01f;

        if (leftReached && rightReached)
        {
            // ahora sí, rush rápido al centro
            phase = Phase.FastIn;
            timer = 0f;
        }
    }

    // ---------- RUSH RÁPIDO AL CENTRO + COLISIÓN ----------

    private void UpdateFastIn(float dt)
    {
        if (!leftInst || !rightInst)
        {
            StartGapAfterError();
            return;
        }

        float speed = useFancyMovement ? ctrl.A4_FastMoveSpeed : ctrl.A4_MoveSpeed;

        Vector3 lp = leftInst.transform.position;
        Vector3 rp = rightInst.transform.position;

        lp.x += speed * dt;
        rp.x -= speed * dt;

        leftInst.transform.position = lp;
        rightInst.transform.position = rp;

        // aseguramos que estén visibles mientras rushéan
        SetBothAlpha(1f);

        // Comprobamos colisión usando los colliders
        var leftCol = leftInst.GetComponent<Collider2D>();
        var rightCol = rightInst.GetComponent<Collider2D>();

        bool collided = false;

        if (leftCol && rightCol)
        {
            Bounds lb = leftCol.bounds;
            Bounds rb = rightCol.bounds;

            collided = lb.max.x >= rb.min.x;

            if (collided)
            {
                // ajustar para que queden justito tocándose
                float overlap = lb.max.x - rb.min.x;
                if (overlap > 0f)
                {
                    float half = overlap * 0.5f;

                    lp = leftInst.transform.position;
                    rp = rightInst.transform.position;

                    lp.x -= half;
                    rp.x += half;

                    leftInst.transform.position = lp;
                    rightInst.transform.position = rp;
                }
            }
        }
        else
        {
            // Fallback si falta algún collider
            collided = lp.x >= rp.x;
        }

        if (collided)
        {
            Debug.Log("[DIABLO/Walls] Colliders contact → holding");
            phase = Phase.Holding;
            timer = 0f;
        }
    }

    // ---------- HOLDING (SE QUEDAN APRETANDO + FADE OUT) ----------

    private void UpdateHolding()
    {
        if (!leftInst || !rightInst)
        {
            StartGapAfterError();
            return;
        }

        // tiempo total que se quedan apretando
        float holdTime = ctrl.A4_HoldTime;

        // Durante los primeros fadeOutDuration segundos, vamos bajando alpha de 1 a 0
        float tFade = Mathf.Clamp01(timer / fadeOutDuration);
        float alpha = Mathf.Lerp(1f, 0f, tFade);
        SetBothAlpha(alpha);

        if (timer >= holdTime)
        {
            DestroyWalls();
            phase = Phase.Gap;
            timer = 0f;
        }
    }

    // ---------- GAP ENTRE OLAS ----------

    private void UpdateGap()
    {
        if (timer >= ctrl.A4_WaveGap)
        {
            waveIndex++;

            if (waveIndex < ctrl.A4_Waves)
            {
                StartWarning();
            }
            else
            {
                Debug.Log("[DIABLO/Walls] Ataque completo");
                IsFinished = true;
            }
        }
    }

    private void StartGapAfterError()
    {
        // Si por algún motivo las paredes desaparecen antes,
        // evitamos que se quede colgado el ataque.
        DestroyWalls();
        phase = Phase.Gap;
        timer = 0f;
    }

    private void DestroyWalls()
    {
        if (leftInst) Object.Destroy(leftInst);
        if (rightInst) Object.Destroy(rightInst);

        leftInst = null;
        rightInst = null;
    }
}

//using UnityEngine;

//public class DiabloAttack_Walls : IDiabloAttack
//{
//    public bool IsFinished { get; private set; }

//    private DiabloController ctrl;

//    // Fases:
//    //  - Warning: delay antes de que aparezcan
//    //  - SlowIn: van desde spawn -> mid (lento)
//    //  - BackOut: vuelven desde mid -> spawn (lento)
//    //  - FastIn: van desde spawn -> centro (rápido, se frenan al chocar)
//    //  - Holding: se quedan un rato apretando
//    //  - Gap: pausa entre olas
//    private enum Phase { Warning, SlowIn, BackOut, FastIn, Holding, Gap }
//    private Phase phase;

//    private float timer;
//    private int waveIndex;

//    private GameObject leftInst;
//    private GameObject rightInst;

//    // Para fallback si no hay mid-points
//    private bool useFancyMovement = false;

//    public void Start(DiabloController c)
//    {
//        ctrl = c;
//        IsFinished = false;
//        waveIndex = 0;
//        StartWarning();

//        // enemigos extra configurados para este ataque
//        ctrl.SpawnExtraEnemiesForAttack(4);
//    }

//    public void Tick(DiabloController c)
//    {
//        if (IsFinished) return;

//        float dt = Time.deltaTime;
//        timer += dt;

//        switch (phase)
//        {
//            case Phase.Warning:
//                UpdateWarning();
//                break;

//            case Phase.SlowIn:
//                UpdateSlowIn(dt);
//                break;

//            case Phase.BackOut:
//                UpdateBackOut(dt);
//                break;

//            case Phase.FastIn:
//                UpdateFastIn(dt);
//                break;

//            case Phase.Holding:
//                UpdateHolding();
//                break;

//            case Phase.Gap:
//                UpdateGap();
//                break;
//        }
//    }

//    public void Stop(DiabloController c)
//    {
//        DestroyWalls();
//        IsFinished = true;
//    }

//    // ---------- FASE WARNING ----------

//    private void StartWarning()
//    {
//        phase = Phase.Warning;
//        timer = 0f;
//        Debug.Log($"[DIABLO/Walls] Wave {waveIndex + 1} WARNING");
//    }

//    private void UpdateWarning()
//    {
//        if (timer >= ctrl.A4_WarnTime)
//        {
//            StartWave();
//        }
//    }

//    // ---------- SPAWN DE PAREDES ----------

//    private void StartWave()
//    {
//        timer = 0f;

//        if (ctrl.A4_LeftWallPrefab == null ||
//            ctrl.A4_RightWallPrefab == null ||
//            ctrl.A4_LeftSpawn == null ||
//            ctrl.A4_RightSpawn == null)
//        {
//            Debug.LogWarning("[DIABLO/Walls] Faltan referencias. Cancelando ataque.");
//            IsFinished = true;
//            return;
//        }

//        leftInst = Object.Instantiate(
//            ctrl.A4_LeftWallPrefab,
//            ctrl.A4_LeftSpawn.position,
//            ctrl.A4_LeftSpawn.rotation);

//        rightInst = Object.Instantiate(
//            ctrl.A4_RightWallPrefab,
//            ctrl.A4_RightSpawn.position,
//            ctrl.A4_RightSpawn.rotation);

//        useFancyMovement = (ctrl.A4_LeftMid && ctrl.A4_RightMid);

//        if (useFancyMovement)
//        {
//            phase = Phase.SlowIn;
//        }
//        else
//        {
//            // si no hay mid-points, vamos directo al comportamiento clásico
//            phase = Phase.FastIn;
//        }

//        Debug.Log($"[DIABLO/Walls] Spawn walls L={leftInst.transform.position} R={rightInst.transform.position}");
//    }

//    // ---------- MOVIMIENTO LENTO HASTA LA MITAD ----------

//    private void UpdateSlowIn(float dt)
//    {
//        if (!leftInst || !rightInst)
//        {
//            StartGapAfterError();
//            return;
//        }

//        float speed = ctrl.A4_MoveSpeed;

//        // ir desde spawn -> mid
//        Vector3 lTarget = ctrl.A4_LeftMid.position;
//        Vector3 rTarget = ctrl.A4_RightMid.position;

//        leftInst.transform.position =
//            Vector3.MoveTowards(leftInst.transform.position, lTarget, speed * dt);

//        rightInst.transform.position =
//            Vector3.MoveTowards(rightInst.transform.position, rTarget, speed * dt);

//        bool leftReached = Vector3.Distance(leftInst.transform.position, lTarget) < 0.01f;
//        bool rightReached = Vector3.Distance(rightInst.transform.position, rTarget) < 0.01f;

//        // cuando ambas llegaron a mitad, pasamos a BackOut
//        if (leftReached && rightReached)
//        {
//            phase = Phase.BackOut;
//            timer = 0f;
//        }
//    }

//    // ---------- VUELTA HACIA ATRÁS (MID -> SPAWN) ----------

//    private void UpdateBackOut(float dt)
//    {
//        if (!leftInst || !rightInst)
//        {
//            StartGapAfterError();
//            return;
//        }

//        float speed = ctrl.A4_MoveSpeed;

//        Vector3 lTarget = ctrl.A4_LeftSpawn.position;
//        Vector3 rTarget = ctrl.A4_RightSpawn.position;

//        leftInst.transform.position =
//            Vector3.MoveTowards(leftInst.transform.position, lTarget, speed * dt);

//        rightInst.transform.position =
//            Vector3.MoveTowards(rightInst.transform.position, rTarget, speed * dt);

//        bool leftReached = Vector3.Distance(leftInst.transform.position, lTarget) < 0.01f;
//        bool rightReached = Vector3.Distance(rightInst.transform.position, rTarget) < 0.01f;

//        if (leftReached && rightReached)
//        {
//            // ahora sí, rush rápido al centro
//            phase = Phase.FastIn;
//            timer = 0f;
//        }
//    }

//    // ---------- RUSH RÁPIDO AL CENTRO + COLISIÓN ----------

//    private void UpdateFastIn(float dt)
//    {
//        if (!leftInst || !rightInst)
//        {
//            StartGapAfterError();
//            return;
//        }

//        float speed = useFancyMovement ? ctrl.A4_FastMoveSpeed : ctrl.A4_MoveSpeed;

//        Vector3 lp = leftInst.transform.position;
//        Vector3 rp = rightInst.transform.position;

//        lp.x += speed * dt;
//        rp.x -= speed * dt;

//        leftInst.transform.position = lp;
//        rightInst.transform.position = rp;

//        // Comprobamos colisión usando los colliders
//        var leftCol = leftInst.GetComponent<Collider2D>();
//        var rightCol = rightInst.GetComponent<Collider2D>();

//        bool collided = false;

//        if (leftCol && rightCol)
//        {
//            Bounds lb = leftCol.bounds;
//            Bounds rb = rightCol.bounds;

//            collided = lb.max.x >= rb.min.x;

//            if (collided)
//            {
//                // ajustar para que queden justito tocándose
//                float overlap = lb.max.x - rb.min.x;
//                if (overlap > 0f)
//                {
//                    float half = overlap * 0.5f;

//                    lp = leftInst.transform.position;
//                    rp = rightInst.transform.position;

//                    lp.x -= half;
//                    rp.x += half;

//                    leftInst.transform.position = lp;
//                    rightInst.transform.position = rp;
//                }
//            }
//        }
//        else
//        {
//            // Fallback si falta algún collider
//            collided = lp.x >= rp.x;
//        }

//        if (collided)
//        {
//            Debug.Log("[DIABLO/Walls] Colliders contact → holding");
//            phase = Phase.Holding;
//            timer = 0f;
//        }
//    }

//    // ---------- HOLDING (SE QUEDAN APRETANDO) ----------

//    private void UpdateHolding()
//    {
//        if (!leftInst || !rightInst)
//        {
//            StartGapAfterError();
//            return;
//        }

//        if (timer >= ctrl.A4_HoldTime)
//        {
//            DestroyWalls();
//            phase = Phase.Gap;
//            timer = 0f;
//        }
//    }

//    // ---------- GAP ENTRE OLAS ----------

//    private void UpdateGap()
//    {
//        if (timer >= ctrl.A4_WaveGap)
//        {
//            waveIndex++;

//            if (waveIndex < ctrl.A4_Waves)
//            {
//                StartWarning();
//            }
//            else
//            {
//                Debug.Log("[DIABLO/Walls] Ataque completo");
//                IsFinished = true;
//            }
//        }
//    }

//    private void StartGapAfterError()
//    {
//        // Si por algún motivo las paredes desaparecen antes,
//        // evitamos que se quede colgado el ataque.
//        DestroyWalls();
//        phase = Phase.Gap;
//        timer = 0f;
//    }

//    private void DestroyWalls()
//    {
//        if (leftInst) Object.Destroy(leftInst);
//        if (rightInst) Object.Destroy(rightInst);

//        leftInst = null;
//        rightInst = null;
//    }
//}


