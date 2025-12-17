using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;

public enum RockHardDamageMode { AddFlat, ExponentialSteps }

[CreateAssetMenu(fileName = "RockHardPowerUp", menuName = "PowerUps/RockHard!")]
public class RockHardPowerUp : PowerUp
{
    [Header("Daño")]
    public RockHardDamageMode damageMode = RockHardDamageMode.AddFlat;

    [Tooltip("Si damageMode = AddFlat, suma este valor al daño base.")]
    public float flatDamage = 5f;

    [Tooltip("Si damageMode = ExponentialSteps, multiplica: damage *= (expFactor ^ expSteps).")]
    public float expFactor = 1.15f;

    [Tooltip("Cantidad de pasos 'exponenciales' (por default 5 ≈ 2x con expFactor=1.15).")]
    public int expSteps = 5;

    [Header("Movimiento")]
    [Tooltip("Cuánto reduce la velocidad de movimiento (unidades).")]
    public float moveSpeedDelta = -3f;

    [Header("Dash")]
    [Tooltip("Multiplicador al cooldown del dash (1.5 = +50%).")]
    public float dashCooldownMultiplier = 1.5f;

    // Estado para revertir cooldown (no lo usamos para one-shot, pero lo dejo intacto)
    private object dashHost;
    private FieldInfo dashCooldownField;
    private float originalDashCooldown;
    private bool dashPatched;

    public override void Apply(PlayerController player)
    {
        if (player == null) return;

        // ✅ Evitar doble aplicación entre escenas / dobles flujos
        if (GameObject.Find("RockHardMarker") != null) return;

        GameObject marker = new GameObject("RockHardMarker");
        Object.DontDestroyOnLoad(marker);

        // ✅ Aplicar diferido (misma idea que LifeUp)
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

        dashPatched = false;
        dashHost = null;
        dashCooldownField = null;

        // ====== DAÑO ======
        if (damageMode == RockHardDamageMode.AddFlat)
        {
            int add = Mathf.RoundToInt(flatDamage);
            player.baseDamage += add;
        }
        else
        {
            float mult = Mathf.Pow(expFactor, Mathf.Max(0, expSteps));
            player.baseDamage = Mathf.RoundToInt(player.baseDamage * mult);
        }

        // ====== MOVIMIENTO ======
        player.moveSpeed = Mathf.Max(0f, player.moveSpeed + moveSpeedDelta);

        // ====== DASH COOLDOWN (por reflexión) ======
        PatchDashCooldown(player, dashCooldownMultiplier);

        // ✅ One-shot: borrar de la lista para que no vuelva a aplicarse al cambiar de escena
        var list = new List<PowerUp>(player.initialPowerUps);
        bool removedAny = false;
        while (list.Remove(this))
            removedAny = true;

        if (removedAny)
            player.initialPowerUps = list.ToArray();

        // ✅ Guardar al final: stats aplicadas + lista ya limpia
        if (GameDataManager.Instance != null)
            GameDataManager.Instance.SavePlayerData(player);

        Object.Destroy(marker);
    }

    public override void Remove(PlayerController player)
    {
        // One-shot: normalmente no se llama.
        // Lo dejo como estaba por compatibilidad (por si lo usás en algún reset / remover manual).
        if (player == null) return;

        if (damageMode == RockHardDamageMode.AddFlat)
        {
            int add = Mathf.RoundToInt(flatDamage);
            player.baseDamage -= add;
        }
        else
        {
            float mult = Mathf.Pow(expFactor, Mathf.Max(0, expSteps));
            float invMult = 1f / mult;
            player.baseDamage = Mathf.RoundToInt(player.baseDamage * invMult);
        }

        player.moveSpeed = player.moveSpeed - moveSpeedDelta;

        if (dashPatched && dashHost != null && dashCooldownField != null)
        {
            dashCooldownField.SetValue(dashHost, originalDashCooldown);
        }

        dashPatched = false;
        dashHost = null;
        dashCooldownField = null;
    }

    private void PatchDashCooldown(PlayerController player, float multiplier)
    {
        string[] names = { "dashCooldown", "DashCooldown", "dashCd", "dashDelay", "cooldown" };
        var comps = player.GetComponents<MonoBehaviour>();

        foreach (var c in comps)
        {
            if (c == null) continue;
            var t = c.GetType();

            foreach (var n in names)
            {
                var f = t.GetField(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (f != null && f.FieldType == typeof(float))
                {
                    dashHost = c;
                    dashCooldownField = f;

                    originalDashCooldown = (float)dashCooldownField.GetValue(dashHost);
                    float newCd = originalDashCooldown * Mathf.Max(0.01f, multiplier);
                    dashCooldownField.SetValue(dashHost, newCd);

                    dashPatched = true;
                    return;
                }
            }
        }
    }
}
