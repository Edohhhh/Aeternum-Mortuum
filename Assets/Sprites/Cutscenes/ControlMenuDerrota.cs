using UnityEngine;
using UnityEngine.UI;

public class ControlMenuDerrota : MonoBehaviour
{
    public CanvasGroup menuCanvasGroup;

    public void ActivarInteraccion()
    {
        menuCanvasGroup.interactable = true;
        menuCanvasGroup.blocksRaycasts = true;
    }
}