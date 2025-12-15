using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class RunStatsShrine : MonoBehaviour
{
    [Header("Diálogo (estilo NPC)")]
    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text nameText;
    public Image portraitImage;
    public float fallbackTypingSpeed = 0.02f;

    [Header("Interacción")]
    public KeyCode interactKey = KeyCode.F;
    public string playerTag = "Player";
    public Sprite interactIcon;
    public float iconOffsetY = 1.5f;

    [Header("Condición del Run")]
    [Min(0)] public int requiredWins = 1;
    [Min(0)] public int requiredDeaths = 1;

    [TextArea]
    public string notEnoughRunStatsMessage =
        "Aún no has vivido lo suficiente para tentar al azar.";

    [Header("Ruleta")]
    public EasyUI.PickerWheelUI.WheelUIController wheelUIController;

    // ---- Estado interno ----
    private bool playerInRange;
    private bool isDialogueActive;
    private bool isTyping;
    private int dialogueIndex;
    private bool showedInsufficientLine;
    private SpriteRenderer runtimeIconRenderer;

    // Candado global para que no hablen 2 NPC/altares al mismo tiempo
    private static bool anyDialogueBusy = false;

    // Bloqueo hasta salir del trigger
    private bool blockedUntilExit = false;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    private void Awake()
    {
        if (interactIcon != null)
        {
            var go = new GameObject("InteractIcon");
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0f, iconOffsetY, 0f);
            runtimeIconRenderer = go.AddComponent<SpriteRenderer>();
            runtimeIconRenderer.sprite = interactIcon;
            runtimeIconRenderer.sortingOrder = 50;
            runtimeIconRenderer.enabled = false;
        }

        if (dialoguePanel) dialoguePanel.SetActive(false);
    }

    private void Start()
    {
        if (wheelUIController == null)
            wheelUIController = FindFirstObjectByType<EasyUI.PickerWheelUI.WheelUIController>();
    }

    private void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(interactKey))
        {
            // Iniciar diálogo
            if (!isDialogueActive &&
                !anyDialogueBusy &&
                !blockedUntilExit)
            {
                StartDialogue();
                return;
            }

            // Completar tipeo
            if (isDialogueActive && isTyping)
            {
                StopAllCoroutines();
                dialogueText.SetText(GetCurrentLineForDisplay());
                isTyping = false;
                return;
            }

            // Última línea
            if (isDialogueActive && !isTyping && IsLastLine())
            {
                if (HasRequiredRunStats())
                {
                    ExecuteRouletteAndClose();
                }
                else
                {
                    if (!showedInsufficientLine)
                    {
                        showedInsufficientLine = true;
                        StartTypingNewLine(notEnoughRunStatsMessage, GetTypingSpeed());
                    }
                    else
                    {
                        blockedUntilExit = true;
                        CloseAndReset();
                    }
                }
                return;
            }

            // Avanzar diálogo
            if (isDialogueActive && !isTyping)
            {
                NextLine();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            if (!isDialogueActive &&
                runtimeIconRenderer &&
                !blockedUntilExit)
            {
                runtimeIconRenderer.enabled = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            blockedUntilExit = false;
            CloseAndReset();
        }
    }

    // ---------- Diálogo ----------
    private void StartDialogue()
    {
        if (dialogueData == null || dialoguePanel == null || dialogueText == null)
        {
            Debug.LogError("RunStatsShrine: Faltan referencias de diálogo.");
            return;
        }

        StopAllCoroutines();
        anyDialogueBusy = true;
        isDialogueActive = true;
        isTyping = false;
        dialogueIndex = 0;
        showedInsufficientLine = false;

        if (nameText) nameText.SetText(dialogueData.npcName);
        if (portraitImage) portraitImage.sprite = dialogueData.npcPortrait;

        dialogueText.text = "";
        dialoguePanel.SetActive(true);
        if (runtimeIconRenderer) runtimeIconRenderer.enabled = false;

        StartTypingNewLine(GetCurrentLine(), GetTypingSpeed());
    }

    private void NextLine()
    {
        dialogueIndex++;
        if (dialogueIndex < SafeLinesCount())
            StartTypingNewLine(GetCurrentLine(), GetTypingSpeed());
        else
            CloseAndReset();
    }

    private void StartTypingNewLine(string line, float speed)
    {
        StopAllCoroutines();
        dialogueText.text = "";
        StartCoroutine(TypeLine(line, speed));
    }

    private IEnumerator TypeLine(string line, float speed)
    {
        isTyping = true;
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(speed);
        }
        isTyping = false;
    }

    // ---------- Ruleta ----------
    private void ExecuteRouletteAndClose()
    {
        if (wheelUIController != null)
            wheelUIController.MostrarRuleta();

        CloseAndReset();
    }

    // ---------- Condición ----------
    private bool HasRequiredRunStats()
    {
        var stats = RunStatsManager.Instance;
        if (stats == null) return false;

        return stats.wins >= requiredWins &&
               stats.deaths >= requiredDeaths;
    }

    private void CloseAndReset()
    {
        StopAllCoroutines();
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (dialogueText) dialogueText.text = "";

        isDialogueActive = false;
        isTyping = false;
        anyDialogueBusy = false;
        dialogueIndex = 0;
        showedInsufficientLine = false;

        if (runtimeIconRenderer)
        {
            runtimeIconRenderer.enabled =
                playerInRange &&
                !isDialogueActive &&
                !blockedUntilExit;
        }
    }

    // ---------- Helpers ----------
    private string GetCurrentLine() =>
        dialogueData.dialogueLines[Mathf.Clamp(dialogueIndex, 0, SafeLinesCount() - 1)];

    private string GetCurrentLineForDisplay() =>
        showedInsufficientLine ? notEnoughRunStatsMessage : GetCurrentLine();

    private int SafeLinesCount() =>
        dialogueData.dialogueLines != null ? dialogueData.dialogueLines.Length : 0;

    private float GetTypingSpeed() =>
        dialogueData.typingSpeed > 0f ? dialogueData.typingSpeed : fallbackTypingSpeed;

    private bool IsLastLine() => dialogueIndex >= SafeLinesCount() - 1;
}
