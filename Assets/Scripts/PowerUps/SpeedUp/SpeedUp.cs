using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "PlayerSpeedPowerUp", menuName = "PowerUps/Player Speed Up")]
public class PlayerSpeedPowerUp : PowerUp
{
    [Tooltip("Cantidad de velocidad a sumar por pickup")]
    public float speedBonus = 1f;

    public override void Apply(PlayerController player)
    {
        if (player == null) return;

        // Evitar doble aplicación (por loads, doble flujo, etc.)
        if (GameObject.Find("PlayerSpeedUpMarker") != null) return;

        GameObject marker = new GameObject("PlayerSpeedUpMarker");
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

        // ✅ SUMA directa
        player.moveSpeed += speedBonus;

        BeggarValueObserver.NotifyTargetPerkApplied(this);

        // ✅ Remover TODAS las copias del perk (por si estaba duplicado)
        var list = new List<PowerUp>(player.initialPowerUps);
        bool removedAny = false;
        while (list.Remove(this))
            removedAny = true;

        if (removedAny)
            player.initialPowerUps = list.ToArray();

        // ✅ Recalcular sinergias (si aplica)
        BeggarValueObserver.RequestReapply();

        // ✅ Guardar al final: así persiste moveSpeed y la lista ya sin este perk
        if (GameDataManager.Instance != null)
            GameDataManager.Instance.SavePlayerData(player);

        Object.Destroy(marker);
    }

    public override void Remove(PlayerController player)
    {
        // Intencionalmente vacío (one-shot, como LifeUp)
    }
}