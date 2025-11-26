using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(Rigidbody2D))]
public class IceBookController : MonoBehaviour
{
    [Header("Prefab del hielo")]
    public GameObject iceBookPrefab;

    [Header("Olas de hielo")]
    public int shardsPerWave = 5;
    public float shardSpacing = 0.5f;
    public float iceOffset = 0.7f;
    public float fireCooldown = 0.3f;

    [Header("Recoil")]
    public float recoilForce = 4f;

    [Header("Origen del hielo")]
    public Transform iceOrigin;

    [Header("Config de congelación")]
    public float freezeDuration = 2f;
    public float tickInterval = 0.2f;
    public float shardLifeTime = 1.5f;
    public GameObject freezeVfxPrefab;

    private PlayerController player;
    private Rigidbody2D rb;
    private Camera cam;
    private float fireTimer = 0f;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        if (iceOrigin == null)
            iceOrigin = transform;
    }

    private void Update()
    {
        fireTimer -= Time.deltaTime;

        if (!CanCast())
            return;

        bool pressing = Input.GetMouseButton(0);
        if (!pressing)
            return;

        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
        }

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector3 origin = iceOrigin.position;
        Vector2 dir = (mouseWorld - origin);
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right;
        else
            dir.Normalize();

        if (fireTimer <= 0f)
        {
            SpawnIceWave(origin, dir);
            ApplyRecoil(dir);
            fireTimer = fireCooldown;
        }
    }

    private bool CanCast()
    {
        return
            player != null &&
            player.canMove &&
            player.stateMachine.CurrentState != player.DashState &&
            player.stateMachine.CurrentState != player.RecoilState &&
            player.stateMachine.CurrentState != player.KnockbackState;
    }

    private void SpawnIceWave(Vector3 origin, Vector2 dir)
    {
        if (iceBookPrefab == null) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        for (int i = 0; i < shardsPerWave; i++)
        {
            float dist = iceOffset + shardSpacing * i;
            Vector3 pos = origin + (Vector3)(dir * dist);

            GameObject shard = Instantiate(
                iceBookPrefab,
                pos,
                Quaternion.AngleAxis(angle, Vector3.forward)
            );

            var hitbox = shard.GetComponent<IceBookHitbox>();
            if (hitbox == null)
                hitbox = shard.AddComponent<IceBookHitbox>();

            hitbox.freezeDuration = freezeDuration;
            hitbox.tickInterval = tickInterval;
            hitbox.lifeTime = shardLifeTime;
            hitbox.freezeVfxPrefab = freezeVfxPrefab;

            hitbox.SetDirection(dir);
        }
    }

    private void ApplyRecoil(Vector2 dir)
    {
        if (rb == null) return;

        Vector2 recoilDir = -dir.normalized;
        rb.AddForce(recoilDir * recoilForce, ForceMode2D.Impulse);
    }
}
