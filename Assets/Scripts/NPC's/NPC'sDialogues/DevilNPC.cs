using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DevilNPC : MonoBehaviour
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

    [Header("Chequeo de rango (SIN triggers)")]
    public LayerMask playerLayer;
    public Vector2 interactBoxSize = new Vector2(2.2f, 2.0f);
    public Vector2 interactBoxOffset = new Vector2(0f, 0.5f);

    [Header("Condición para poder hablar (ENEMIGO)")]
    public EnemyHealth requiredEnemyHealth;
    public bool requireEnemyDeadToInteract = true;

    [Header("Cambio de escena al terminar diálogo")]
    public List<string> scenesToLoad = new List<string>();
    public bool avoidCurrentScene = true;

    [Header("Fade a negro (OBLIGATORIO para el efecto)")]
    [Tooltip("Image negro fullscreen en un Canvas. Debe arrancar con alpha 0.")]
    public Image fadeImage;
    public float fadeDuration = 0.7f;
    public bool useUnscaledTimeForFade = true;

    private static DevilNPC currentActiveNPC = null;

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

    private SpriteRenderer runtimeIconRenderer;
    private UIDialogueAnimator dialogueAnimator;

    private Transform player;
    private bool playerInRange;

    // NUEVO: cuando true, se corta todo excepto el cambio de escena
    private bool isTransitioning;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag(playerTag)?.transform;

        if (dialoguePanel)
        {
            dialoguePanel.SetActive(false);
            dialogueAnimator = dialoguePanel.GetComponent<UIDialogueAnimator>();
        }

        // Icono
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

        // Fade image: aseguramos alpha 0 al inicio (si existe)
        if (fadeImage != null)
        {
            var c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (isTransitioning) return; // IMPORTANTE: durante la transición no pasa nada más

        // Mantener icono arriba del NPC
        if (runtimeIconRenderer != null)
        {
            var pos = transform.position;
            pos.y += spriteOffsetY;
            runtimeIconRenderer.transform.position = pos;
        }

        // Chequeo de rango: OverlapBox
        playerInRange = IsPlayerInsideBox();

        UpdateIconVisibility();

        if (playerInRange && Input.GetKeyDown(interactKey))
            Interact();

        // Si estás en diálogo y el player se va, cerramos sin cambiar escena
        if (isDialogueActive && !playerInRange)
            EndDialogue(false);
    }

    bool IsPlayerInsideBox()
    {
        Vector2 center = (Vector2)transform.position + interactBoxOffset;
        Collider2D hit = Physics2D.OverlapBox(center, interactBoxSize, 0f, playerLayer);

        if (hit == null && player != null)
        {
            hit = Physics2D.OverlapBox(center, interactBoxSize, 0f);
            if (hit != null && hit.CompareTag(playerTag)) return true;
            return false;
        }

        return hit != null;
    }

    bool EnemyIsDeadOrDying()
    {
        if (!requireEnemyDeadToInteract) return true;
        if (requiredEnemyHealth == null) return false;
        return requiredEnemyHealth.GetCurrentHealth() <= 0f;
    }

    public void Interact()
    {
        if (isTransitioning) return;
        if (currentActiveNPC != null && currentActiveNPC != this) return;
        if (!EnemyIsDeadOrDying()) return;

        if (isDialogueActive) NextLine();
        else StartDialogue();
    }

    void StartDialogue()
    {
        if (isTransitioning) return;
        if (currentActiveNPC != null && currentActiveNPC != this) return;
        if (!EnemyIsDeadOrDying()) return;

        currentActiveNPC = this;
        isDialogueActive = true;
        dialogueIndex = 0;

        if (dialogueData == null || dialogueData.dialogueLines == null || dialogueData.dialogueLines.Length == 0)
        {
            EndDialogue(false);
            return;
        }

        if (nameText != null) nameText.SetText(dialogueData.npcName);
        if (portraitImage != null) portraitImage.sprite = dialogueData.npcPortrait;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            dialogueAnimator ??= dialoguePanel.GetComponent<UIDialogueAnimator>();
            dialogueAnimator?.AnimateIn();
        }

        SetIconVisible(false);
        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (isTransitioning) return;

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
            // Terminó TODO: ahora “cortamos el mundo” y hacemos fade+scene
            BeginSceneTransition();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            if (isTransitioning) yield break;

            dialogueText.text += letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping = false;

        if (!isTransitioning &&
            dialogueData.autoProgressLines != null &&
            dialogueData.autoProgressLines.Length > dialogueIndex &&
            dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    void EndDialogue(bool finishedAllLines)
    {
        StopAllCoroutines();
        isTyping = false;
        isDialogueActive = false;
        if (dialogueText != null) dialogueText.SetText("");

        if (dialoguePanel != null)
        {
            // Si estamos transicionando, no animamos nada: cerramos ya.
            if (isTransitioning)
            {
                dialoguePanel.SetActive(false);
            }
            else
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

                dialoguePanel.SetActive(false);
            }
        }

        if (currentActiveNPC == this) currentActiveNPC = null;
        UpdateIconVisibility();
    }

    // === NUEVO: Acá se detiene todo lo que no sea el cambio de escena ===
    void BeginSceneTransition()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        // Cortamos TODO: coroutines, diálogo, icono, interacción, etc.
        StopAllCoroutines();
        isTyping = false;

        SetIconVisible(false);
        isDialogueActive = false;

        // Cerramos UI ya (sin AnimateOut para que no “haga cosas” durante el fade)
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // Liberamos el lock del NPC (ya no se vuelve a usar en esta escena)
        if (currentActiveNPC == this) currentActiveNPC = null;

        // Fade + load
        StartCoroutine(FadeToBlackThenLoad());
    }

    IEnumerator FadeToBlackThenLoad()
    {
        // Si no asignaste fadeImage, igual cambiamos de escena (sin efecto).
        if (fadeImage != null)
        {
            // Aseguro que esté visible
            fadeImage.gameObject.SetActive(true);

            Color c = fadeImage.color;
            float t = 0f;
            float startA = c.a;

            while (t < fadeDuration)
            {
                float dt = useUnscaledTimeForFade ? Time.unscaledDeltaTime : Time.deltaTime;
                t += dt;
                float a = Mathf.Lerp(startA, 1f, Mathf.Clamp01(t / fadeDuration));
                c.a = a;
                fadeImage.color = c;
                yield return null;
            }

            c.a = 1f;
            fadeImage.color = c;
        }

        LoadRandomScene();
    }

    void LoadRandomScene()
    {
        if (scenesToLoad == null || scenesToLoad.Count == 0) return;

        string current = SceneManager.GetActiveScene().name;

        List<string> candidates = scenesToLoad;
        if (avoidCurrentScene)
        {
            candidates = new List<string>();
            foreach (var s in scenesToLoad)
                if (!string.IsNullOrWhiteSpace(s) && s != current) candidates.Add(s);

            if (candidates.Count == 0) return;
        }

        int idx = Random.Range(0, candidates.Count);
        SceneManager.LoadScene(candidates[idx]);
    }

    void UpdateIconVisibility()
    {
        if (isTransitioning)
        {
            SetIconVisible(false);
            return;
        }

        bool canShow = playerInRange && !isDialogueActive && currentActiveNPC == null;

        if (requireEnemyDeadToInteract)
            canShow = canShow && EnemyIsDeadOrDying();

        SetIconVisible(canShow);
    }

    void SetIconVisible(bool visible)
    {
        if (interactRendererInScene != null) interactRendererInScene.enabled = visible;
        if (runtimeIconRenderer != null) runtimeIconRenderer.enabled = visible;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position + (Vector3)interactBoxOffset;
        Vector3 size = new Vector3(interactBoxSize.x, interactBoxSize.y, 0.01f);
        Gizmos.DrawWireCube(center, size);
    }
#endif
}
