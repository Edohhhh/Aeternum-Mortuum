using UnityEngine;
using TMPro;
using DG.Tweening;

public class InventoryTooltipUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private RectTransform rectTransform;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        Hide();
    }

    private void Update()
    {
        // Seguir el mouse si el tooltip es visible
        if (canvasGroup.alpha > 0f)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform.parent as RectTransform,
                Input.mousePosition,
                mainCamera,
                out pos);

            // Offset para que no tape el cursor
            rectTransform.anchoredPosition = pos + new Vector2(40, -40);
        }
    }

    public void Show(string name, string description)
    {
        nameText.text = name;
        descriptionText.text = description;

        gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = false; // IMPORTANTE: El tooltip nunca debe bloquear raycast

        // Animación ignorando TimeScale (funciona en pausa)
        canvasGroup.DOFade(1f, 0.2f).From(0f).SetUpdate(true);
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}