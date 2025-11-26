using UnityEngine;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "IceBookPowerUp", menuName = "PowerUps/Ice Book")]
public class IceBookPowerUp : PowerUp
{
    [Header("Prefab del hielo")]
    public GameObject iceBookPrefab;

    [Header("Recoil")]
    public float recoilForce = 4f;

    [Header("Olas de hielo")]
    public int shardsPerWave = 5;
    public float shardSpacing = 0.5f;
    public float iceOffset = 0.7f;
    public float fireCooldown = 0.3f;

    [Header("Config de congelación")]
    public float freezeDuration = 2f;
    public float tickInterval = 0.2f;
    public float shardLifeTime = 1.5f;
    public GameObject freezeVfxPrefab;

    public override void Apply(PlayerController player)
    {
        if (player == null) return;

        var controller = player.GetComponent<IceBookController>();
        if (controller == null)
            controller = player.gameObject.AddComponent<IceBookController>();

        controller.iceBookPrefab = iceBookPrefab;
        controller.recoilForce = recoilForce;

        controller.shardsPerWave = shardsPerWave;
        controller.shardSpacing = shardSpacing;
        controller.iceOffset = iceOffset;
        controller.fireCooldown = fireCooldown;


        controller.freezeDuration = freezeDuration;
        controller.tickInterval = tickInterval;
        controller.shardLifeTime = shardLifeTime;
        controller.freezeVfxPrefab = freezeVfxPrefab;

        controller.enabled = true;
    }

    public override void Remove(PlayerController player)
    {
        if (player == null) return;

        var controller = player.GetComponent<IceBookController>();
        if (controller != null)
        {
            Object.Destroy(controller);
        }

    }
}
