using UnityEngine;
using System.Collections.Generic;
using TMPro;
using EasyUI.PickerWheelUI;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class WeightedRuleta
{
    public GameObject prefab;
    [Range(0f, 100f)] public float weight = 1f;
}

public class WheelSelector : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private List<WeightedRuleta> ruletaWeightedPool;
    [SerializeField] private Transform ruletaContainer;
    [SerializeField] private List<RuletaUISet> ruletaUISets;
    [SerializeField] private WheelUIController wheelUIController;

    [Header("Extras")]
    [SerializeField] private TextMeshProUGUI selectedLabel;
    [SerializeField] private GameObject confettiPrefab;

    private List<PickerWheel> ruletasInstanciadas = new List<PickerWheel>();
    private PickerWheel ruletaSeleccionada;

    // 🔒 BANDERA DE BLOQUEO
    private bool seleccionRealizada = false;

    public void IniciarSelector()
    {
        InstanciarRuletasAleatorias();
    }

    public void InstanciarRuletasAleatorias()
    {
        // Limpieza
        foreach (Transform child in ruletaContainer) Destroy(child.gameObject);
        ruletasInstanciadas.Clear();
        ruletaSeleccionada = null;
        seleccionRealizada = false; // 🔓 Reseteamos el bloqueo al iniciar

        foreach (var set in ruletaUISets) if (set != null) set.Limpiar();

        if (selectedLabel != null) selectedLabel.text = "Seleccione una ruleta...";

        // Generación (Código igual al anterior)
        List<WeightedRuleta> poolTemp = new List<WeightedRuleta>(ruletaWeightedPool);

        for (int i = 0; i < ruletaUISets.Count; i++)
        {
            if (poolTemp.Count == 0) break;

            float totalWeight = 0f;
            foreach (var item in poolTemp) totalWeight += item.weight;
            float r = Random.Range(0f, totalWeight);
            float acc = 0f;
            WeightedRuleta chosen = null;

            foreach (var item in poolTemp) { acc += item.weight; if (r <= acc) { chosen = item; break; } }

            if (chosen != null)
            {
                GameObject obj = Instantiate(chosen.prefab, ruletaContainer);
                PickerWheel pw = obj.GetComponent<PickerWheel>();

                if (pw != null)
                {
                    ruletasInstanciadas.Add(pw);

                    // Botón invisible en la ruleta 3D/UI
                    Button btn = obj.GetComponentInChildren<Button>();
                    if (btn != null)
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => SeleccionarRuleta(pw));
                    }

                    // Inicializar UI Set
                    if (ruletaUISets[i] != null)
                    {
                        ruletaUISets[i].Inicializar(pw, this);
                        // Estado inicial: Todos muestran "Seleccionar"
                        ruletaUISets[i].MostrarEstadoSeleccion();
                    }
                }
                poolTemp.Remove(chosen);
            }
        }
    }

    public void SeleccionarRuleta(PickerWheel wheel)
    {
        if (wheel == null) return;

        // 🔒 SI YA ELEGIMOS UNA, NO HACEMOS NADA
        if (seleccionRealizada) return;

        seleccionRealizada = true; // 🔒 Bloqueamos para siempre
        ruletaSeleccionada = wheel;

        if (selectedLabel != null) selectedLabel.text = "¡Suerte!";

        // Actualizar UI Sets
        foreach (var set in ruletaUISets)
        {
            if (set.linkedWheel == wheel)
            {
                // A la elegida: La activamos (Muestra Spin)
                set.ActivarModoJuego();
            }
            else
            {
                // A las descartadas: Las apagamos totalmente
                set.DesactivarTotalmente();
            }
        }
    }

    public void SpinRuleta(PickerWheel wheel = null)
    {
        // Solo permitimos girar si es la seleccionada
        if (wheel != null && wheel != ruletaSeleccionada) return;

        if (ruletaSeleccionada != null)
        {
            if (!ruletaSeleccionada.IsSpinning)
            {
                ruletaSeleccionada.OnSpinEnd -= OnRuletaTermino;
                ruletaSeleccionada.OnSpinEnd += OnRuletaTermino;
                ruletaSeleccionada.Spin();
            }
            else
            {
                ruletaSeleccionada.Spin(); // Skip
            }
        }
    }

    private void OnRuletaTermino(WheelPiece pieza)
    {
        if (ruletaSeleccionada != null)
        {
            ruletaSeleccionada.OnSpinEnd -= OnRuletaTermino;
            ruletaSeleccionada.MostrarPopupUltimoPremio();
        }
    }

    public void ConfirmarRuleta(PickerWheel wheel = null)
    {
        PickerWheel target = wheel != null ? wheel : ruletaSeleccionada;
        if (target != null)
        {
            target.AplicarUltimoPremio();
            if (confettiPrefab != null)
            {
                confettiPrefab.SetActive(true);
                StartCoroutine(DesactivarConfetti(confettiPrefab, 2f));
            }
            if (wheelUIController != null) wheelUIController.ConfirmarPremio();
        }
    }

    private IEnumerator DesactivarConfetti(GameObject confetti, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        confetti.SetActive(false);
    }
}