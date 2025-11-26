using UnityEngine;
using UnityEngine.Events;
using EasyUI.PickerWheelUI;   //  importante para ver WheelUIController

/// <summary>
/// Escucha el evento onWavesComplete del WaveManager
/// y, cuando se completan las waves, le pide a la ruleta
/// que se muestre si no hay enemigos.
/// </summary>
public class WavesCompleteWheelTrigger : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private WaveManager waveManager;          // Gestor de oleadas
    [SerializeField] private WheelUIController wheelUI;        // Controlador de la ruleta

    private void Reset()
    {
        // Si el script está en el mismo GameObject que el WaveManager,
        // se autocompleta solo.
        if (waveManager == null)
            waveManager = GetComponent<WaveManager>();
    }

    private void OnEnable()
    {
        if (waveManager != null)
        {
            // Nos suscribimos al UnityEvent del WaveManager
            waveManager.onWavesComplete.AddListener(OnWavesComplete);
        }
        else
        {
            Debug.LogWarning("[WavesCompleteWheelTrigger] Falta referencia a WaveManager.", this);
        }
    }

    private void OnDisable()
    {
        if (waveManager != null)
        {
            // Nos desuscribimos para evitar referencias colgando
            waveManager.onWavesComplete.RemoveListener(OnWavesComplete);
        }
    }

    /// <summary>
    /// Método que se ejecuta cuando WaveManager dispara onWavesComplete.
    /// </summary>
    private void OnWavesComplete()
    {
        if (wheelUI == null)
        {
            Debug.LogWarning("[WavesCompleteWheelTrigger] Falta referencia a WheelUIController.", this);
            return;
        }

        // Es EXACTAMENTE lo que hace tu amigo:
        // llamar a VerificarYMostrarSiNoHayEnemigos() cuando se terminan las waves.
        wheelUI.VerificarYMostrarSiNoHayEnemigos();
    }

    // Opcional: para probar desde el Inspector (botoncito con UnityEvent)
    public void TestTrigger()
    {
        OnWavesComplete();
    }
}
