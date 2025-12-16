using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "ExtraSpinUp", menuName = "PowerUps/Extra Spin +1")]
public class ExtraSpinUp : PowerUp
{
    private void OnEnable()
    {
        isPermanent = true;
    }

    public override void Apply(PlayerController player)
    {
        if (player == null) return;

        if (GameObject.Find("ExtraSpinUpMarker") != null) return;

        GameObject marker = new GameObject("ExtraSpinUpMarker");
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

        player.extraSpins += 1;
        Debug.Log($"PowerUp aplicado: Tiradas extra ahora en {player.extraSpins}");

        // ✅ avisar al mendigo (para que sume su bonus)
        BeggarValueObserver.NotifyTargetPerkApplied(this);

        // one-shot: borrar de la lista
        var list = new List<PowerUp>(player.initialPowerUps);
        if (list.Contains(this))
        {
            list.Remove(this);
            player.initialPowerUps = list.ToArray();
        }

        // ✅ guardar al final (para que persista y quede removido)
        if (GameDataManager.Instance != null)
            GameDataManager.Instance.SavePlayerData(player);

        Object.Destroy(marker);
    }

    public override void Remove(PlayerController player) { }
}