using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class FireBookController : MonoBehaviour
{
    [Header("Prefab del cono de fuego")]
    public GameObject fireBookPrefab;

    [Header("Stats")]
    public float damagePerSecond = 4f;
    public float burnDamagePerSecond = 2f;
    public float burnDuration = 5f;
    public float recoilForce = 5f;

    [Header("Origen del fuego")]
    [Tooltip("Punto desde donde sale el fuego. Si es nulo, se usa la posición del jugador.")]
    public Transform fireOrigin;

    [Tooltip("Distancia hacia adelante desde el origen para colocar el cono de fuego.")]
    public float fireOffset = 0.7f;

    private PlayerController player;
    private Rigidbody2D rb;

    private GameObject currentFire;
    private FireBookHitbox currentHitbox;
    private Camera cam;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        if (fireOrigin == null)
            fireOrigin = transform;

        if (cam == null)
        {
            Debug.LogWarning("[FireBook] No hay cámara con tag MainCamera en la escena. El apuntado al mouse puede fallar.");
        }
    }

    private void Update()
    {
        if (!CanCast())
        {
            StopFire();
            return;
        }

        bool pressing = Input.GetMouseButton(0); // click izquierdo
        if (!pressing)
        {
            StopFire();
            return;
        }

        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null)
            {
                // No podemos calcular bien la posición del mouse sin cámara
                return;
            }
        }

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector3 origin = fireOrigin.position;
        Vector2 dir = (mouseWorld - origin);
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right;
        else
            dir.Normalize();

        if (currentFire == null)
        {
            SpawnFire(origin, dir);
        }

        AimFire(origin, dir);
        ApplyRecoil(dir);
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

    private void SpawnFire(Vector3 origin, Vector2 dir)
    {
        if (fireBookPrefab == null) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Vector3 spawnPos = origin + (Vector3)(dir * fireOffset);

        currentFire = Instantiate(
            fireBookPrefab,
            spawnPos,
            Quaternion.AngleAxis(angle, Vector3.forward)
        );

        currentHitbox = currentFire.GetComponent<FireBookHitbox>();
        if (currentHitbox == null)
            currentHitbox = currentFire.AddComponent<FireBookHitbox>();

        currentHitbox.damagePerSecond = damagePerSecond;
        currentHitbox.burnDamagePerSecond = burnDamagePerSecond;
        currentHitbox.burnDuration = burnDuration;
        currentHitbox.SetDirection(dir);
    }

    private void AimFire(Vector3 origin, Vector2 dir)
    {
        if (currentFire == null) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Vector3 spawnPos = origin + (Vector3)(dir * fireOffset);

        currentFire.transform.position = spawnPos;
        currentFire.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        if (currentHitbox != null)
        {
            currentHitbox.SetDirection(dir);
        }
    }

    private void ApplyRecoil(Vector2 dir)
    {
        if (rb == null) return;

        Vector2 recoilDir = -dir.normalized;
        rb.AddForce(recoilDir * recoilForce * Time.deltaTime, ForceMode2D.Force);
    }

    private void StopFire()
    {
        if (currentFire != null)
        {
            Destroy(currentFire);
            currentFire = null;
            currentHitbox = null;
        }
    }

    private void OnDisable()
    {
        StopFire();
    }
}
