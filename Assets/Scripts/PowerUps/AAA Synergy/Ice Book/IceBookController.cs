using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class IceBookController : MonoBehaviour
{
    [Header("Prefab del cono de hielo")]
    public GameObject iceBookPrefab;

    [Header("Stats")]
    public float damagePerSecond = 3f;
    public float freezeDuration = 2f;
    public float recoilForce = 5f;

    [Header("Origen del disparo")]
    [Tooltip("Punto desde donde sale el cono. Si es nulo, se usa la posición del jugador.")]
    public Transform fireOrigin;

    [Tooltip("Distancia hacia adelante desde el origen para colocar el cono.")]
    public float fireOffset = 0.7f;

    private PlayerController player;
    private Rigidbody2D rb;

    private GameObject currentCone;
    private IceBookHitbox currentHitbox;
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
            Debug.LogWarning("[IceBook] No hay cámara con tag MainCamera. El apuntado al mouse puede fallar.");
        }
    }

    private void Update()
    {
        if (!CanCast())
        {
            StopCone();
            return;
        }

        bool pressing = Input.GetMouseButton(0); // click izquierdo
        if (!pressing)
        {
            StopCone();
            return;
        }

        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
        }

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector3 origin = fireOrigin.position;
        Vector2 dir = (mouseWorld - origin);
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right;
        else
            dir.Normalize();

        if (currentCone == null)
        {
            SpawnCone(origin, dir);
        }

        AimCone(origin, dir);
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

    private void SpawnCone(Vector3 origin, Vector2 dir)
    {
        if (iceBookPrefab == null) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Vector3 pos = origin + (Vector3)(dir * fireOffset);

        currentCone = Instantiate(
            iceBookPrefab,
            pos,
            Quaternion.AngleAxis(angle, Vector3.forward)
        );

        currentHitbox = currentCone.GetComponent<IceBookHitbox>();
        if (currentHitbox == null)
            currentHitbox = currentCone.AddComponent<IceBookHitbox>();

        currentHitbox.damagePerSecond = damagePerSecond;
        currentHitbox.freezeDuration = freezeDuration;
        currentHitbox.SetDirection(dir);
    }

    private void AimCone(Vector3 origin, Vector2 dir)
    {
        if (currentCone == null) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Vector3 pos = origin + (Vector3)(dir * fireOffset);

        currentCone.transform.position = pos;
        currentCone.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        if (currentHitbox != null)
            currentHitbox.SetDirection(dir);
    }

    private void ApplyRecoil(Vector2 dir)
    {
        if (rb == null) return;

        Vector2 recoilDir = -dir.normalized;
        rb.AddForce(recoilDir * recoilForce * Time.deltaTime, ForceMode2D.Force);
    }

    private void StopCone()
    {
        if (currentCone != null)
        {
            Destroy(currentCone);
            currentCone = null;
            currentHitbox = null;
        }
    }

    private void OnDisable()
    {
        StopCone();
    }
}
