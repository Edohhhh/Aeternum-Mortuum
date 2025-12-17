using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarWorld : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private EnemyHealth targetHealth;
    [SerializeField] private Image fillImage;         // vida real
    [SerializeField] private Image delayedFillImage;  // vida "atrasada"
    [SerializeField] private Transform followTarget;

    [Header("Follow")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, 0f);

    [Header("Damage FX")]
    [SerializeField] private float delayBeforeCatchUp = 1f;   // 1 segundo
    [SerializeField] private float catchUpSpeed = 3f;         // qué tan rápido alcanza
    [SerializeField] private float shakeDuration = 0.12f;
    [SerializeField] private float shakeStrength = 0.06f;     // movimiento en unidades mundo
    [SerializeField] private int shakeVibrato = 10;

    private float currentFill;        // fill "real"
    private float delayedFill;        // fill "tarde"
    private Coroutine delayedRoutine;
    private Coroutine shakeRoutine;

    private Vector3 baseLocalPos;

    private void Awake()
    {
        if (targetHealth == null)
            targetHealth = GetComponentInParent<EnemyHealth>();

        followTarget = targetHealth != null ? targetHealth.transform : transform.parent;

        baseLocalPos = transform.localPosition;

        if (targetHealth != null)
        {
            targetHealth.OnDamaged += OnDamaged;
            targetHealth.OnDeath += OnDeath;
        }

        // Inicializar valores
        float pct = GetHealthPercent();
        currentFill = pct;
        delayedFill = pct;

        ApplyFillsImmediate();
    }

    private void LateUpdate()
    {
        if (followTarget != null)
            transform.position = followTarget.position + offset;
    }

    private void OnDamaged()
    {
        // Actualiza vida real al instante
        float pct = GetHealthPercent();
        currentFill = pct;

        if (fillImage != null)
            fillImage.fillAmount = currentFill;

        // Vibración
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(Shake());

        // La barra delayed: se queda donde estaba y después alcanza
        if (delayedRoutine != null) StopCoroutine(delayedRoutine);
        delayedRoutine = StartCoroutine(DelayedCatchUp());
    }

    private IEnumerator DelayedCatchUp()
    {
        // Espera 1 segundo con el pedazo "perdido" visible
        yield return new WaitForSeconds(delayBeforeCatchUp);

        // Baja suave hasta la vida real
        while (delayedFill > currentFill + 0.0001f)
        {
            delayedFill = Mathf.MoveTowards(delayedFill, currentFill, catchUpSpeed * Time.deltaTime);

            if (delayedFillImage != null)
                delayedFillImage.fillAmount = delayedFill;

            yield return null;
        }

        delayedFill = currentFill;
        if (delayedFillImage != null)
            delayedFillImage.fillAmount = delayedFill;
    }

    private IEnumerator Shake()
    {
        float t = 0f;
        while (t < shakeDuration)
        {
            // vibración random chiquita
            Vector2 rnd = Random.insideUnitCircle * shakeStrength;
            transform.localPosition = baseLocalPos + new Vector3(rnd.x, rnd.y, 0f);

            t += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = baseLocalPos;
    }

    private void ApplyFillsImmediate()
    {
        if (fillImage != null) fillImage.fillAmount = currentFill;
        if (delayedFillImage != null) delayedFillImage.fillAmount = delayedFill;
    }

    private float GetHealthPercent()
    {
        if (targetHealth == null) return 1f;
        float max = Mathf.Max(1f, targetHealth.maxHealth);
        float cur = Mathf.Clamp(targetHealth.GetCurrentHealth(), 0f, max);
        return cur / max;
    }

    private void OnDeath()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (targetHealth != null)
        {
            targetHealth.OnDamaged -= OnDamaged;
            targetHealth.OnDeath -= OnDeath;
        }
    }
}

//using UnityEngine;
//using UnityEngine.UI;

//public class BossHealthBarWorld : MonoBehaviour
//{
//    [Header("Refs")]
//    [SerializeField] private EnemyHealth targetHealth;
//    [SerializeField] private Image fillImage;
//    [SerializeField] private Transform followTarget;

//    [Header("Follow")]
//    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, 0f);

//    private void Awake()
//    {
//        // Buscar EnemyHealth en el padre (el boss)
//        if (targetHealth == null)
//            targetHealth = GetComponentInParent<EnemyHealth>();

//        // Seguir SIEMPRE al boss
//        followTarget = targetHealth != null ? targetHealth.transform : transform.parent;

//        // Buscar automáticamente la imagen Fill
//        if (fillImage == null)
//            fillImage = GetComponentInChildren<Image>();

//        if (targetHealth != null)
//        {
//            targetHealth.OnDamaged += Refresh;
//            targetHealth.OnDeath += OnDeath;
//        }

//        Refresh();
//    }

//    private void LateUpdate()
//    {
//        if (followTarget != null)
//            transform.position = followTarget.position + offset;
//    }

//    private void Refresh()
//    {
//        if (targetHealth == null || fillImage == null) return;

//        float max = Mathf.Max(1f, targetHealth.maxHealth);
//        float current = Mathf.Clamp(targetHealth.GetCurrentHealth(), 0f, max);

//        fillImage.fillAmount = current / max;
//    }

//    private void OnDeath()
//    {
//        gameObject.SetActive(false);
//    }

//    private void OnDestroy()
//    {
//        if (targetHealth != null)
//        {
//            targetHealth.OnDamaged -= Refresh;
//            targetHealth.OnDeath -= OnDeath;
//        }
//    }
//}


