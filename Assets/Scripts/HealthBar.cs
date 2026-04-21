using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Health Bar")]
    public Image healthBarFill;
    public float maxHealth = 999f;
    private float currentHealth;

    [Header("Number Display")]
    public Image[] digitImages;
    public Sprite[] numberSprites;
    public Image percentSign;

    void Start()
    {
        currentHealth = 0f;
        UpdateHealthDisplay();
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth = Mathf.Clamp(currentHealth + damageAmount, 0, maxHealth);
        UpdateHealthDisplay();
    }

    public void Heal(float healAmount)
    {
        currentHealth = Mathf.Clamp(currentHealth - healAmount, 0, maxHealth);
        UpdateHealthDisplay();
    }

    public void SetHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        UpdateHealthDisplay();
    }

    public void ResetHealth()
    {
        currentHealth = 0f;
        UpdateHealthDisplay();
    }

    private void UpdateHealthDisplay()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }

        int displayValue = Mathf.RoundToInt(currentHealth);
        string healthString = displayValue.ToString().PadLeft(digitImages.Length, '0');

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

        if (percentSign != null)
        {
            percentSign.enabled = (currentHealth > 0);
        }
    }

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