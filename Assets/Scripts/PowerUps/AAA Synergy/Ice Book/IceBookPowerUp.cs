using UnityEngine;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "IceBookPowerUp", menuName = "PowerUps/Ice Book")]
public class IceBookPowerUp : PowerUp
{
    [Header("Prefab del cono de hielo")]
    public GameObject iceBookPrefab;

    [Header("Daño directo")]
    [Tooltip("Daño por segundo mientras el enemigo está dentro del hielo.")]
    public float damagePerSecond = 3f;

    [Header("Freeze")]
    [Tooltip("Duración del congelamiento en segundos.")]
    public float freezeDuration = 2f;

    [Header("Recoil")]
    [Tooltip("Fuerza con la que el disparo empuja al jugador hacia atrás.")]
    public float recoilForce = 5f;

    public override void Apply(PlayerController player)
    {
        if (player == null) return;

        var controller = player.GetComponent<IceBookController>();
        if (controller == null)
            controller = player.gameObject.AddComponent<IceBookController>();

        controller.iceBookPrefab = iceBookPrefab;
        controller.damagePerSecond = damagePerSecond;
        controller.freezeDuration = freezeDuration;
        controller.recoilForce = recoilForce;

        controller.enabled = true;
    }

    public override void Remove(PlayerController player)
    {
        if (player == null) return;

        var controller = player.GetComponent<IceBookController>();
        if (controller != null)
            controller.enabled = false;
    }
}
