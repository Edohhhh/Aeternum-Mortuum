using UnityEngine;

[CreateAssetMenu(fileName = "SpectralCompanionPowerUp", menuName = "PowerUps/Spectral Companion")]
public class SpectralCompanionPowerUp : PowerUp
{
    public GameObject companionPrefab;
    public GameObject bulletPrefab;
    public float fireInterval = 3f;
    public int damagePerShot = 1;
    public float speedMultiplier = 0.75f;

    private GameObject instance;

    public override void Apply(PlayerController player)
    {
        if (instance != null) return;

        instance = GameObject.Instantiate(companionPrefab);

        var companion = instance.GetComponent<SpectralCompanion>();
        if (companion == null)
            companion = instance.AddComponent<SpectralCompanion>();

        companion.bulletPrefab = bulletPrefab;
        companion.Initialize(player.transform, player.moveSpeed * speedMultiplier, fireInterval, damagePerShot);

        Object.DontDestroyOnLoad(instance);
    }

    public override void Remove(PlayerController player)
    {
        if (instance != null)
        {
            GameObject.Destroy(instance);
            instance = null;
        }
    }
}
