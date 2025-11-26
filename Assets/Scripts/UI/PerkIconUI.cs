using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Necesario para detectar el ratón

public class PerkIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;

    private PowerUpEffect powerUpEffect;
    private InventoryTooltipUI tooltip;

    public void Initialize(PowerUpEffect effect, InventoryTooltipUI tooltipUI)
    {
        this.powerUpEffect = effect;
        this.tooltip = tooltipUI;

        if (iconImage == null)
            iconImage = GetComponent<Image>();

        if (powerUpEffect != null)
            iconImage.sprite = powerUpEffect.icon;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // DEBUG: Si esto no sale en consola, el Raycast Target de la IMAGEN de este objeto está apagado
        // o no hay EventSystem en la escena.
        Debug.Log($"Mouse encima del perk: {gameObject.name}");

        if (tooltip != null && powerUpEffect != null)
        {
            tooltip.Show(powerUpEffect.label, powerUpEffect.description);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
        {
            tooltip.Hide();
        }
    }
}