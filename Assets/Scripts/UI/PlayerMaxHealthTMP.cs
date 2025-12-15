using UnityEngine;
using TMPro;

public class PlayerMaxHealthTMP : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private TMP_Text maxHealthText;

    private float lastMaxHealth = -1f;

    private void Start()
    {
        UpdateText();
    }

    private void Update()
    {
        // Detecta cambios en el maxHealth (perks, debuffs, etc)
        if (playerHealth == null) return;

        if (!Mathf.Approximately(playerHealth.maxHealth, lastMaxHealth))
        {
            UpdateText();
        }
    }

    private void UpdateText()
    {
        if (playerHealth == null || maxHealthText == null)
            return;

        lastMaxHealth = playerHealth.maxHealth;

        // Mostramos el valor actual del máximo
        maxHealthText.text = Mathf.CeilToInt(playerHealth.maxHealth).ToString();
    }
}
