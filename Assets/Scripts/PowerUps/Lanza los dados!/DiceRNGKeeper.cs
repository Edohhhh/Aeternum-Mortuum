using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Guarda los deltas y los reaplica en cada escena SIN stackear,
/// tomando como baseline el valor del player después de su LoadPlayerData.
/// </summary>
public class DiceRNGKeeper : MonoBehaviour
{
    public int deltaDamage;
    public int deltaSpeed;    // se suma a moveSpeed (float)
    public int deltaHealth;   // se suma a maxHealth (float, pero usamos int tirado)

    private int lastAppliedSceneIndex = int.MinValue;

    // ✅ Singleton para evitar múltiples keepers
    private static DiceRNGKeeper instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded -= OnSceneLoaded; // extra safety
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>Llamar tras tirar los dados en la escena actual.</summary>
    public void RecordDeltasForThisScene(int dDmg, int dSpeed, int dHP)
    {
        deltaDamage = dDmg;
        deltaSpeed = dSpeed;
        deltaHealth = dHP;
        lastAppliedSceneIndex = SceneManager.GetActiveScene().buildIndex;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si ya aplicamos en esta escena (por ejemplo la actual donde se tiraron los dados), no repetir
        if (scene.buildIndex == lastAppliedSceneIndex) return;

        ApplyOnceInThisScene();
        lastAppliedSceneIndex = scene.buildIndex;
    }

    private void ApplyOnceInThisScene()
    {
        var player = Object.FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        // === Baselines post-LoadPlayerData ===
        int baselineDamage = player.baseDamage;
        float baselineSpeed = player.moveSpeed;

        // DAÑO (mínimo 1)
        player.baseDamage = Mathf.Max(1, baselineDamage + deltaDamage);

        // VELOCIDAD (mínimo 1)
        player.moveSpeed = Mathf.Max(1f, baselineSpeed + deltaSpeed);

        // VIDA (ajusta max + current, no persiste en PlayerData)
        var ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            float baselineMax = ph.maxHealth;
            float newMax = Mathf.Max(1f, baselineMax + deltaHealth);
            float deltaApplied = newMax - ph.maxHealth; // por si otro sistema tocó antes

            ph.maxHealth = newMax;

            if (deltaApplied > 0)
                ph.currentHealth = Mathf.Min(ph.currentHealth + deltaApplied, ph.maxHealth);
            else
                ph.currentHealth = Mathf.Min(ph.currentHealth, ph.maxHealth);

            if (ph.healthUI != null)
            {
                ph.healthUI.Initialize(ph.maxHealth);
                ph.healthUI.UpdateHearts(ph.currentHealth);
            }
        }
    }
}