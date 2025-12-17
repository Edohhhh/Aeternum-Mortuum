using UnityEngine;
using UnityEngine.SceneManagement;

public class LaRosaManager : MonoBehaviour
{
    public int reflectDamage = 1;

    private bool active;

    private PlayerHealth ph;
    private LaRosaHitTracker hitTracker;

    private float lastHealth;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void SetEnabled(bool enabled)
    {
        active = enabled;

        // si la desactivás, soltamos referencias
        if (!active)
        {
            ph = null;
            hitTracker = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!active) return;
        RebindNow();
    }

    public void RebindNow()
    {
        var player = Object.FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        ph = player.GetComponent<PlayerHealth>();
        if (ph == null) return;

        // Asegurar tracker de “quién fue el último enemigo que tocó al player”
        hitTracker = player.GetComponent<LaRosaHitTracker>();
        if (hitTracker == null)
            hitTracker = player.gameObject.AddComponent<LaRosaHitTracker>();

        hitTracker.enabled = true;

        // IMPORTANTÍSIMO: baseline para que NO dispare al cargar escena
        lastHealth = ph.currentHealth;
    }

    private void Update()
    {
        if (!active) return;
        if (ph == null || hitTracker == null)
        {
            // Si por algún motivo no está bindeado todavía
            RebindNow();
            return;
        }

        float current = ph.currentHealth;

        // Si la vida bajó => daño real
        if (current < lastHealth)
        {
            var enemy = hitTracker.LastEnemy;
            if (enemy != null)
            {
                enemy.TakeDamage(reflectDamage, Vector2.zero, 0f);
            }
        }

        lastHealth = current;
    }
}
