using UnityEngine;
using UnityEngine.UI;
using EasyUI.PickerWheelUI;
using TMPro;

public class RuletaUISet : MonoBehaviour
{
    [Header("Contenedor")]
    public Transform buttonsContainer;

    [HideInInspector] public PickerWheel linkedWheel;

    private Button selectButton;
    private Button spinButton;
    private Button confirmButton;
    private TextMeshProUGUI spinButtonText;

    private CanvasGroup ruletaCanvasGroup;
    private WheelSelector mainSelector;

    public void Limpiar()
    {
        if (buttonsContainer != null) foreach (Transform child in buttonsContainer) Destroy(child.gameObject);
        linkedWheel = null;
        selectButton = null;
        spinButton = null;
        confirmButton = null;
    }

    public void Inicializar(PickerWheel wheel, WheelSelector selector)
    {
        Limpiar();
        linkedWheel = wheel;
        mainSelector = selector;

        ruletaCanvasGroup = linkedWheel.GetComponent<CanvasGroup>();
        if (ruletaCanvasGroup == null) ruletaCanvasGroup = linkedWheel.gameObject.AddComponent<CanvasGroup>();

        var theme = linkedWheel.GetComponent<RuletaTheme>();
        if (theme != null)
        {
            // Instanciar Select
            if (theme.selectButtonPrefab != null)
            {
                GameObject obj = Instantiate(theme.selectButtonPrefab, buttonsContainer);
                selectButton = obj.GetComponent<Button>();
                if (selectButton == null) selectButton = obj.GetComponentInChildren<Button>();
                selectButton.onClick.AddListener(() => mainSelector.SeleccionarRuleta(linkedWheel));
            }
            // Instanciar Spin
            if (theme.spinButtonPrefab != null)
            {
                GameObject obj = Instantiate(theme.spinButtonPrefab, buttonsContainer);
                spinButton = obj.GetComponent<Button>();
                if (spinButton == null) spinButton = obj.GetComponentInChildren<Button>();
                spinButtonText = obj.GetComponentInChildren<TextMeshProUGUI>();
                spinButton.onClick.AddListener(() => mainSelector.SpinRuleta(linkedWheel));
            }
            // Instanciar Confirm
            if (theme.confirmButtonPrefab != null)
            {
                GameObject obj = Instantiate(theme.confirmButtonPrefab, buttonsContainer);
                confirmButton = obj.GetComponent<Button>();
                if (confirmButton == null) confirmButton = obj.GetComponentInChildren<Button>();
                confirmButton.onClick.AddListener(() => mainSelector.ConfirmarRuleta(linkedWheel));
            }
        }
    }

    // --- ESTADO 1: AL INICIAR (Todas esperando selección) ---
    public void MostrarEstadoSeleccion()
    {
        if (ruletaCanvasGroup) ruletaCanvasGroup.alpha = 1f;
        if (selectButton) selectButton.gameObject.SetActive(true);
        if (spinButton) spinButton.gameObject.SetActive(false);
        if (confirmButton) confirmButton.gameObject.SetActive(false);
    }

    // --- ESTADO 2: ELEGIDA (Esta es la que vas a jugar) ---
    public void ActivarModoJuego()
    {
        if (ruletaCanvasGroup) ruletaCanvasGroup.alpha = 1f;
        if (selectButton) selectButton.gameObject.SetActive(false); // Adios select
        if (spinButton) spinButton.gameObject.SetActive(true);     // Hola Spin
        // Confirm se gestiona en Update según si terminó de girar
    }

    // --- ESTADO 3: DESCARTADA (Las otras ruletas se apagan) ---
    public void DesactivarTotalmente()
    {
        // Se ven muy transparentes para indicar que no son accesibles
        if (ruletaCanvasGroup) ruletaCanvasGroup.alpha = 0.2f;

        // Desactivamos TODOS los botones
        if (selectButton) selectButton.gameObject.SetActive(false);
        if (spinButton) spinButton.gameObject.SetActive(false);
        if (confirmButton) confirmButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (linkedWheel == null) return;

        // Solo ejecutamos lógica si el botón Spin está activo (significa que es la elegida)
        if (spinButton != null && spinButton.gameObject.activeSelf)
        {
            bool isSpinning = linkedWheel.IsSpinning;
            bool hasUses = linkedWheel.UsosRestantes > 0;
            bool finished = linkedWheel.UsosRestantes < linkedWheel.UsosMaximos && !isSpinning;

            spinButton.interactable = hasUses || isSpinning;

            if (spinButtonText != null)
            {
                spinButtonText.text = isSpinning ? "Skip >>" : (hasUses ? "Spin" : "Out Of Stock");
            }

            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(finished); // Solo aparece al final
                confirmButton.interactable = finished;
            }
        }
    }
}