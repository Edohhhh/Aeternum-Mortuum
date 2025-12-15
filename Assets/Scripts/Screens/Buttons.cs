using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    public enum RunResult
    {
        None,
        Win,
        Death
    }

    [Header("Run Result (solo en win/lose screens)")]
    public RunResult runResult = RunResult.None;

    public void PlayAgain()
    {
        RegisterRunResult();

        if (RoomRandomizer.Instance == null)
        {
            Debug.LogError("[MenuButtons] No se encontró RoomRandomizer.");
            return;
        }

        RoomRandomizer.Instance.GenerateRun();

        string firstScene = RoomRandomizer.Instance.GetNextRoom();

        if (!string.IsNullOrEmpty(firstScene))
        {
            SceneManager.LoadScene(firstScene);
        }
        else
        {
            Debug.LogError("[MenuButtons] No hay salas en la run.");
        }
    }

    public void Menu()
    {
        RegisterRunResult();
        SceneManager.LoadScene("Menu");
    }

    private void RegisterRunResult()
    {
        if (runResult == RunResult.None)
            return;

        if (RunStatsManager.Instance == null)
        {
            Debug.LogWarning("RunStatsManager not found");
            return;
        }

        switch (runResult)
        {
            case RunResult.Win:
                RunStatsManager.Instance.AddWin();
                break;

            case RunResult.Death:
                RunStatsManager.Instance.AddDeath();
                break;
        }
    }
}
