using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Health Bar")]
    public Image healthBarFill;
    public float maxHealth = 999f;      // Maximum possible percentage (Melee caps at 999%)
    private float currentHealth;

    [Header("Number Display")]
    public Image[] digitImages;          // Hundreds, Tens, Ones
    public Sprite[] numberSprites;       // 0-9 sprites
    public Image percentSign;            // % sprite (optional)

    void Start()
    {
        // Start at 0% (like Melee)
        currentHealth = 0f;
        UpdateHealthDisplay();
    }

    /// <summary>
    /// Call this to add damage (increases percentage INSTANTLY)
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        currentHealth = Mathf.Clamp(currentHealth + damageAmount, 0, maxHealth);
        UpdateHealthDisplay();
    }

    /// <summary>
    /// Call this to heal (decreases percentage INSTANTLY)
    /// </summary>
    public void Heal(float healAmount)
    {
        currentHealth = Mathf.Clamp(currentHealth - healAmount, 0, maxHealth);
        UpdateHealthDisplay();
    }

    /// <summary>
    /// Sets health directly to a specific value INSTANTLY
    /// </summary>
    public void SetHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        UpdateHealthDisplay();
    }

    /// <summary>
    /// Resets health to 0% (like losing a stock)
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = 0f;
        UpdateHealthDisplay();
    }

    /// <summary>
    /// Updates both the fill bar and the number sprites
    /// </summary>
    private void UpdateHealthDisplay()
    {
        // 1. Update the filled bar
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }

        // 2. Update the number sprites
        int displayValue = Mathf.RoundToInt(currentHealth);
        string healthString = displayValue.ToString().PadLeft(digitImages.Length, '0');

        // Loop through each digit image
        for (int i = 0; i < digitImages.Length; i++)
        {
            if (i < healthString.Length)
            {
                char digitChar = healthString[i];
                int digitValue = int.Parse(digitChar.ToString());
                digitImages[i].sprite = numberSprites[digitValue];
                digitImages[i].enabled = true;
            }
            else
            {
                digitImages[i].enabled = false;
            }
        }

        // Hide percent sign when health is 0 (like Melee)
        if (percentSign != null)
        {
            percentSign.enabled = (currentHealth > 0);
        }
    }

    // ===== TEST METHODS (Right-Click in Inspector) =====

    [ContextMenu("Take Damage (10%)")]
    public void TakeDamage10()
    {
        TakeDamage(10);
    }

    [ContextMenu("Take Damage (25%)")]
    public void TakeDamage25()
    {
        TakeDamage(25);
    }

    [ContextMenu("Heal (10%)")]
    public void Heal10()
    {
        Heal(10);
    }

    [ContextMenu("Reset to 0%")]
    public void ResetToZero()
    {
        ResetHealth();
    }

    [ContextMenu("Set to 999% (Max)")]
    public void SetToMax()
    {
        SetHealth(maxHealth);
    }
}