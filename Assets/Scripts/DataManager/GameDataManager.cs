using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }
    public PlayerData playerData = new PlayerData();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            var initialPlayer = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            if (initialPlayer != null)
            {
                SavePlayerData(initialPlayer);
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(WaitAndLoadPlayer());
    }

    private System.Collections.IEnumerator WaitAndLoadPlayer()
    {
        yield return null; // esperar un frame

        var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            // ✅ Cargar stats + lista de perks desde PlayerData
            ApplyPlayerDataToPlayer(player);

            // ✅ Aplicar perks cargadas (las one-shot se auto-borran)
            ApplyInitialPowerUps(player);

            // Restaurar vida completa al cambiar de escena (como lo tenías)
            var health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.currentHealth = health.maxHealth;

                if (health.healthUI != null)
                {
                    health.healthUI.Initialize(health.maxHealth);
                    health.healthUI.UpdateHearts(health.currentHealth);
                }
            }

            // ✅ Guardar por si alguna perk se auto-removió / cambió stats al cargar
            SavePlayerData(player);
        }

        // ✅ WheelSelector (como lo tenías)
        var selector = FindObjectOfType<WheelSelector>(true);
        if (selector != null)
        {
            selector.IniciarSelector();
        }
    }

    private void ApplyPlayerDataToPlayer(PlayerController player)
    {
        if (player == null) return;

        // Movement / Dash
        player.moveSpeed = playerData.moveSpeed;
        player.dashSpeed = playerData.dashSpeed;
        player.dashIframes = playerData.dashIframes;
        player.dashSlideDuration = playerData.dashSlideDuration;
        player.dashDuration = playerData.dashDuration;
        player.dashCooldown = playerData.dashCooldown;

        // Attack
        player.baseDamage = playerData.baseDamage;

        // Extra spins
        player.extraSpins = playerData.extraSpins;

        // Health
        var health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.maxHealth = playerData.maxHealth;
            health.regenerationRate = playerData.regenerationRate;
            health.regenDelay = playerData.regenDelay;
            health.invulnerableTime = playerData.invulnerableTime;

            // Si querés restaurar vida exacta guardada, descomentá:
            // health.currentHealth = Mathf.Min(playerData.currentHealth, health.maxHealth);

            if (health.healthUI != null)
            {
                health.healthUI.Initialize(health.maxHealth);
                health.healthUI.UpdateHearts(health.currentHealth);
            }
        }

        // PowerUps activos
        if (playerData.initialPowerUps != null)
            player.initialPowerUps = playerData.initialPowerUps.ToArray();
        else
            player.initialPowerUps = new PowerUp[0];

        // Posición
        player.transform.position = playerData.position;
    }

    private void ApplyInitialPowerUps(PlayerController player)
    {
        if (player == null) return;
        if (player.initialPowerUps == null || player.initialPowerUps.Length == 0) return;

        // Copia para poder iterar aunque algunas perks se auto-remuevan
        var copy = new List<PowerUp>(player.initialPowerUps);

        foreach (var perk in copy)
        {
            if (perk == null) continue;
            perk.Apply(player);
        }
    }

    public void SavePlayerData(PlayerController player)
    {
        if (player == null) return;

        // ✅ 1) Si el mendigo está aplicado, lo quitamos SOLO para guardar “base stats”
        var tracker = player.GetComponent<StatAugmentTracker>();
        bool hadBeggarBuff = (tracker != null);

        if (hadBeggarBuff)
            tracker.RemoveFrom(player);

        // ---- Guardado normal (igual que tuyo) ----

        // Movimiento / Dash
        playerData.moveSpeed = player.moveSpeed;
        playerData.dashSpeed = player.dashSpeed;
        playerData.dashIframes = player.dashIframes;
        playerData.dashSlideDuration = player.dashSlideDuration;
        playerData.dashDuration = player.dashDuration;
        playerData.dashCooldown = player.dashCooldown;

        playerData.baseDamage = player.baseDamage;

        // Extra spins
        playerData.extraSpins = player.extraSpins;

        // Salud
        var health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            playerData.maxHealth = health.maxHealth;
            playerData.currentHealth = health.currentHealth;
            playerData.regenerationRate = health.regenerationRate;
            playerData.regenDelay = health.regenDelay;
            playerData.invulnerableTime = health.invulnerableTime;
        }

        // Posición
       // playerData.position = player.transform.position;

        // Guardar PowerUps activos
        playerData.initialPowerUps.Clear();
        foreach (var powerUp in player.initialPowerUps)
        {
            if (powerUp != null)
                playerData.initialPowerUps.Add(powerUp);
        }

        // ✅ 2) Re-aplicamos el mendigo para que el gameplay no quede “debuffeado”
        if (hadBeggarBuff)
            BeggarValueObserver.RequestReapply();
    }

    // Remover perk individual
    public void RemovePerk(PlayerController player, PowerUp perk)
    {
        if (perk != null)
        {
            perk.Remove(player); // limpia efectos
            playerData.initialPowerUps.Remove(perk);
        }
    }

    // Remover todas las perks
    public void ClearAllPerks(PlayerController player)
    {
        if (player == null) return;

        foreach (var perk in player.initialPowerUps)
        {
            if (perk != null)
                perk.Remove(player);
        }

        player.initialPowerUps = new PowerUp[0];
        playerData.initialPowerUps.Clear();
    }

    // reset parcial de stats
    public void ResetPlayerData(PlayerController player)
    {
        ClearAllPerks(player);

        playerData = new PlayerData
        {
            moveSpeed = 5,
            dashSpeed = 10,
            dashIframes = 10,
            dashSlideDuration = 0.1f,
            dashDuration = 0.15f,
            dashCooldown = 0.75f,
            baseDamage = 3,
            extraSpins = 0,
            maxHealth = 4,
            currentHealth = 4,
            regenerationRate = 2,
            regenDelay = 3,
            invulnerableTime = 1,
            position = Vector2.zero,
            initialPowerUps = new List<PowerUp>()
        };

        Debug.Log("[GameDataManager] Datos reseteados (stats base, perks vacías).");
    }

    // Reset TOTAL con Player
    public void ResetPlayerCompletely(PlayerController player)
    {
        if (player == null) return;

        ClearAllPerks(player);

        playerData = new PlayerData
        {
            moveSpeed = 5,
            dashSpeed = 10,
            dashIframes = 10,
            dashSlideDuration = 0.1f,
            dashDuration = 0.15f,
            dashCooldown = 0.75f,
            baseDamage = 3,
            extraSpins = 0,
            maxHealth = 4,
            currentHealth = 4,
            regenerationRate = 2,
            regenDelay = 3,
            invulnerableTime = 1,
            position = Vector2.zero,
            initialPowerUps = new List<PowerUp>()
        };

        Debug.Log("[GameDataManager] 🚨 Reset TOTAL con Player.");
    }

    // Reset TOTAL sin Player (ejemplo: Win/Lose)
    public void ResetAllWithoutPlayer()
    {
        playerData = new PlayerData
        {
            moveSpeed = 5,
            dashSpeed = 10,
            dashIframes = 10,
            dashSlideDuration = 0.1f,
            dashDuration = 0.15f,
            dashCooldown = 0.75f,
            baseDamage = 3,
            extraSpins = 0,
            maxHealth = 4,
            currentHealth = 4,
            regenerationRate = 2,
            regenDelay = 3,
            invulnerableTime = 1,
            position = Vector2.zero,
            initialPowerUps = new List<PowerUp>()
        };

        // Borrar cualquier objeto colgado en DontDestroyOnLoad
        var ddolScene = gameObject.scene;

        foreach (var go in ddolScene.GetRootGameObjects())
        {
            if (go == this.gameObject)
                continue;

            // 🛑 NO destruir objetos marcados como no reseteables
            if (go.GetComponent<INonResettable>() != null)
                continue;

            Destroy(go);
        }

        Debug.Log("[GameDataManager] Reset TOTAL aplicado SIN Player en escena.");
    }
}
