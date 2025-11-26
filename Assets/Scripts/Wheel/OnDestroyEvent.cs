using UnityEngine;
using UnityEngine.Events;
using System;

public class OnDestroyEvent : MonoBehaviour
{
    [Header("Evento que se llama cuando este objeto es destruido")]
    public UnityEvent onDestroyed;

    [Header("¿Este objeto notifica a la ruleta?")]
    public bool notificarRuleta = true;

    // Evento global: se dispara cada vez que se destruye
    public static event Action<OnDestroyEvent> OnAnyDestroyed;

    private void OnDestroy()
    {
        if (!Application.isPlaying)
            return;

        // Evento local (lo que configures en el inspector, si querés)
        onDestroyed?.Invoke();

        // Evento global para los sistemas externos
        OnAnyDestroyed?.Invoke(this);
    }
}
