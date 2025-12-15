using UnityEngine;

[System.Serializable]
public class DialogueCondition
{
    [Header("Condiciones mínimas")]
    public int minDeaths;
    public int minWins;

    [Header("Diálogo a usar")]
    public NPCDialogue dialogueOverride;
}

