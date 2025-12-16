using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "LifeUpPowerUp", menuName = "PowerUps/Life Up")]
public class LifeUpPowerUp : PowerUp
{
    public float extraMaxHealth = 0.5f;

    public override void Apply(PlayerController player)
    {
        if (GameObject.Find("LifeUpMarker") != null) return;

        GameObject marker = new GameObject("LifeUpMarker");
        Object.DontDestroyOnLoad(marker);

        player.StartCoroutine(ApplyDelayed(player, marker));
    }

    private IEnumerator ApplyDelayed(PlayerController player, GameObject marker)
    {
        yield return new WaitForEndOfFrame();

        var health = player.GetComponent<PlayerHealth>();
        if (health == null)
        {
            Object.Destroy(marker);
            yield break;
        }

        // 1) aplicar LifeUp base
        health.maxHealth += extraMaxHealth;
        health.currentHealth = Mathf.Min(health.currentHealth + extraMaxHealth, health.maxHealth);

        if (health.healthUI != null)
        {
            health.healthUI.Initialize(health.maxHealth);
            health.healthUI.UpdateHearts(health.currentHealth);
        }

        // 2) avisar al mendigo (tu sistema especial de LifeUp)
        BeggarValueObserver.NotifyLifeUpApplied();

        // 3) auto-remover de la lista
        var list = new List<PowerUp>(player.initialPowerUps);
        if (list.Contains(this))
        {
            list.Remove(this);
            player.initialPowerUps = list.ToArray();
        }

        // 4) guardar al final para que persista (vida + lista ya limpia)
        if (GameDataManager.Instance != null)
            GameDataManager.Instance.SavePlayerData(player);

        Object.Destroy(marker);
    }

    public override void Remove(PlayerController player)
    {
    }
}