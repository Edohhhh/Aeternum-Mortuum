using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class BeggarValueObserver : MonoBehaviour
{
    [HideInInspector] public float bonusPerStat = 1f; // +1 por stack

    private readonly List<PowerUp> targetPerks = new();
    private readonly Dictionary<PowerUp, int> appliedStacksByPerk = new();

    // LifeUp: stacks permanentes
    private int lifeUpStacks = 0;

    public void SetTargets(List<PowerUp> list)
    {
        targetPerks.Clear();
        if (list != null) targetPerks.AddRange(list);
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public static void RequestReapply()
    {
        var go = GameObject.Find("BeggarValueObserver");
        if (go == null) return;

        var obs = go.GetComponent<BeggarValueObserver>();
        if (obs != null)
            obs.ReapplyNow();
    }

    public static void NotifyTargetPerkApplied(PowerUp appliedPerk)
    {
        if (appliedPerk == null) return;

        var go = GameObject.Find("BeggarValueObserver");
        if (go == null) return;

        var obs = go.GetComponent<BeggarValueObserver>();
        if (obs == null) return;

        if (!obs.targetPerks.Contains(appliedPerk))
            return;

        if (!obs.appliedStacksByPerk.ContainsKey(appliedPerk))
            obs.appliedStacksByPerk[appliedPerk] = 0;

        obs.appliedStacksByPerk[appliedPerk] += 1;

        obs.ReapplyNow();
    }

    // ✅ LifeUp: suma stack y reaplica (sin flags)
    public static void NotifyLifeUpApplied()
    {
        var go = GameObject.Find("BeggarValueObserver");
        if (go == null) return;

        var obs = go.GetComponent<BeggarValueObserver>();
        if (obs != null)
        {
            obs.lifeUpStacks++;
            obs.ReapplyNow();
        }
    }

    public void ReapplyNow()
    {
        var player = Object.FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        var tracker = player.GetComponent<StatAugmentTracker>();
        if (tracker == null)
            tracker = player.gameObject.AddComponent<StatAugmentTracker>();

        // Quitar buff previo del mendigo
        tracker.RemoveFrom(player);

        int addDamage = 0;
        float addMoveSpeed = 0f;
        float addMaxHealth = 0f;
        float multMoveSpeed = 1f;
        int addExtraSpins = 0;

        foreach (var kv in appliedStacksByPerk)
        {
            var perkAsset = kv.Key;
            int stacks = kv.Value;

            if (perkAsset == null || stacks <= 0) continue;
            if (!targetPerks.Contains(perkAsset)) continue;

            if (perkAsset is AttackUp)
                addDamage += Mathf.RoundToInt(bonusPerStat * stacks);

            if (perkAsset is PlayerSpeedPowerUp)
                addMoveSpeed += bonusPerStat * stacks;

            if (perkAsset is ExtraSpinUp)
                addExtraSpins += Mathf.RoundToInt(bonusPerStat * stacks);
        }

        // ✅ LifeUp SIEMPRE suma mientras haya stacks
        if (lifeUpStacks > 0)
            addMaxHealth += bonusPerStat * lifeUpStacks;

        tracker.ApplyTo(player, addDamage, addMoveSpeed, addMaxHealth, multMoveSpeed, addExtraSpins);
    }
}