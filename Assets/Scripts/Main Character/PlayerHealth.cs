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
    public float invulnerableTime = 2f;
    private bool invulnerable = false;

    private Coroutine regenRoutine;

    [Header("UI")]
    public HealthUI healthUI;
    public HealCounterUI healCounterUI;

    private PlayerController playerController;

    [Header("Debug / Testing")]
    public float healAmount = 1f;

    [Header("Curas (ya NO se reinician por escena)")]
    public int healsLeft;

    // Cache
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
            TryUseHeal();
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

    public bool IsInvulnerable =>
        invulnerable || (playerController != null && playerController.isInvulnerable);

   
    public void TakeDamage(
        float amount,
        Vector2 sourcePosition,
        float knockbackForce = 10f,
        float knockbackDuration = 0.2f)
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
            Die();
    }

    public void ModifyHealthPercent(float percent)
    {
        ModifyHealthFlat(maxHealth * percent);
    }

  
    private IEnumerator InvulnerabilityRoutine()
    {
        invulnerable = true;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            Collider2D[] enemyColliders = enemy.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D enemyCol in enemyColliders)
                foreach (Collider2D playerCol in playerColliders)
                    Physics2D.IgnoreCollision(playerCol, enemyCol, true);
        }

        float elapsed = 0f;
        float flickerSpeed = 0.15f;

        while (elapsed < invulnerableTime)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = !spriteRenderer.enabled;

            yield return new WaitForSeconds(flickerSpeed);
            elapsed += flickerSpeed;
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            Collider2D[] enemyColliders = enemy.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D enemyCol in enemyColliders)
                foreach (Collider2D playerCol in playerColliders)
                    Physics2D.IgnoreCollision(playerCol, enemyCol, false);
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
