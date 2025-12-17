using UnityEngine;

public class DiabloAnimEventsForwarder : MonoBehaviour
{
    [SerializeField] private DiabloController controller;

    private void Awake()
    {
        if (!controller) controller = GetComponentInParent<DiabloController>();
    }

    // Nombres iguales a los Animation Events de tus clips:
    public void OnAnimEnd() => controller?.OnAnimEnd();
    public void OnAttackEnd() => controller?.OnAttackEnd();
    public void OnDeathAnimFinished() => controller?.OnDeathAnimFinished();
}
