using UnityEngine;

public class ResetGameOnKey : MonoBehaviour
{
    [Header("Tecla para resetear")]
    public KeyCode resetKey = KeyCode.F;

    void Update()
    {
        if (Input.GetKeyDown(resetKey))
        {
            ResetGame();
        }
    }

    private void ResetGame()
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

        Debug.Log("[ResetGameOnKey] Reset ejecutado con tecla " + resetKey);
    }
}
