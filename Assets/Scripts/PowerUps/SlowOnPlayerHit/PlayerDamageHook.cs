using UnityEngine;

public class PlayerDamageHook : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private float lastHealth;

    public System.Action OnPlayerDamaged;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
            lastHealth = playerHealth.currentHealth;
    }

    private void Update()
    {
        if (playerHealth == null) return;

        if (playerHealth.currentHealth < lastHealth)
        {
            lastHealth = playerHealth.currentHealth;
            OnPlayerDamaged?.Invoke();
        }
        else if (playerHealth.currentHealth > lastHealth)
        {
            lastHealth = playerHealth.currentHealth;
        }
    }

    public static void Attach(PlayerController player, PlayerHurtSlowObserver observer)
    {
        if (player == null || observer == null) return;

        var hook = player.GetComponent<PlayerDamageHook>();
        if (hook == null)
            hook = player.gameObject.AddComponent<PlayerDamageHook>();

        hook.OnPlayerDamaged -= observer.OnPlayerDamaged;
        hook.OnPlayerDamaged += observer.OnPlayerDamaged;
    }
}
