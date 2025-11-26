using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    [Header("Objeto a destruir")]
    public GameObject targetObject;

    [Header("Tiempo antes de destruir")]
    public float delay = 3f;

    private void Start()
    {
        if (targetObject != null)
        {
            Destroy(targetObject, delay);
        }
        else
        {
            Debug.LogWarning("No se asignó ningún objeto en DestroyAfterTime.");
        }
    }
}


