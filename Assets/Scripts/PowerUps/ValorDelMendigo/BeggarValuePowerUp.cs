using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BeggarValuePowerUp", menuName = "PowerUps/Valor del Mendigo")]
public class BeggarValuePowerUp : PowerUp
{
    [Header("Cuánto agregar por perk y por stat detectada")]
    public float bonusPerStat = 2f;

    [Header("Perks a potenciar (arrastrá aquí tus ScriptableObject de stats)")]
    public List<PowerUp> targetPerks = new List<PowerUp>();

    public override void Apply(PlayerController player)
    {
        var existing = GameObject.Find("BeggarValueObserver");
        BeggarValueObserver obs;

        if (existing == null)
        {
            var go = new GameObject("BeggarValueObserver");
            go.name = "BeggarValueObserver";
            obs = go.AddComponent<BeggarValueObserver>();
            Object.DontDestroyOnLoad(go);
        }
        else
        {
            obs = existing.GetComponent<BeggarValueObserver>();
        }

        obs.bonusPerStat = bonusPerStat;
        obs.SetTargets(targetPerks);

        // aplicar ya en la escena actual
        obs.ReapplyNow();
    }

    public override void Remove(PlayerController player)
    {
        var go = GameObject.Find("BeggarValueObserver");
        if (go != null) Object.Destroy(go);
    }
}