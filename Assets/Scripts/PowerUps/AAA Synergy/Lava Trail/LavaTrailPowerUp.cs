using UnityEngine;

[CreateAssetMenu(fileName = "LavaTrailPowerUp", menuName = "PowerUps/Lava Trail")]
public class LavaTrailPowerUp : PowerUp
{
    [Header("Charco de lava")]
    public GameObject lavaTrailPrefab;
    public float trailLifetime = 3f;

    public override void Apply(PlayerController player)
    {
        if (player == null) return;

        var dashTrail = player.GetComponent<LavaTrailDash>();
        if (dashTrail == null)
            dashTrail = player.gameObject.AddComponent<LavaTrailDash>();

        dashTrail.trailPrefab = lavaTrailPrefab;
        dashTrail.trailDuration = trailLifetime;
        dashTrail.enabled = true;
    }

    public override void Remove(PlayerController player)
    {
        if (player == null) return;

        var dashTrail = player.GetComponent<LavaTrailDash>();
        if (dashTrail != null)
            dashTrail.enabled = false;
    }
}
