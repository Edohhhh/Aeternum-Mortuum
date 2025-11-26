using UnityEngine;
using UnityEngine.UI;
using EasyUI.PickerWheelUI;
using TMPro; // ✅ 1. AÑADIR: Importar TextMeshPro

public class RuletaUISet : MonoBehaviour
{
    [Header("Contenedor donde se instancian los botones")]
    public Transform buttonsContainer;

    [HideInInspector] public PickerWheel linkedWheel;

    public Button selectButton;
    public Button spinButton;
    public Button confirmButton;

    // ✅ 2. MODIFICADO: Cambiar 'Text' por 'TextMeshProUGUI'
    public TextMeshProUGUI spinButtonText;

    private CanvasGroup ruletaCanvasGroup;

    public void Inicializar(PickerWheel wheel, WheelSelector selector)
    {
        // ✅ 1. NUEVO: Limpiar botones antiguos antes de crear los nuevos
        // Esto evita que se sobrepongan botones de temas anteriores o duplicados.
        if (buttonsContainer != null)
        {
            foreach (Transform child in buttonsContainer)
            {
                Destroy(child.gameObject);
            }
        }

        linkedWheel = wheel;

        // CanvasGroup para oscurecer la ruleta
        ruletaCanvasGroup = linkedWheel.GetComponent<CanvasGroup>();
        if (ruletaCanvasGroup == null)
            ruletaCanvasGroup = linkedWheel.gameObject.AddComponent<CanvasGroup>();

        // 🎨 Buscar el tema configurado en el prefab de la ruleta
        RuletaTheme theme = linkedWheel.GetComponent<RuletaTheme>();
        if (theme != null)
        {
            // Instanciar prefabs de botones
            if (theme.selectButtonPrefab != null)
            {
                var obj = Instantiate(theme.selectButtonPrefab, buttonsContainer);
                selectButton = obj.GetComponent<Button>();
            }

            if (theme.spinButtonPrefab != null)
            {
                var obj = Instantiate(theme.spinButtonPrefab, buttonsContainer);
                spinButton = obj.GetComponent<Button>();

                // ✅ Buscar 'TextMeshProUGUI' (ajuste que ya tenías)
                spinButtonText = spinButton.GetComponentInChildren<TextMeshProUGUI>();
            }

            if (theme.confirmButtonPrefab != null)
            {
                var obj = Instantiate(theme.confirmButtonPrefab, buttonsContainer);
                confirmButton = obj.GetComponent<Button>();
            }
        }

        // 🔗 Listeners de botones
        if (selectButton != null)
            selectButton.onClick.AddListener(() => selector.SeleccionarRuletaDesdeBoton(this));

        if (spinButton != null)
        {
            spinButton.onClick.AddListener(() => selector.SpinRuleta(linkedWheel));
            spinButton.interactable = false;
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(() => selector.ConfirmarRuleta(linkedWheel));
            confirmButton.interactable = false;
        }

        // 🔁 Actualizar texto de Spin después de girar
        if (linkedWheel != null)
            linkedWheel.AddSpinEndListener((_) => ActualizarTextoSpin());

        ActualizarTextoSpin();
    }

    public void Activar(bool estado)
    {
        if (ruletaCanvasGroup != null)
        {
            ruletaCanvasGroup.alpha = estado ? 1f : 0.2f;
            ruletaCanvasGroup.interactable = estado;
            ruletaCanvasGroup.blocksRaycasts = estado;
        }

        // Habilitar 'Spin' solo si quedan usos
        if (spinButton != null)
        {
            bool quedanUsos = linkedWheel != null && linkedWheel.UsosRestantes > 0;
            spinButton.interactable = estado && quedanUsos;
        }

        // Habilitar 'Confirm' solo si ya se giró
        if (confirmButton != null)
        {
            bool yaGiro = linkedWheel != null && linkedWheel.UsosRestantes < linkedWheel.UsosMaximos;
            // Solo se puede confirmar si está activo, ya giró, y NO está girando
            confirmButton.interactable = estado && yaGiro && !linkedWheel.IsSpinning;
        }
    }

    public void ActualizarTextoSpin()
    {
        if (linkedWheel != null && spinButtonText != null)
        {
            // ✅ 4. MODIFICADO: Usar UsosMaximos (requiere el Paso 2)
            spinButtonText.text = $"Spin ({linkedWheel.UsosRestantes}/{linkedWheel.UsosMaximos})";
        }
        else if (spinButtonText == null && spinButton != null)
        {
            Debug.LogError($"❌ No se encontró el componente 'TextMeshProUGUI' dentro del prefab '{spinButton.name}'. Verifica el prefab.");
        }
    }
}