using UnityEngine;
using UnityEngine.UI;

public class DamageVignetteController : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;

    [Header("UI Images")]
    public Image damageVignette;   // Imagen roja
    public Image healVignette;     // Imagen verde

    [Header("Audio")]
    public AudioSource[] damageAudios; // 🔊 Lista de sonidos de daño (random)
    public AudioSource healAudio;      // Sonido de curación

    [Header("Damage Flash")]
    public float damageFlashAlpha = 0.65f;
    public float damageFlashDuration = 0.25f;
    public float damageFadeSpeed = 5f;
    public float lowHPAlpha = 0.45f;
    public float lowHPThreshold = 1f;

    [Header("Heal Flash")]
    public float healFlashAlpha = 0.45f;
    public float healFlashDuration = 0.25f;
    public float healFadeSpeed = 5f;

    private float damageTargetAlpha = 0f;
    private float healTargetAlpha = 0f;

    private float lastHealth;

    private void Start()
    {
        if (playerHealth != null)
            lastHealth = playerHealth.currentHealth;
    }

    private void Update()
    {
        if (playerHealth == null)
            return;

        float current = playerHealth.currentHealth;

        // --- Detectar daño ---
        if (current < lastHealth)
        {
            TriggerDamageFlash();
        }

        // --- Detectar curación ---
        if (current > lastHealth)
        {
            TriggerHealFlash();
        }

        // --- Low HP constante ---
        if (current <= lowHPThreshold && current > 0)
        {
            damageTargetAlpha = lowHPAlpha;
        }
        else
        {
            if (damageTargetAlpha != damageFlashAlpha)
                damageTargetAlpha = 0f;
        }

        // --- Fade de daño ---
        if (damageVignette != null)
        {
            Color c = damageVignette.color;
            c.a = Mathf.Lerp(c.a, damageTargetAlpha, Time.deltaTime * damageFadeSpeed);
            damageVignette.color = c;
        }

        // --- Fade de cura ---
        if (healVignette != null)
        {
            Color c = healVignette.color;
            c.a = Mathf.Lerp(c.a, healTargetAlpha, Time.deltaTime * healFadeSpeed);
            healVignette.color = c;
        }

        lastHealth = current;
    }

    // ---------------- DAMAGE FLASH ----------------
    public void TriggerDamageFlash()
    {
        StopCoroutine(nameof(DamageFlashCoroutine));
        StartCoroutine(DamageFlashCoroutine());

        PlayRandomDamageSound();
    }

    private void PlayRandomDamageSound()
    {
        if (damageAudios == null || damageAudios.Length == 0)
            return;

        int index = Random.Range(0, damageAudios.Length);
        if (damageAudios[index] != null)
            damageAudios[index].Play();
    }

    private System.Collections.IEnumerator DamageFlashCoroutine()
    {
        damageTargetAlpha = damageFlashAlpha;

        yield return new WaitForSeconds(damageFlashDuration);

        if (playerHealth.currentHealth <= lowHPThreshold)
            damageTargetAlpha = lowHPAlpha;
        else
            damageTargetAlpha = 0f;
    }

    // ---------------- HEAL FLASH ----------------
    public void TriggerHealFlash()
    {
        StopCoroutine(nameof(HealFlashCoroutine));
        StartCoroutine(HealFlashCoroutine());

        if (healAudio != null)
            healAudio.Play();
    }

    private System.Collections.IEnumerator HealFlashCoroutine()
    {
        healTargetAlpha = healFlashAlpha;

        yield return new WaitForSeconds(healFlashDuration);

        healTargetAlpha = 0f;
    }
}
