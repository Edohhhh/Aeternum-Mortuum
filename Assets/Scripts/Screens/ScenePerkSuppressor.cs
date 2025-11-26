using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class ScenePerkSuppressor : MonoBehaviour
{
    [Header("Perks a desactivar SOLO en esta escena")]
    [Tooltip("Arrastrá aquí los ScriptableObject de las perks que NO querés que funcionen en esta escena.")]
    public List<PowerUp> perksToSuppress = new List<PowerUp>();

    private List<PowerUp> originalPlayerPerks;
    private List<PowerUp> originalDataPerks;

    private bool applied = false;

    private IEnumerator Start()
    {
        PlayerController player = null;
        while (player == null)
        {
            player = Object.FindFirstObjectByType<PlayerController>();
            yield return null;
        }

        yield return null;

        if (player.initialPowerUps != null)
            originalPlayerPerks = new List<PowerUp>(player.initialPowerUps);
        else
            originalPlayerPerks = new List<PowerUp>();


        var gdm = GameDataManager.Instance;
        if (gdm != null && gdm.playerData != null && gdm.playerData.initialPowerUps != null)
            originalDataPerks = new List<PowerUp>(gdm.playerData.initialPowerUps);
        else
            originalDataPerks = null;

        var filtradasPlayer = new List<PowerUp>();

        foreach (var perk in originalPlayerPerks)
        {
            if (perk == null) continue;

            if (perksToSuppress.Contains(perk))
            {
                try
                {
                    perk.Remove(player);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[ScenePerkSuppressor] Error al remover perk {perk.name}: {e.Message}");
                }

                continue;
            }

            filtradasPlayer.Add(perk);
        }


        player.initialPowerUps = filtradasPlayer.ToArray();


        if (gdm != null && gdm.playerData != null)
        {
            gdm.playerData.initialPowerUps = new List<PowerUp>(filtradasPlayer);
        }

        applied = true;

        Debug.Log($"[ScenePerkSuppressor] Perks activas en esta escena: {filtradasPlayer.Count} (suprimidas: {originalPlayerPerks.Count - filtradasPlayer.Count})");
    }

    private void OnDestroy()
    {
        if (!applied) return;

        var gdm = GameDataManager.Instance;

        if (gdm != null && gdm.playerData != null && originalDataPerks != null)
        {
            gdm.playerData.initialPowerUps = new List<PowerUp>(originalDataPerks);
            Debug.Log("[ScenePerkSuppressor] Restauradas perks originales en GameDataManager al salir de la escena.");
        }

        var player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null && originalPlayerPerks != null)
        {
            player.initialPowerUps = originalPlayerPerks.ToArray();
            Debug.Log("[ScenePerkSuppressor] Restauradas perks originales en PlayerController al salir de la escena.");
        }
    }
}
