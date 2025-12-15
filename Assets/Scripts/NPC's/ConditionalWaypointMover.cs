using UnityEngine;

public class ConditionalWaypointMover : MonoBehaviour
{
    public enum RunCondition
    {
        Win,
        Death
    }

    [Header("Condición")]
    [SerializeField] private RunCondition condition;
    [SerializeField] private int requiredAmount = 1;

    [Header("Objetos a mover")]
    [SerializeField] private GameObject[] objectsToMove;

    [Header("Waypoint destino")]
    [SerializeField] private Transform targetWaypoint;

    [Header("Opciones")]
    [SerializeField] private bool checkOnStart = true;

    private void Start()
    {
        if (!checkOnStart) return;

        TryMoveObjects();
    }

    public void TryMoveObjects()
    {
        if (RunStatsManager.Instance == null)
        {
            Debug.LogWarning("RunStatsManager no encontrado.");
            return;
        }

        if (!ConditionMet()) return;

        MoveObjects();
    }

    private bool ConditionMet()
    {
        switch (condition)
        {
            case RunCondition.Win:
                return RunStatsManager.Instance.wins >= requiredAmount;

            case RunCondition.Death:
                return RunStatsManager.Instance.deaths >= requiredAmount;
        }

        return false;
    }

    private void MoveObjects()
    {
        if (targetWaypoint == null)
        {
            Debug.LogWarning("Waypoint no asignado.");
            return;
        }

        foreach (GameObject obj in objectsToMove)
        {
            if (obj == null) continue;

            obj.transform.position = targetWaypoint.position;
        }

        Debug.Log("Objetos movidos por condición: " + condition);
    }
}
