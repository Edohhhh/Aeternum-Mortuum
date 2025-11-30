using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class NPCQuest : MonoBehaviour
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

    [Header("Misión / Quest")]
    [Tooltip("Los items que el jugador DEBE tener para completar la misión.")]
    public List<PowerUp> requiredItems; // Los scriptables que se comparan y eliminan

    [Tooltip("Lista de posibles premios. Se elegirá uno al azar.")]
    public List<PowerUp> rewardPool; // "Premios"

    [Header("Mensajes de Estado")]
    [TextArea] public string successMessage = "¡Gracias! Aquí tienes tu recompensa.";
    [TextArea] public string missingItemsMessage = "Aún no tienes lo que te pedí...";
    [TextArea] public string inventoryFullMessage = "No tienes espacio para la recompensa.";

    // ---- Estado interno ----
    private bool playerInRange;
    private bool isDialogueActive;
    private bool isTyping;
    private int dialogueIndex;

    // Control de flujo de mensajes finales
    private bool showedResultMessage;
    private bool questCompletedSuccess;

    private SpriteRenderer runtimeIconRenderer;
    private static bool anyDialogueBusy = false;
    private bool isQuestLocked = false;

    // Bloqueo temporal hasta salir del trigger si falla
    private bool blockedUntilExit = false;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    private void Awake()
    {
        // Configuración del icono flotante
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

    private void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(interactKey))
        {
            // 1. Iniciar diálogo
            if (!isDialogueActive &&
                !anyDialogueBusy &&
                !isQuestLocked &&
                !blockedUntilExit)
            {
                StartDialogue();
                return;
            }

            // 2. Completar tipeo instantáneo
            if (isDialogueActive && isTyping)
            {
                StopAllCoroutines();
                dialogueText.SetText(GetCurrentLineForDisplay());
                isTyping = false;
                return;
            }

            // 3. Lógica al final del diálogo
            if (isDialogueActive && !isTyping && IsLastLine())
            {
                // Si ya mostramos el resultado (éxito o fallo), cerramos
                if (showedResultMessage)
                {
                    if (questCompletedSuccess)
                    {
                        isQuestLocked = true; // Bloquear NPC tras éxito (opcional)
                        CloseAndReset();
                    }
                    else
                    {
                        blockedUntilExit = true; // Bloquear hasta salir y volver a entrar
                        CloseAndReset();
                    }
                }
                else
                {
                    // Es el momento de verificar la misión
                    AttemptCompleteQuest();
                }
                return;
            }

            // 4. Avanzar diálogo normal
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
                !isQuestLocked &&
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
            blockedUntilExit = false; // Resetear bloqueo al salir
            CloseAndReset();
        }
    }

    // ---------- Lógica de Quest ----------

    private void AttemptCompleteQuest()
    {
        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;

        var pc = player.GetComponent<PlayerController>();
        if (pc == null) return;

        // 1. Verificar si tiene los items requeridos
        if (CheckIfPlayerHasItems(pc))
        {
            // 2. Intentar dar recompensa y quitar items
            if (ProcessExchange(pc))
            {
                questCompletedSuccess = true;
                showedResultMessage = true;
                StartTypingNewLine(successMessage, GetTypingSpeed());
            }
            else
            {
                // Fallo raro (ej. inventario lleno, aunque aquí asumimos intercambio directo)
                questCompletedSuccess = false;
                showedResultMessage = true;
                StartTypingNewLine(inventoryFullMessage, GetTypingSpeed());
            }
        }
        else
        {
            // 3. No tiene los items
            questCompletedSuccess = false;
            showedResultMessage = true;
            StartTypingNewLine(missingItemsMessage, GetTypingSpeed());
        }
    }

    private bool CheckIfPlayerHasItems(PlayerController pc)
    {
        if (requiredItems == null || requiredItems.Count == 0) return true; // Si no pide nada, pasa.

        // Convertimos el inventario del jugador a una lista temporal para simular la búsqueda
        var playerInventory = pc.initialPowerUps != null
            ? pc.initialPowerUps.Where(p => p != null).ToList()
            : new List<PowerUp>();

        foreach (var reqItem in requiredItems)
        {
            if (reqItem == null) continue;

            // Buscamos si el inventario tiene este item específico
            if (playerInventory.Contains(reqItem))
            {
                // Lo removemos de la lista temporal para manejar cantidades (ej. si pide 2 pociones iguales)
                playerInventory.Remove(reqItem);
            }
            else
            {
                return false; // Falta al menos un item
            }
        }
        return true;
    }

    private bool ProcessExchange(PlayerController pc)
    {
        // --- 1. Remover Items del Jugador ---
        var currentInventory = pc.initialPowerUps.ToList();

        foreach (var reqItem in requiredItems)
        {
            if (reqItem == null) continue;
            // Buscamos y removemos la primera instancia encontrada
            currentInventory.Remove(reqItem);

            // Opcional: Notificar al GameDataManager si es necesario remover perks activos
            var gdm = GameDataManager.Instance;
            if (gdm != null) gdm.RemovePerk(pc, reqItem);
        }

        // --- 2. Seleccionar y Añadir Premio ---
        if (rewardPool != null && rewardPool.Count > 0)
        {
            PowerUp premio = rewardPool[Random.Range(0, rewardPool.Count)];
            if (premio != null)
            {
                currentInventory.Add(premio);
                // Lógica visual o de datos extra si el premio se activa al instante:
                // pc.AddPerk(premio); // Si tienes un método así en PlayerController
            }
        }

        // --- 3. Guardar cambios ---
        pc.initialPowerUps = currentInventory.ToArray();

        try
        {
            GameDataManager.Instance.SavePlayerData(pc);
        }
        catch { }

        return true;
    }

    // ---------- Sistema de Diálogo (Idéntico a WheelShrine) ----------
    private void StartDialogue()
    {
        if (dialogueData == null || dialoguePanel == null || dialogueText == null)
        {
            Debug.LogError("NPCQuest: Faltan referencias de diálogo en el Inspector.");
            return;
        }

        StopAllCoroutines();
        anyDialogueBusy = true;
        isDialogueActive = true;
        isTyping = false;
        dialogueIndex = 0;
        showedResultMessage = false;
        questCompletedSuccess = false;

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
        {
            StartTypingNewLine(GetCurrentLine(), GetTypingSpeed());
        }
        else
        {
            // Esto solo ocurre si el diálogo normal termina y no hay lógica de quest pendiente
            // (Usualmente controlado por IsLastLine en Update)
            CloseAndReset();
        }
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

    private void CloseAndReset()
    {
        StopAllCoroutines();
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (dialogueText) dialogueText.text = "";

        isDialogueActive = false;
        isTyping = false;
        anyDialogueBusy = false;
        dialogueIndex = 0;
        showedResultMessage = false;

        if (runtimeIconRenderer)
            runtimeIconRenderer.enabled =
                playerInRange &&
                !isDialogueActive &&
                !isQuestLocked &&
                !blockedUntilExit;
    }

    // ---------- Helpers ----------
    private string GetCurrentLine() =>
        dialogueData != null &&
        dialogueData.dialogueLines != null &&
        dialogueData.dialogueLines.Length > 0 ?
        dialogueData.dialogueLines[Mathf.Clamp(dialogueIndex, 0, SafeLinesCount() - 1)] : "";

    private string GetCurrentLineForDisplay()
    {
        if (showedResultMessage)
        {
            return questCompletedSuccess ? successMessage : missingItemsMessage;
        }
        return GetCurrentLine();
    }

    private int SafeLinesCount() =>
        (dialogueData != null && dialogueData.dialogueLines != null)
        ? dialogueData.dialogueLines.Length : 0;

    private float GetTypingSpeed() =>
        (dialogueData != null && dialogueData.typingSpeed > 0f)
        ? dialogueData.typingSpeed : Mathf.Max(0.001f, fallbackTypingSpeed);

    private bool IsLastLine() => dialogueIndex >= SafeLinesCount() - 1;
}