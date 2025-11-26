using UnityEngine;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "FireBookPowerUp", menuName = "PowerUps/Fire Book")]
public class FireBookPowerUp : PowerUp
{
    [Header("Prefab del cono de fuego")]
    public GameObject fireBookPrefab;

    [Header("Daño directo")]
    public float damagePerSecond = 4f;

    [Header("Quemadura")]
    public float burnDamagePerSecond = 2f;
    public float burnDuration = 5f;

    [Header("Recoil")]
    public float recoilForce = 5f;

    public override void Apply(PlayerController player)
    {
        if (player == null) return;

        var controller = player.GetComponent<FireBookController>();
        if (controller == null)
            controller = player.gameObject.AddComponent<FireBookController>();

        controller.fireBookPrefab = fireBookPrefab;
        controller.damagePerSecond = damagePerSecond;
        controller.burnDamagePerSecond = burnDamagePerSecond;
        controller.burnDuration = burnDuration;
        controller.recoilForce = recoilForce;

        controller.enabled = true;
    }

    public override void Remove(PlayerController player)
    {
        var controller = player.GetComponent<FireBookController>();
        if (controller != null)
            controller.enabled = false;
    }
}
