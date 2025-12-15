using UnityEngine;
using UnityEngine.SceneManagement;

public class RunResultButton : MonoBehaviour
{
    public enum ResultType
    {
        Win,
        Death
    }

    [Header("Result Type")]
    public ResultType resultType;


    public void OnButtonClicked()
    {
        if (RunStatsManager.Instance == null)
        {
            Debug.LogWarning("RunStatsManager not found");
            return;
        }

        switch (resultType)
        {
            case ResultType.Win:
                RunStatsManager.Instance.AddWin();
                break;

            case ResultType.Death:
                RunStatsManager.Instance.AddDeath();
                break;
        }
    }
}
