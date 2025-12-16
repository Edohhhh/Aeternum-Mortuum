using UnityEngine;

[DisallowMultipleComponent]
public class StatAugmentTracker : MonoBehaviour
{
    private int appliedDamage;
    private float appliedMoveSpeed;
    private float appliedMaxHealth;
    private float appliedMultiplier = 1f;

    // ✅ NUEVO: spins
    private int appliedExtraSpins;

    public void ApplyTo(PlayerController player,
        int addDamage,
        float addMoveSpeed,
        float addMaxHealth,
        float moveSpeedMultiplier,
        int addExtraSpins)
    {
        // Aplicar daño
        player.baseDamage = (int)(player.baseDamage + addDamage);
        appliedDamage = addDamage;

        // Aplicar velocidad (sumatoria y multiplicador)
        player.moveSpeed = (player.moveSpeed + addMoveSpeed) * moveSpeedMultiplier;
        appliedMoveSpeed = addMoveSpeed;
        appliedMultiplier = moveSpeedMultiplier;

        // ✅ Aplicar spins extra
        player.extraSpins += addExtraSpins;
        appliedExtraSpins = addExtraSpins;

        // Aplicar vida máxima
        var ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.maxHealth += addMaxHealth;
            ph.currentHealth = Mathf.Min(ph.currentHealth + addMaxHealth, ph.maxHealth);
            appliedMaxHealth = addMaxHealth;

            if (ph.healthUI != null)
            {
                ph.healthUI.Initialize(ph.maxHealth);
                ph.healthUI.UpdateHearts(ph.currentHealth);
            }
        }
    }

    public void RemoveFrom(PlayerController player)
    {
        // Revertir daño
        player.baseDamage = (int)(player.baseDamage - appliedDamage);

        // Revertir velocidad
        player.moveSpeed = (player.moveSpeed - appliedMoveSpeed) / appliedMultiplier;

        // ✅ Revertir spins extra
        player.extraSpins -= appliedExtraSpins;

        // Revertir vida máxima
        var ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.maxHealth -= appliedMaxHealth;
            ph.currentHealth = Mathf.Min(ph.currentHealth, ph.maxHealth);

            if (ph.healthUI != null)
            {
                ph.healthUI.Initialize(ph.maxHealth);
                ph.healthUI.UpdateHearts(ph.currentHealth);
            }
        }

        appliedDamage = 0;
        appliedMoveSpeed = 0f;
        appliedMaxHealth = 0f;
        appliedMultiplier = 1f;

        // ✅ reset spins
        appliedExtraSpins = 0;
    }
}