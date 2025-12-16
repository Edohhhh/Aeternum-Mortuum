using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "AttackUp", menuName = "PowerUps/Damage Up +15%")]
public class AttackUp : PowerUp
{
    [Tooltip("Multiplicador por pickup (1.15 = +15%)")]
    public float damageMultiplier = 1.15f;

    public override void Apply(PlayerController player)
    {
        if (player == null) return;

        // Evitar doble aplicaciónen que 
        if (GameObject.Find("AttackUpMarker") != null) return;

        GameObject marker = new GameObject("AttackUpMarker");
        Object.DontDestroyOnLoad(marker);

        player.StartCoroutine(ApplyDelayed(player, marker));
    }

    private IEnumerator ApplyDelayed(PlayerController player, GameObject marker)
    {
        yield return new WaitForEndOfFrame();

        if (player == null)
        {
            Object.Destroy(marker);
            yield break;
        }

        int before = Mathf.Max(1, player.baseDamage);
        int after = Mathf.CeilToInt(before * damageMultiplier);
        if (after <= before) after = before + 1;

        player.baseDamage = after;

        BeggarValueObserver.NotifyTargetPerkApplied(this);

        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.SavePlayerData(player);
        }

        BeggarValueObserver.RequestReapply();

        // 🔻 Auto-remover de la lista (como LifeUp)
        var list = new List<PowerUp>(player.initialPowerUps);
        if (list.Contains(this))
        {
            list.Remove(this);
            player.initialPowerUps = list.ToArray();
        }

        Object.Destroy(marker);
    }

    public override void Remove(PlayerController player)
    {
        // vacío (como tu intención original)
    }
}