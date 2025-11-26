using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(Rigidbody2D))]
public class FireBookController : MonoBehaviour
{
    [Header("Prefab de la llama")]
    public GameObject fireBookPrefab;

    [Header("Stats de daño")]
    public float damagePerSecond = 4f;
    public float burnDamagePerSecond = 2f;
    public float burnDuration = 5f;

    [Header("Olas de fuego")]
    public int flamesPerWave = 5;
    public float flameSpacing = 0.5f;
    public float fireOffset = 0.7f;
    public float fireCooldown = 0.3f;

    [Header("Recoil")]
    public float recoilForce = 4f;

    [Header("Origen del fuego")]
    public Transform fireOrigin;

    private PlayerController player;
    private Rigidbody2D rb;
    private Camera cam;
    private float fireTimer;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        if (!fireOrigin) fireOrigin = transform;
    }

    private void Update()
    {
        fireTimer -= Time.deltaTime;

        if (!CanCast()) return;
        if (!Input.GetMouseButton(0)) return;

        if (!cam) cam = Camera.main;
        if (!cam) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector3 origin = fireOrigin.position;
        Vector2 dir = (mouseWorld - origin).normalized;

        if (fireTimer <= 0f)
        {
            SpawnFlameWave(origin, dir);
            ApplyRecoil(dir);
            fireTimer = fireCooldown;
        }
    }

    private bool CanCast()
    {
        return player && player.canMove &&
            player.stateMachine.CurrentState != player.DashState &&
            player.stateMachine.CurrentState != player.RecoilState &&
            player.stateMachine.CurrentState != player.KnockbackState;
    }

    private void SpawnFlameWave(Vector3 origin, Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        for (int i = 0; i < flamesPerWave; i++)
        {
            Vector3 pos = origin + (Vector3)(dir * (fireOffset + flameSpacing * i));

            GameObject flame = Instantiate(
                fireBookPrefab,
                pos,
                Quaternion.AngleAxis(angle, Vector3.forward)
            );

            var hitbox = flame.GetComponent<FireBookHitbox>();
            if (!hitbox) hitbox = flame.AddComponent<FireBookHitbox>();

            hitbox.damagePerSecond = damagePerSecond;
            hitbox.burnDamagePerSecond = burnDamagePerSecond;
            hitbox.burnDuration = burnDuration;

            hitbox.SetDirection(dir);
        }
    }

    private void ApplyRecoil(Vector2 dir)
    {
        rb.AddForce(-dir * recoilForce, ForceMode2D.Impulse);
    }
}
