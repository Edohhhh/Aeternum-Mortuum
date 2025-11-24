using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 5f;
    public float currentHealth = 5f;

    [Header("Regeneration")]
    public bool enableRegeneration = false;
    public float regenerationRate = 2f;
    public float regenDelay = 3f;
    private bool regenActive = false;

    [Header("Invulnerability")]
    // ⬇️ DURACIÓN DE LA INVULNERABILIDAD AL RECIBIR DAÑO
    public float invulnerableTime = 2f;
    private bool invulnerable = false;

    private Coroutine regenRoutine;

    [Header("UI")]
    public HealthUI healthUI;
    public HealCounterUI healCounterUI;

    private PlayerController playerController;

    [Header("Debug/Testing")]
    public float healAmount = 1f;

    [Header("Curas (ya NO se reinician por escena)")]
    public int healsLeft;

    // Cacheamos los colliders y el renderer para no buscarlos cada vez
    private Collider2D[] playerColliders;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerColliders = GetComponentsInChildren<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        if (healthUI != null)
            healthUI.Initialize(maxHealth);

        if (HealthDataNashe.Instance != null)
        {
            healsLeft = HealthDataNashe.Instance.healsLeft;
            UpdateHealCounterUI();
        }

        if (regenRoutine != null)
            StopCoroutine(regenRoutine);
        regenActive = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryUseHeal();
        }
    }

    private void TryUseHeal()
    {
        if (HealthDataNashe.Instance.healsLeft <= 0)
        {
            Debug.Log("[Heal] No quedan curas.");
            return;
        }

        if (currentHealth <= 0f)
        {
            Debug.Log("[Heal] Estás muerto, no podés curarte.");
            return;
        }

        ModifyHealthFlat(healAmount);

        HealthDataNashe.Instance.healsLeft--;
        healsLeft = HealthDataNashe.Instance.healsLeft;

        UpdateHealCounterUI();

        Debug.Log($"[Heal] Curación usada. Vida actual: {currentHealth}. Curas restantes: {healsLeft}");
    }

    public bool IsInvulnerable => invulnerable || (playerController != null && playerController.isInvulnerable);

    public void TakeDamage(float amount, Vector2 sourcePosition, float knockbackForce = 10f, float knockbackDuration = 0.2f)
    {
        if (IsInvulnerable ||
            (playerController != null &&
             playerController.stateMachine.CurrentState == playerController.KnockbackState))
            return;

        currentHealth -= amount;
        UpdateUI();

        if (currentHealth <= 0f)
        {
            Die();
            return;
        }

        StartCoroutine(InvulnerabilityRoutine());

        if (enableRegeneration)
            RestartRegenDelay();
    }

    public void ModifyHealthFlat(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateUI();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void ModifyHealthPercent(float percent)
    {
        float delta = maxHealth * percent;
        ModifyHealthFlat(delta);
    }

    // --- LÓGICA DE INVULNERABILIDAD MODIFICADA ---
    private IEnumerator InvulnerabilityRoutine()
    {
        invulnerable = true;

        // 1. Buscar a todos los enemigos activos en la escena por su Tag
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // Hacemos que el Player ignore las colisiones con estos enemigos
        foreach (GameObject enemy in enemies)
        {
            // Verificamos null por seguridad
            if (enemy != null)
            {
                // Obtenemos todos los colliders del enemigo (puede tener varios, box, circle, en hijos, etc.)
                Collider2D[] enemyColliders = enemy.GetComponentsInChildren<Collider2D>();

                foreach (Collider2D enemyCol in enemyColliders)
                {
                    foreach (Collider2D playerCol in playerColliders)
                    {
                        // 'true' significa IGNORAR colisión entre estos dos
                        Physics2D.IgnoreCollision(playerCol, enemyCol, true);
                    }
                }
            }
        }

        // 2. Parpadeo (Flicker) durante el tiempo de invulnerabilidad
        float elapsed = 0f;
        float flickerSpeed = 0.15f;

        while (elapsed < invulnerableTime)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = !spriteRenderer.enabled;

            yield return new WaitForSeconds(flickerSpeed);
            elapsed += flickerSpeed;
        }

        // 3. Restaurar estado normal
        if (spriteRenderer != null) spriteRenderer.enabled = true;

        // Reactivamos las colisiones con los enemigos que encontramos antes
        foreach (GameObject enemy in enemies)
        {
            // Es importante chequear si el enemigo sigue vivo (no es null)
            if (enemy != null)
            {
                Collider2D[] enemyColliders = enemy.GetComponentsInChildren<Collider2D>();

                foreach (Collider2D enemyCol in enemyColliders)
                {
                    foreach (Collider2D playerCol in playerColliders)
                    {
                        // 'false' significa DEJAR DE IGNORAR (volver a chocar)
                        Physics2D.IgnoreCollision(playerCol, enemyCol, false);
                    }
                }
            }
        }

        invulnerable = false;
    }

    private void RestartRegenDelay()
    {
        if (!enableRegeneration) return;

        if (regenRoutine != null)
            StopCoroutine(regenRoutine);

        regenRoutine = StartCoroutine(RegenRoutine());
    }

    private IEnumerator RegenRoutine()
    {
        yield return new WaitForSeconds(regenDelay);
        regenActive = true;
        while (regenActive && currentHealth < maxHealth)
        {
            ModifyHealthFlat(regenerationRate * Time.deltaTime);
            yield return null;
        }
        regenActive = false;
    }

    public void UpdateUI()
    {
        if (healthUI != null)
            healthUI.UpdateHearts(currentHealth);
    }

    public void UpdateHealCounterUI()
    {
        if (healCounterUI != null && HealthDataNashe.Instance != null)
            healCounterUI.SetHealsRemaining(
                HealthDataNashe.Instance.healsLeft,
                HealthDataNashe.Instance.maxHeals
            );
    }

    private void Die()
    {
        Debug.Log("Player Died");
        SceneManager.LoadScene("Lose");
    }
}