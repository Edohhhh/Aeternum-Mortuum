using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ResetGameButton : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonPressed);
    }

    public void OnButtonPressed()
    {
        var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();

        if (player != null)
        {
            // 🔹 Gameplay: reset completo del jugador
            GameDataManager.Instance.ResetPlayerCompletely(player);
        }
        else
        {
            // 🔹 Win / Lose: reset general
            GameDataManager.Instance.ResetAllWithoutPlayer();
        }

        // 🛑 IMPORTANTE: NO tocar RunStatsManager
        PreserveRunStats();

        Debug.Log("[ResetGameButton] Reset ejecutado (RunStats preservado).");
    }

    private void PreserveRunStats()
    {
        if (RunStatsManager.Instance == null)
            return;

        // No hacemos nada a propósito
        // Este método existe solo para dejar explícito
        // que RunStatsManager NO debe resetearse
    }
}
