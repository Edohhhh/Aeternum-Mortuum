using UnityEngine;

[CreateAssetMenu(fileName = "FrozenTrailPowerUp", menuName = "PowerUps/Frozen Trail")]
public class FrozenTrailPowerUp : PowerUp
{
    [Header("Charco de hielo")]
    public GameObject frozenTrailPrefab;
    public float trailLifetime = 3f;

    public override void Apply(PlayerController player)
    {
        if (player == null) return;

        var dashTrail = player.GetComponent<FrozenTrailDash>();
        if (dashTrail == null)
            dashTrail = player.gameObject.AddComponent<FrozenTrailDash>();

        dashTrail.trailPrefab = frozenTrailPrefab;
        dashTrail.trailDuration = trailLifetime;
        dashTrail.enabled = true;
    }

    public override void Remove(PlayerController player)
    {
        if (player == null) return;

        var dashTrail = player.GetComponent<FrozenTrailDash>();
        if (dashTrail != null)
            dashTrail.enabled = false;
    }
}
