using UnityEngine;

public class RunStatsManager : MonoBehaviour, INonResettable
{
    public static RunStatsManager Instance;

    [Header("Stats")]
    public int wins = 0;
    public int deaths = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddWin()
    {
        wins++;
        Debug.Log("Wins: " + wins);
    }

    public void AddDeath()
    {
        deaths++;
        Debug.Log("Deaths: " + deaths);
    }
}
