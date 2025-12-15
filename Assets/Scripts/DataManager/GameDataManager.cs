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
            player.LoadPlayerData();

            // Restaurar vida completa al cambiar de escena
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
        }

        // ✅ --- AÑADIDO (Corrección de Orden de Ejecución) ---
        // Ahora que el jugador está 100% cargado (con stats y power-ups),
        // buscamos el WheelSelector y le decimos que inicie.
        var selector = FindObjectOfType<WheelSelector>(true); // true = buscar inactivos
        if (selector != null)
        {
            selector.IniciarSelector(); // Llamamos al nuevo método
        }
        // ✅ --- FIN ---
    }

    public void SavePlayerData(PlayerController player)
    {
        // Movimiento / Dash
        playerData.moveSpeed = player.moveSpeed;
        playerData.dashSpeed = player.dashSpeed;
        playerData.dashIframes = player.dashIframes;
        playerData.dashSlideDuration = player.dashSlideDuration;
        playerData.dashDuration = player.dashDuration;
        playerData.dashCooldown = player.dashCooldown;

        playerData.baseDamage = player.baseDamage;

        // ✅ --- AÑADIDO ---
        playerData.extraSpins = player.extraSpins;

        // Salud
        var health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            playerData.maxHealth = health.maxHealth;
            playerData.regenerationRate = health.regenerationRate;
            playerData.regenDelay = health.regenDelay;
            playerData.invulnerableTime = health.invulnerableTime;
        }

        // Posición
        playerData.position = player.transform.position;

        // Guardar PowerUps activos
        playerData.initialPowerUps.Clear();
        foreach (var powerUp in player.initialPowerUps)
        {
            if (powerUp != null)
                playerData.initialPowerUps.Add(powerUp);
        }
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
            moveSpeed = 4,
            dashSpeed = 10,
            dashIframes = 10,
            dashSlideDuration = 0.1f,
            dashDuration = 0.15f,
            dashCooldown = 0.75f,
            baseDamage = 1,
            // ✅ --- AÑADIDO ---
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

    //  Reset TOTAL con Player
    public void ResetPlayerCompletely(PlayerController player)
    {
        if (player == null) return;

        // Limpiar perks y efectos
        ClearAllPerks(player);

        // Resetear stats base
        playerData = new PlayerData
        {
            moveSpeed = 4,
            dashSpeed = 10,
            dashIframes = 10,
            dashSlideDuration = 0.1f,
            dashDuration = 0.15f,
            dashCooldown = 0.75f,
            baseDamage = 1,
            // ✅ --- AÑADIDO ---
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

    //  Reset TOTAL sin Player (ejemplo: Win/Lose)
    public void ResetAllWithoutPlayer()
    {
        // Resetear stats base
        playerData = new PlayerData
        {
            moveSpeed = 4,
            dashSpeed = 10,
            dashIframes = 10,
            dashSlideDuration = 0.1f,
            dashDuration = 0.15f,
            dashCooldown = 0.75f,
            baseDamage = 1,
            // ✅ --- AÑADIDO ---
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