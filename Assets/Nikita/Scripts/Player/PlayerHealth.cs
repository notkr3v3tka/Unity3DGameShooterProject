using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Runtime")]
    [SerializeField] private float currentHealth;

    private PlayerStats playerStats;

    private float invulnerabilityTimer;
    private float regenDelayTimer;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => playerStats != null ? playerStats.MaxHealth : 0f;

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();

        if (playerStats != null)
        {
            currentHealth = playerStats.MaxHealth;
        }
    }

    private void Update()
    {
        HandleTimers();
        HandleRegeneration();
    }

    private void HandleTimers()
    {
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;
        }

        if (regenDelayTimer > 0f)
        {
            regenDelayTimer -= Time.deltaTime;
        }
    }

    private void HandleRegeneration()
    {
        if (playerStats == null) return;
        if (currentHealth <= 0f) return;
        if (regenDelayTimer > 0f) return;
        if (currentHealth >= playerStats.MaxHealth) return;
        if (playerStats.HealthRegenPerSecond <= 0f) return;

        currentHealth += playerStats.HealthRegenPerSecond * Time.deltaTime;
        currentHealth = Mathf.Min(currentHealth, playerStats.MaxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (playerStats == null) return;
        if (currentHealth <= 0f) return;
        if (invulnerabilityTimer > 0f) return;

        float clampedReduction = Mathf.Clamp01(playerStats.DamageReduction);
        float finalDamage = damage * (1f - clampedReduction);

        currentHealth -= finalDamage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        invulnerabilityTimer = playerStats.InvulnerabilityDuration;
        regenDelayTimer = playerStats.RegenDelayAfterDamage;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (playerStats == null) return;
        if (currentHealth <= 0f) return;
        if (amount <= 0f) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, playerStats.MaxHealth);
    }

    public void IncreaseMaxHealth(float amount, bool healToFull = false)
    {
        if (playerStats == null) return;
        if (amount <= 0f) return;

        playerStats.MaxHealth += amount;

        if (healToFull)
        {
            currentHealth = playerStats.MaxHealth;
        }
        else
        {
            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, playerStats.MaxHealth);
        }
    }

    private void Die()
    {
        Debug.Log("Player died");

        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.GameOver();
        }
    }
}