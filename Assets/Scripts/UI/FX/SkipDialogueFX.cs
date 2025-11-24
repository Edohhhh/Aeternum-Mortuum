using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class SkipDialogueFX : MonoBehaviour
{
    [Header("Ajustes del Efecto")]
    [Tooltip("Tiempo que tarda en ir de opacidad máxima a mínima.")]
    public float fadeDuration = 0.8f;

    [Tooltip("Opacidad mínima a la que llegará el objeto.")]
    [Range(0f, 1f)]
    public float minAlpha = 0.3f;

    [Tooltip("Opacidad máxima a la que llegará el objeto.")]
    [Range(0f, 1f)]
    public float maxAlpha = 1f;

    private CanvasGroup canvasGroup;
    private Tween currentTween;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Al activarse el objeto (cuando aparece el diálogo), iniciamos el efecto automáticamente.
    /// </summary>
    void OnEnable()
    {
        // Reseteamos la opacidad a 0 para que haga una entrada suave
        canvasGroup.alpha = 0f;
        StartEffect();
    }

    /// <summary>
    /// Al desactivarse, limpiamos las animaciones para evitar errores.
    /// </summary>
    void OnDisable()
    {
        currentTween?.Kill();
    }

    public void StartEffect()
    {
        currentTween?.Kill();

        // 1. Fade-in inicial: de invisible (0) a la opacidad mínima
        currentTween = canvasGroup.DOFade(minAlpha, fadeDuration * 0.5f)
                                  .SetEase(Ease.OutQuad)
                                  .OnComplete(() =>
                                  {
                                      // 2. Loop de pulsación: Va a Max -> Min -> Max... infinitamente
                                      currentTween = DOTween.Sequence()
                                          .Append(canvasGroup.DOFade(maxAlpha, fadeDuration).SetEase(Ease.InOutSine))
                                          .Append(canvasGroup.DOFade(minAlpha, fadeDuration).SetEase(Ease.InOutSine))
                                          .SetLoops(-1, LoopType.Restart); // -1 indica repetición infinita
                                  });
    }

    // Función opcional por si quieres cerrarlo suavemente desde un evento de botón UI
    public void StopEffect()
    {
        currentTween?.Kill();
        canvasGroup.DOFade(0f, 0.2f).OnComplete(() => gameObject.SetActive(false));
    }
}