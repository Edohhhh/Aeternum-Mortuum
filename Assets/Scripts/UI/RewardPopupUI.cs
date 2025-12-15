using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class RewardPopupUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private CanvasGroup popupCanvas;
    [SerializeField] private Image rewardImage;
    [SerializeField] private TextMeshProUGUI rewardName;
    [SerializeField] private TextMeshProUGUI rewardDescription;

    [Header("Configuraci?n")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private KeyCode closeKey = KeyCode.F;

    private Tween fadeTween;
    private bool isVisible = false;

    private void Awake()
    {
        popupCanvas.alpha = 0f;
        popupCanvas.interactable = false;
        popupCanvas.blocksRaycasts = false;
    }

    private void Update()
    {
        if (isVisible && Input.GetKeyDown(closeKey))
        {
            Hide();
        }
    }

    public void ShowReward(Sprite sprite, string name, string description)
    {
        rewardImage.sprite = sprite;
        rewardName.text = name;
        rewardDescription.text = description;

        popupCanvas.interactable = true;
        popupCanvas.blocksRaycasts = true;

        fadeTween?.Kill();
        fadeTween = popupCanvas.DOFade(1f, fadeDuration);

        isVisible = true;
    }

    public void Hide()
    {
        if (!isVisible) return;

        isVisible = false;

        fadeTween?.Kill();
        fadeTween = popupCanvas.DOFade(0f, fadeDuration)
            .OnComplete(() =>
            {
                popupCanvas.interactable = false;
                popupCanvas.blocksRaycasts = false;
            });
    }
}
