using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPC : MonoBehaviour
{
    [Header("Datos de diálogo")]
    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;

    [Header("Interacción")]
    public KeyCode interactKey = KeyCode.F;
    public Sprite interactIcon;
    public SpriteRenderer interactRendererInScene;
    public float spriteOffsetY = 2f;
    public string playerTag = "Player";
    public string iconSortingLayer = "UI";
    public int iconSortingOrder = 50;

    // ===== IMPORTANTE =====
    [Header("NPC Importante")]
    public bool isImportantNPC = false;     // Se tilda desde inspector
    public Sprite importantIcon;            // Sprite del "!"
    public float importantIconOffsetY = 2.7f;
    private SpriteRenderer importantIconRenderer;
    private bool hasSpokenBefore = false;   // Para ocultar el "!" después de hablar

    private static NPC currentActiveNPC = null;

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;
    private bool playerInRange;

    private SpriteRenderer runtimeIconRenderer;
    private UIDialogueAnimator dialogueAnimator;


    [Header("Distancia para mantener el diálogo")]
    public float maxDialogueDistance = 3.5f;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag(playerTag)?.transform;

        if (dialoguePanel)
        {
            dialoguePanel.SetActive(false);
            dialogueAnimator = dialoguePanel.GetComponent<UIDialogueAnimator>();
        }

        // ========================
        //   ICONO DE INTERACCIÓN F
        // ========================
        SpriteRenderer target = interactRendererInScene;
        if (target == null)
        {
            var go = new GameObject("InteractIcon");
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0f, spriteOffsetY, 0f);
            target = go.AddComponent<SpriteRenderer>();
            runtimeIconRenderer = target;
        }

        if (interactIcon != null) target.sprite = interactIcon;
        target.sortingLayerName = iconSortingLayer;
        target.sortingOrder = iconSortingOrder;
        target.enabled = false;

        // ========================
        //    ICONO IMPORTANTE (!)
        // ========================
        if (isImportantNPC && importantIcon != null)
        {
            var go = new GameObject("ImportantIcon");
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0f, importantIconOffsetY, 0f); // ← USAMOS LA VARIABLE

            importantIconRenderer = go.AddComponent<SpriteRenderer>();
            importantIconRenderer.sprite = importantIcon;
            importantIconRenderer.sortingLayerName = iconSortingLayer;
            importantIconRenderer.sortingOrder = iconSortingOrder + 5;

            importantIconRenderer.enabled = !hasSpokenBefore;
        }

    }

    void Update()
    {
        // Mantener iconos arriba del NPC
        if (runtimeIconRenderer != null)
        {
            var pos = transform.position;
            pos.y += spriteOffsetY;
            runtimeIconRenderer.transform.position = pos;
        }
        if (importantIconRenderer != null)
        {
            var pos = transform.position;
            pos.y += importantIconOffsetY;   // ← ahora configurable
            importantIconRenderer.transform.position = pos;
        }


        UpdateIconVisibility();

        if (playerInRange && Input.GetKeyDown(interactKey))
            Interact();

        if (isDialogueActive && player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist > maxDialogueDistance)
                EndDialogue();
        }
    }

    public bool CanInteract() => !isDialogueActive && currentActiveNPC == null;

    public void Interact()
    {
        if (currentActiveNPC != null && currentActiveNPC != this) return;

        if (isDialogueActive) NextLine();
        else StartDialogue();
    }

    void StartDialogue()
    {
        if (currentActiveNPC != null && currentActiveNPC != this) return;

        currentActiveNPC = this;
        isDialogueActive = true;
        dialogueIndex = 0;

        if (nameText != null) nameText.SetText(dialogueData.npcName);
        if (portraitImage != null) portraitImage.sprite = dialogueData.npcPortrait;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            dialogueAnimator ??= dialoguePanel.GetComponent<UIDialogueAnimator>();
            dialogueAnimator?.AnimateIn();
        }

        SetIconVisible(false);

        // ===== IMPORTANTE: hablar por 1ra vez → ocultar "!" =====
        if (isImportantNPC && importantIconRenderer != null)
        {
            hasSpokenBefore = true;
            importantIconRenderer.enabled = false;
        }

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping = false;

        if (dialogueData.autoProgressLines.Length > dialogueIndex &&
            dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueText.SetText("");

        if (dialoguePanel != null)
        {
            dialogueAnimator ??= dialoguePanel.GetComponent<UIDialogueAnimator>();
            if (dialogueAnimator != null)
            {
                dialogueAnimator.AnimateOut(() =>
                {
                    dialoguePanel.SetActive(false);
                    if (currentActiveNPC == this) currentActiveNPC = null;
                    UpdateIconVisibility();
                });
                return;
            }
        }

        dialoguePanel.SetActive(false);

        if (currentActiveNPC == this)
            currentActiveNPC = null;

        UpdateIconVisibility();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            UpdateIconVisibility();

            // ===== Importante: ocultar "!" si entra al rango F =====
            if (isImportantNPC && importantIconRenderer != null && !hasSpokenBefore)
                importantIconRenderer.enabled = false;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            UpdateIconVisibility();

            if (isDialogueActive)
                EndDialogue();

            // ===== Volver a mostrar "!" si nunca habló =====
            if (isImportantNPC && importantIconRenderer != null && !hasSpokenBefore)
                importantIconRenderer.enabled = true;
        }
    }

    void UpdateIconVisibility()
    {
        bool visible = playerInRange && !isDialogueActive && currentActiveNPC == null;
        SetIconVisible(visible);
    }

    void SetIconVisible(bool visible)
    {
        if (interactRendererInScene != null) interactRendererInScene.enabled = visible;
        if (runtimeIconRenderer != null) runtimeIconRenderer.enabled = visible;
    }
}