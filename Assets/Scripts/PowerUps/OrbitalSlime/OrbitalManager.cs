using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class OrbitalManager : MonoBehaviour
{
    private List<GameObject> orbitals = new List<GameObject>();
    private OrbitalPowerUp config;
    private PlayerController player;
    private int stackCount = 0;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void AddStack(OrbitalPowerUp cfg, PlayerController newPlayer)
    {
        config = cfg;

        if (player != newPlayer)
        {
            ClearOrbitalsInternal();
            stackCount = 0;
            player = newPlayer;
        }

        stackCount++;
        RebuildOrbitals();
    }

    private void RebuildOrbitals()
    {
        // limpiar instancias viejas
        ClearOrbitalsInternal();

        if (config == null || player == null || stackCount <= 0)
            return;

        for (int i = 0; i < stackCount; i++)
        {
            var go = Object.Instantiate(config.orbitalPrefab);
            var orbital = go.AddComponent<PlayerOrbital>();

            float angleOffset = (360f / stackCount) * i;
            float adjustedSpeed = config.rotationSpeed * ((i % 2 == 0) ? 1f : -1f);

            orbital.Initialize(player.transform, config.orbitRadius, adjustedSpeed, config.damagePerSecond);
            orbital.SetInitialAngle(angleOffset);

            orbitals.Add(go);
        }
    }

    public void ClearOrbitals()
    {
        ClearOrbitalsInternal();
        stackCount = 0;
    }

    private void ClearOrbitalsInternal()
    {
        foreach (var orb in orbitals)
            if (orb != null) Object.Destroy(orb);

        orbitals.Clear();
    }
}
