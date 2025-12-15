using UnityEngine;

public class AutoHealsSecondRun : MonoBehaviour
{
    [Header("Config")]
    public int healsToGive = 3;

    private void Start()
    {
        if (RunStatsManager.Instance == null)
        {
            Debug.LogWarning("RunStatsManager no encontrado");
            return;
        }

        if (HealthDataNashe.Instance == null)
        {
            Debug.LogWarning("HealthDataNashe no encontrado");
            return;
        }

        // Condición REAL: murió o ganó al menos una vez
        if (RunStatsManager.Instance.wins >= 1 ||
            RunStatsManager.Instance.deaths >= 1)
        {
            HealthDataNashe.Instance.SetMaxHeals(healsToGive);
            HealthDataNashe.Instance.AddHeal(healsToGive);

            Debug.Log("AutoHealsSecondRun: curas asignadas por segunda run");
        }
    }
}
