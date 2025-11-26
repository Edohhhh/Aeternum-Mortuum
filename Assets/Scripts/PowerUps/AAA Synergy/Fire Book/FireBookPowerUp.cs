using UnityEngine;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "FireBookPowerUp", menuName = "PowerUps/Fire Book")]
public class FireBookPowerUp : PowerUp
{
    [Header("Prefab de la llama")]
    public GameObject fireBookPrefab;

    [Header("Daño directo")]
    public float damagePerSecond = 4f;

    [Header("Quemadura")]
    public float burnDamagePerSecond = 2f;
    public float burnDuration = 5f;

    [Header("Recoil")]
    public float recoilForce = 4f;

    [Header("Olas de fuego")]
    public int flamesPerWave = 5;
    public float flameSpacing = 0.5f;
    public float fireOffset = 0.7f;
    public float fireCooldown = 0.3f;

    public override void Apply(PlayerController player)
    {
        if (!player) return;

        var controller = player.GetComponent<FireBookController>();
        if (!controller)
            controller = player.gameObject.AddComponent<FireBookController>();

        controller.fireBookPrefab = fireBookPrefab;
        controller.recoilForce = recoilForce;

        controller.flamesPerWave = flamesPerWave;
        controller.flameSpacing = flameSpacing;
        controller.fireOffset = fireOffset;
        controller.fireCooldown = fireCooldown;

        controller.damagePerSecond = damagePerSecond;
        controller.burnDamagePerSecond = burnDamagePerSecond;
        controller.burnDuration = burnDuration;

        controller.enabled = true;
    }

    public override void Remove(PlayerController player)
    {
        var controller = player.GetComponent<FireBookController>();
        if (controller != null)
            Object.Destroy(controller);
    }
}
