using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class ConditionalPortal : MonoBehaviour
{
    [Header("Escena destino")]
    public string targetScene;

    [Header("Condición")]
    public int requiredDeaths = 3;

    [Header("Diálogo (mismo sistema que NPC)")]
    public NPCDialogue blockedDialogue;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;

    [Header("Interacción")]
    public KeyCode interactKey = KeyCode.F;
    public Sprite interactIcon;
    public float iconOffsetY = 2f;
    public string playerTag = "Player";
    public string iconSortingLayer = "UI";
    public int iconSortingOrder = 50;

    private bool playerInRange;
    private bool isDialogueActive;
    private bool isTyping;

    private int dialogueIndex;
    private SpriteRenderer iconRenderer;
    private UIDialogueAnimator dialogueAnimator;

    void Start()
    {
        // Icono flotante
        var iconGO = new GameObject("PortalInteractIcon");
        iconGO.transform.SetParent(transform);
        iconGO.transform.localPosition = new Vector3(0f, iconOffsetY, 0f);

        iconRenderer = iconGO.AddComponent<SpriteRenderer>();
        iconRenderer.sprite = interactIcon;
        iconRenderer.sortingLayerName = iconSortingLayer;
        iconRenderer.sortingOrder = iconSortingOrder;
        iconRenderer.enabled = false;

        if (dialoguePanel)
        {
            dialoguePanel.SetActive(false);
            dialogueAnimator = dialoguePanel.GetComponent<UIDialogueAnimator>();
        }
    }

    void Update()
    {
        if (iconRenderer != null)
        {
            var pos = transform.position;
            pos.y += iconOffsetY;
            iconRenderer.transform.position = pos;
        }

        if (playerInRange && Input.GetKeyDown(interactKey))
            Interact();
    }

    void Interact()
    {
        if (isDialogueActive)
        {
            NextLine();
            return;
        }

        if (CanUsePortal())
        {
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            StartDialogue();
        }
    }

    bool CanUsePortal()
    {
        if (RunStatsManager.Instance == null) return false;
        return RunStatsManager.Instance.deaths >= requiredDeaths;
    }

    void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        if (nameText) nameText.SetText(blockedDialogue.npcName);
        if (portraitImage) portraitImage.sprite = blockedDialogue.npcPortrait;

        dialoguePanel.SetActive(true);
        dialogueAnimator?.AnimateIn();

        iconRenderer.enabled = false;

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(blockedDialogue.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if (++dialogueIndex < blockedDialogue.dialogueLines.Length)
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

        foreach (char c in blockedDialogue.dialogueLines[dialogueIndex])
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(blockedDialogue.typingSpeed);
        }

        isTyping = false;
    }

    void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueText.SetText("");

        dialogueAnimator?.AnimateOut(() =>
        {
            dialoguePanel.SetActive(false);
            iconRenderer.enabled = playerInRange;
        });
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = true;
        iconRenderer.enabled = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInRange = false;
        iconRenderer.enabled = false;

        if (isDialogueActive)
            EndDialogue();
    }
}
