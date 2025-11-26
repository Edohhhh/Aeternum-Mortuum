using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;
using System.Collections.Generic;
using DG.Tweening;

[DefaultExecutionOrder(200)]
public class PlayerStatsHUD : MonoBehaviour
{
    [Header("Mostrar/Ocultar")]
    // ✅ CAMBIO AQUÍ: Configuración para abrir/cerrar con un toque
    [SerializeField] private bool holdToShow = false;
    [SerializeField] private bool toggleMode = true;
    [SerializeField] private KeyCode key = KeyCode.Tab;

    [Header("Panel contenedor")]
    [SerializeField] private GameObject panel;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panelRectTransform;

    [Header("Animación")]
    [SerializeField] private float slideDuration = 0.3f;
    [SerializeField] private Vector2 slideOffset = new Vector2(0, -50);

    [Header("Perks (Inventario)")]
    [SerializeField] private Transform perksContainer;
    [SerializeField] private GameObject perkIconPrefab;
    [SerializeField] private InventoryTooltipUI tooltipUI;

    [Header("Filas (Texto de Stats)")]
    [SerializeField] private TMP_Text speedTxt;
    [SerializeField] private TMP_Text dashSpeedTxt;
    [SerializeField] private TMP_Text dashIframesTxt;
    [SerializeField] private TMP_Text dashSlideDurTxt;
    [SerializeField] private TMP_Text dashDurTxt;
    [SerializeField] private TMP_Text dashCooldownTxt;
    [SerializeField] private TMP_Text baseDamageTxt;
    [SerializeField] private TMP_Text attackCooldownTxt;
    [SerializeField] private TMP_Text recoilDistanceTxt;
    [SerializeField] private TMP_Text attackCooldownRemainingTxt;
    [SerializeField] private TMP_Text regenRateTxt;
    [SerializeField] private TMP_Text regenDelayTxt;

    private PlayerController player;
    private PlayerHealth health;
    private CombatSystem combat;
    private FieldInfo attackCooldownTimerField;

    // Control interno de visibilidad
    private bool isVisible = false;

    private void Start()
    {
        // Buscar referencias globales
        player = FindObjectOfType<PlayerController>();
        health = FindObjectOfType<PlayerHealth>();
        combat = FindObjectOfType<CombatSystem>();

        if (combat != null)
        {
            attackCooldownTimerField = typeof(CombatSystem).GetField("attackCooldownTimer", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        // Estado inicial: Oculto
        isVisible = false;
        if (panel != null) panel.SetActive(false);
        if (canvasGroup != null) canvasGroup.alpha = 0;
    }

    private void Update()
    {
        HandleInput();

        // ✅ NUEVO: Forzar actualización con la tecla 'P'
        if (Input.GetKeyDown(KeyCode.P))
        {
            ActualizarInventarioUI();
            Debug.Log("🔄 Inventario actualizado manualmente con la tecla P.");
        }

        // Solo actualizamos los textos si el panel se ve, para ahorrar rendimiento
        if (isVisible)
        {
            UpdateStatsValues();
        }
    }

    // Método público para actualizar inventario externamente
    public void ActualizarInventarioUI()
    {
        UpdatePerksUI();
    }

    private void HandleInput()
    {
        // Lógica de Toggle (Interruptor)
        if (toggleMode)
        {
            if (Input.GetKeyDown(key))
            {
                TogglePanel();
            }
        }
        // Lógica de Hold (Mantener apretado - por si quieres volver a usarla en el futuro)
        else
        {
            if (Input.GetKeyDown(key)) ShowPanel();
            if (Input.GetKeyUp(key)) HidePanel();
        }
    }

    private void TogglePanel()
    {
        if (isVisible)
            HidePanel();
        else
            ShowPanel();
    }

    private void ShowPanel()
    {
        if (isVisible) return; // Evitar llamadas dobles

        isVisible = true;
        if (panel != null) panel.SetActive(true);

        // Actualizar UI de Inventario al abrir
        UpdatePerksUI();

        // Animación de entrada
        canvasGroup.DOFade(1f, slideDuration).SetUpdate(true);
        panelRectTransform.DOAnchorPos(Vector2.zero, slideDuration).SetUpdate(true);
    }

    private void HidePanel()
    {
        if (!isVisible) return;

        isVisible = false;

        // Animación de salida
        canvasGroup.DOFade(0f, slideDuration).SetUpdate(true).OnComplete(() =>
        {
            if (panel != null && !isVisible)
                panel.SetActive(false);
        });
        panelRectTransform.DOAnchorPos(slideOffset, slideDuration).SetUpdate(true);
    }

    private void UpdatePerksUI()
    {
        if (perksContainer == null || perkIconPrefab == null || player == null) return;

        // 1. Limpiar iconos viejos
        foreach (Transform child in perksContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Crear iconos nuevos
        if (player.initialPowerUps != null)
        {
            foreach (PowerUp powerUp in player.initialPowerUps)
            {
                if (powerUp != null && powerUp.effect != null)
                {
                    GameObject iconObj = Instantiate(perkIconPrefab, perksContainer);
                    PerkIconUI iconScript = iconObj.GetComponent<PerkIconUI>();
                    if (iconScript != null)
                    {
                        iconScript.Initialize(powerUp.effect, tooltipUI);
                    }
                }
            }
        }
    }

    private void UpdateStatsValues()
    {
        // --- Stats de PlayerHealth ---
        if (health != null)
        {
            SetText(regenRateTxt, health.regenerationRate);
            SetText(regenDelayTxt, health.regenDelay);
        }

        // --- Stats de PlayerController ---
        if (player != null)
        {
            SetText(speedTxt, player.moveSpeed);
            SetText(dashSpeedTxt, player.dashSpeed);
            SetText(dashIframesTxt, player.dashIframes);
            SetText(dashSlideDurTxt, player.dashSlideDuration);
            SetText(dashDurTxt, player.dashDuration);
            SetText(dashCooldownTxt, player.dashCooldown);
            SetText(baseDamageTxt, player.baseDamage);
        }

        // --- Stats de CombatSystem ---
        if (combat != null)
        {
            SetText(attackCooldownTxt, combat.attackCooldown);
            SetText(recoilDistanceTxt, combat.recoilDistance);

            if (attackCooldownRemainingTxt != null && attackCooldownTimerField != null)
            {
                var val = attackCooldownTimerField.GetValue(combat);
                float remaining = (val is float f) ? Mathf.Max(0f, f) : 0f;
                attackCooldownRemainingTxt.text = $"{remaining:0.00}";
            }
        }
    }

    private static void SetText(TMP_Text txt, float value)
    {
        if (txt != null) txt.text = value.ToString("0.00");
    }
}