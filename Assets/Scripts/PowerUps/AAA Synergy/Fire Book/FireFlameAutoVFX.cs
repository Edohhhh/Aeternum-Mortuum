using UnityEngine;

public class FireFlameAutoVFX : MonoBehaviour
{
    [Header("FBX / Modelo de fuego")]
    public GameObject fireVFXModel;

    [Header("Ajustes automáticos")]
    public Vector3 localPosition = Vector3.zero;
    public Vector3 localRotation = Vector3.zero;
    public Vector3 localScale = new Vector3(1f, 1f, 1f);

    [Header("Sorting")]
    public string sortingLayer = "Effects";
    public int sortingOrder = 5;

    private GameObject instance;

    private void Awake()
    {
        if (fireVFXModel == null)
        {
            Debug.LogWarning("[FireFlameAutoVFX] No hay un FBX asignado.");
            return;
        }

        // Crear instancia del modelo
        instance = Instantiate(fireVFXModel, transform);

        // Ajustar transform
        instance.transform.localPosition = localPosition;
        instance.transform.localEulerAngles = localRotation;
        instance.transform.localScale = localScale;

        // Ajustar sorting (si tiene MeshRenderer)
        var mesh = instance.GetComponentInChildren<MeshRenderer>();
        if (mesh != null)
        {
            mesh.sortingLayerName = sortingLayer;
            mesh.sortingOrder = sortingOrder;
        }

        // Ajustar sorting (si tiene ParticleSystemRenderer)
        var ps = instance.GetComponentInChildren<ParticleSystemRenderer>();
        if (ps != null)
        {
            ps.sortingLayerName = sortingLayer;
            ps.sortingOrder = sortingOrder;
        }
    }

    private void OnDisable()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
    }
}
