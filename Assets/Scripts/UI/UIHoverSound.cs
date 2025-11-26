using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIHoverSound : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;        // Asigná el AudioSource del botón
    [SerializeField] private AudioClip hoverClip;            // Sonido al pasar el mouse
    [SerializeField] private AudioClip clickClip;            // Sonido al hacer click
    [Range(0f, 1f)][SerializeField] private float volume = 1f;

    [Header("Opcional: Grupo del Mixer")]
    [SerializeField] private AudioMixerGroup mixerGroup;     // ← Podés asignar el grupo aquí

    private Button button;

    private void Reset()
    {
        button = GetComponent<Button>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(PlayClick);

        // Si no hay AudioSource, creamos uno
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Si hay un grupo asignado, lo aplicamos
        if (mixerGroup != null)
            audioSource.outputAudioMixerGroup = mixerGroup;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayHover();
    }

    public void ISelectHandler_OnSelect()
    {
        PlayHover();
    }

    void ISelectHandler.OnSelect(BaseEventData eventData) => ISelectHandler_OnSelect();

    private void PlayHover()
    {
        if (hoverClip == null || audioSource == null) return;

        audioSource.Stop();
        audioSource.PlayOneShot(hoverClip, volume);
    }

    private void PlayClick()
    {
        if (clickClip == null || audioSource == null) return;
        audioSource.PlayOneShot(clickClip, volume);
    }
}
