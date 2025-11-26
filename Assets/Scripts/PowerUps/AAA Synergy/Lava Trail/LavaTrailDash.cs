using UnityEngine;

public class LavaTrailDash : MonoBehaviour
{
    [Tooltip("Prefab del charco de lava que se spawnea durante el dash")]
    public GameObject trailPrefab;

    [Tooltip("Cuánto dura el charco de lava en el mundo")]
    public float trailDuration = 3f;

    private PlayerController player;
    private float spawnCooldown = 0.05f;
    private float timer;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (player == null) return;

        // Sólo spawnear mientras estamos en el DashState
        if (player.stateMachine.CurrentState != player.DashState)
            return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnTrail();
            timer = spawnCooldown;
        }
    }

    private void SpawnTrail()
    {
        if (trailPrefab == null) return;

        var trail = Instantiate(trailPrefab, transform.position, Quaternion.identity);
        Destroy(trail, trailDuration);
    }
}
