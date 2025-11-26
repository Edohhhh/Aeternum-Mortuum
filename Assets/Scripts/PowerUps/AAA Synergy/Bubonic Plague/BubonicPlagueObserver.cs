using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Object = UnityEngine.Object;

public class BubonicPlagueObserver : MonoBehaviour
{
    private Transform player;
    private GameObject slimePrefab;
    private GameObject cloudPrefab;
    private float interval;

    private float timer = 0f;

    [Header("Patrón de spawn")]
    [Tooltip("Cuántos segmentos se generan por burst.")]
    public int segmentsPerBurst = 3;

    [Tooltip("Separación entre cada segmento (en unidades de mundo).")]
    public float segmentSpacing = 0.4f;

    // Alternancia entre cuál empieza primero en cada burst
    private bool startWithSlime = true;

    public void Initialize(Transform playerTransform, GameObject slime, GameObject cloud, float spawnInterval)
    {
        player = playerTransform;
        slimePrefab = slime;
        cloudPrefab = cloud;
        interval = Mathf.Max(0.1f, spawnInterval); // seguridad mínimo
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ReassignPlayer());
    }

    private IEnumerator ReassignPlayer()
    {
        PlayerController found = null;
        while (found == null)
        {
            found = Object.FindFirstObjectByType<PlayerController>();
            yield return null; // esperar 1 frame
        }

        player = found.transform;
    }

    private void Update()
    {
        if (player == null) return;

        // Consideramos que "camina" si tiene input de movimiento
        Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (move.sqrMagnitude > 0.01f)
        {
            timer += Time.deltaTime;
            if (timer >= interval)
            {
                SpawnBurst(move);
                timer = 0f;
            }
        }
        else
        {
            // si querés que el tiempo siga corriendo aunque esté quieto, borrá esta línea
            // timer = 0f;
        }
    }

    private void SpawnBurst(Vector2 moveInput)
    {
        if (player == null) return;
        if (segmentsPerBurst <= 0) return;

        // Dirección hacia "atrás" del movimiento
        Vector2 dir = moveInput.normalized;
        if (dir.sqrMagnitude < 0.001f)
        {
            // Si por alguna razón no hay input válido, usar una dirección por defecto
            dir = Vector2.down;
        }

        // Por comodidad, que el trail quede detrás del player
        Vector2 backDir = -dir;

        for (int i = 0; i < segmentsPerBurst; i++)
        {
            Vector3 spawnPos = player.position + (Vector3)(backDir * segmentSpacing * i);

            // Alternamos entre slime y peste dentro del burst,
            // y además cambiamos quién empieza en cada burst.
            bool useSlime =
                startWithSlime
                ? (i % 2 == 0)   // si este burst empieza con slime → slime, nube, slime...
                : (i % 2 != 0);  // si empieza con nube → nube, slime, nube...

            if (useSlime)
            {
                if (slimePrefab != null)
                    Instantiate(slimePrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                if (cloudPrefab != null)
                    Instantiate(cloudPrefab, spawnPos, Quaternion.identity);
            }
        }

        // La próxima vez invertimos quién arranca para realmente alternar
        startWithSlime = !startWithSlime;
    }
}
