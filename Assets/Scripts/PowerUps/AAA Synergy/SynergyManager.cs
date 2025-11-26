using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema de sinergias independiente (versión "cirugía fina"):
/// - NO modifica GameDataManager ni PlayerController.
/// - Sólo remueve y limpia las perks que son ingredientes de sinergias.
/// - No borra ni reaplica el resto de las perks.
/// </summary>
public class SynergyManager : MonoBehaviour
{
    [System.Serializable]
    public class SynergyEntry
    {
        [Header("Identificación")]
        public string synergyName;

        [Header("Perks que deben estar presentes (ingredientes)")]
        public List<PowerUp> ingredients = new List<PowerUp>();

        [Header("Perk que se obtiene al armar la sinergia")]
        public PowerUp resultPerk;
    }

    [Header("Configuración general")]
    [Tooltip("Si está activado, busca al Player al iniciar la escena y chequea sinergias automáticamente.")]
    public bool autoEvaluateOnStart = true;

    [Tooltip("Lista de sinergias que se pueden formar con las perks del jugador.")]
    public List<SynergyEntry> synergies = new List<SynergyEntry>();

    private PlayerController cachedPlayer;

    private void Start()
    {
        if (autoEvaluateOnStart)
        {
            StartCoroutine(DelayedAutoEvaluate());
        }
    }

    private IEnumerator DelayedAutoEvaluate()
    {
        // Esperar 1 frame para que GameDataManager y PlayerController
        // terminen de cargar datos
        yield return null;
        EvaluateAllSynergies();
    }

    /// <summary>
    /// Llamá esto cuando quieras (por ejemplo después de girar la ruleta).
    /// </summary>
    [ContextMenu("Evaluar sinergias ahora")]
    public void EvaluateAllSynergies()
    {
        if (cachedPlayer == null)
            cachedPlayer = FindFirstObjectByType<PlayerController>();

        if (cachedPlayer == null)
        {
            Debug.LogWarning("[SynergyManager] No se encontró PlayerController en la escena.");
            return;
        }

        if (GameDataManager.Instance == null)
        {
            Debug.LogWarning("[SynergyManager] No se encontró GameDataManager.Instance.");
            return;
        }

        var data = GameDataManager.Instance.playerData;
        if (data == null || data.initialPowerUps == null)
        {
            Debug.LogWarning("[SynergyManager] playerData o initialPowerUps es null.");
            return;
        }

        // 1) Construimos una lista local de las perks actuales del jugador
        List<PowerUp> currentPerks = new List<PowerUp>();

        // Del Player
        if (cachedPlayer.initialPowerUps != null)
        {
            foreach (var p in cachedPlayer.initialPowerUps)
                if (p != null && !currentPerks.Contains(p))
                    currentPerks.Add(p);
        }

        // Del GameData (por seguridad, en caso de diferencias)
        if (data.initialPowerUps != null)
        {
            foreach (var p in data.initialPowerUps)
                if (p != null && !currentPerks.Contains(p))
                    currentPerks.Add(p);
        }

        bool anySynergyFormed = false;

        // 2) Procesar sinergias una por una
        foreach (var synergy in synergies)
        {
            if (synergy == null) continue;
            if (synergy.resultPerk == null) continue;
            if (synergy.ingredients == null || synergy.ingredients.Count == 0) continue;

            // ¿Tiene todas las perks ingrediente?
            if (!PlayerHasAllIngredients(currentPerks, synergy.ingredients))
                continue;

            Debug.Log($"[SynergyManager] Formando sinergia: {synergy.synergyName}");

            // 2.a) Remover ingredientes: efectos + de listas
            foreach (var ing in synergy.ingredients)
            {
                if (ing == null) continue;

                // Limpia efectos y quita de GameData.playerData.initialPowerUps
                GameDataManager.Instance.RemovePerk(cachedPlayer, ing);

                // Quita también de la lista local
                if (currentPerks.Contains(ing))
                    currentPerks.Remove(ing);
            }

            // 2.b) Agregar perk resultado si no está
            if (!currentPerks.Contains(synergy.resultPerk))
            {
                currentPerks.Add(synergy.resultPerk);
                synergy.resultPerk.Apply(cachedPlayer); // aplicar sólo la nueva perk
            }

            anySynergyFormed = true;
        }

        // 3) Sincronizar listas finales (Player + GameData) si hubo cambios
        if (anySynergyFormed)
        {
            data.initialPowerUps = new List<PowerUp>(currentPerks);
            cachedPlayer.initialPowerUps = currentPerks.ToArray();

            // Guardar
            cachedPlayer.SavePlayerData();

            Debug.Log($"[SynergyManager] Sinergias aplicadas. Perks actuales: {currentPerks.Count}");
        }
        else
        {
            Debug.Log("[SynergyManager] No se formó ninguna sinergia.");
        }
    }

    /// <summary>
    /// Devuelve true si el jugador tiene todos los ingredientes.
    /// </summary>
    private bool PlayerHasAllIngredients(List<PowerUp> playerPerks, List<PowerUp> ingredients)
    {
        foreach (var ing in ingredients)
        {
            if (ing == null) return false;
            if (!playerPerks.Contains(ing))
                return false;
        }
        return true;
    }
}
