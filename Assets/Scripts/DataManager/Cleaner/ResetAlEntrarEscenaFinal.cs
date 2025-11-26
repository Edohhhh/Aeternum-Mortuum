using UnityEngine;

public class ResetAlEntrarEscenaFinal : MonoBehaviour
{
    void Start()
    {
        var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();

        if (player != null)
        {
            GameDataManager.Instance.ResetPlayerCompletely(player);
        }
        else
        {
            GameDataManager.Instance.ResetAllWithoutPlayer();
        }

        Debug.Log("[ResetAlEntrarEscenaFinal] Reset ejecutado al abrir la escena final.");
    }
}
