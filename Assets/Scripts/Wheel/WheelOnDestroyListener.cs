using UnityEngine;
using EasyUI.PickerWheelUI;

public class WheelOnDestroyListener : MonoBehaviour
{
    [SerializeField] private WheelUIController wheel;

    private void Awake()
    {
        // Por si te olvidás de asignarlo a mano
        if (wheel == null)
            wheel = GetComponent<WheelUIController>();
    }

    private void OnEnable()
    {
        OnDestroyEvent.OnAnyDestroyed += OnAnyDestroyed;
    }

    private void OnDisable()
    {
        OnDestroyEvent.OnAnyDestroyed -= OnAnyDestroyed;
    }

    private void OnAnyDestroyed(OnDestroyEvent evt)
    {
        // Solo algunos prefabs disparan la ruleta
        if (!evt.notificarRuleta)
            return;

        // Usás la lógica que ya tenés para mostrar la ruleta sólo si no hay enemigos
        wheel.VerificarYMostrarSiNoHayEnemigos();
    }
}
