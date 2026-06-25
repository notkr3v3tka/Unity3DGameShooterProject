using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStats : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Damage Flash")]
    [SerializeField] private float damageFlashDuration = 0.2f;
    [SerializeField] private Color damageFlashColor = Color.red;

    [Header("Stun Visual")]
    [SerializeField] private Color stunColor = new Color(1f, 0.75f, 0f);
    [SerializeField] private float stunBlinkSpeed = 18f;
    [SerializeField] private float stunColorIntensity = 2.2f;
    [SerializeField] private float stunScalePulseAmount = 0.08f;
    [SerializeField] private float stunScalePulseSpeed = 16f;

    private float baseMaxHealth;
    private float damageMultiplier = 1f;

    private float currentHealth;
    private float damageCounter;

    private Renderer[] enemyRenderers;
    private Color[] originalColors;

    private bool isDead = false;
    private bool isStunned = false;

    private bool lastHitWasMelee = false;
    private Transform lastAttacker;

    private NavMeshAgent agent;
    private Coroutine knockbackCoroutine;
    private Coroutine stunCoroutine;

    private Vector3 originalScale;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float DamageMultiplier => damageMultiplier;
    public bool IsDead => isDead;
    public bool IsStunned => isStunned;

    private void Awake()
    {
        baseMaxHealth = maxHealth;
        currentHealth = maxHealth;

        agent = GetComponent<NavMeshAgent>();
        originalScale = transform.localScale;

        enemyRenderers = GetComponentsInChildren<Renderer>(true);
        originalColors = new Color[enemyRenderers.Length];

        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            if (enemyRenderers[i] != null && enemyRenderers[i].material != null)
            {
                originalColors[i] = enemyRenderers[i].material.color;
            }
        }
    }

    private void Update()
    {
        if (damageCounter > 0f)
        {
            damageCounter -= Time.deltaTime;
        }

        UpdateVisuals();
    }

    public void TakeDamage(float damage, bool isMelee = false, Transform attacker = null)
    {
        if (isDead) return;

        lastHitWasMelee = isMelee;
        lastAttacker = attacker;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayEnemyHitSound();
        }

        if (isStunned && isMelee)
        {
            currentHealth = 0f;
            Die();
            return;
        }

        currentHealth -= damage;
        damageCounter = damageFlashDuration;

        TryApplyStun(attacker);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void KillSilently()
    {
        if (isDead) return;

        currentHealth = 0f;
        lastHitWasMelee = false;
        lastAttacker = null;
        Die();
    }

    private void TryApplyStun(Transform attacker)
    {
        if (isDead || isStunned) return;
        if (attacker == null) return;

        PlayerStats playerStats = attacker.GetComponent<PlayerStats>();
        if (playerStats == null) return;

        float roll = Random.value;

        if (roll <= playerStats.StunChance)
        {
            ApplyStun(playerStats.StunDuration);
        }
    }

    public void ApplyStun(float duration)
    {
        if (isDead) return;

        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }

        stunCoroutine = StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        isStunned = false;
        transform.localScale = originalScale;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }

        stunCoroutine = null;
    }

    public void ApplyKnockback(Vector3 direction, float force, float duration)
    {
        if (isDead) return;

        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        knockbackCoroutine = StartCoroutine(KnockbackRoutine(direction, force, duration));
    }

    private IEnumerator KnockbackRoutine(Vector3 direction, float force, float duration)
    {
        bool hadAgent = agent != null && agent.enabled;

        if (hadAgent)
        {
            agent.enabled = false;
        }

        float timer = 0f;
        Vector3 velocity = direction.normalized * force;

        while (timer < duration)
        {
            transform.position += velocity * Time.deltaTime;
            velocity = Vector3.Lerp(velocity, Vector3.zero, 10f * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null;
        }

        if (agent != null)
        {
            agent.enabled = true;

            if (agent.isOnNavMesh)
            {
                agent.Warp(transform.position);

                if (!isStunned)
                {
                    agent.isStopped = false;
                }
                else
                {
                    agent.isStopped = true;
                }
            }
        }

        knockbackCoroutine = null;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (lastHitWasMelee && lastAttacker != null)
        {
            RewardPlayerForMeleeKill(lastAttacker);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddKill();
        }

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyKilled();
        }

        Destroy(gameObject);
    }

    private void RewardPlayerForMeleeKill(Transform attacker)
    {
        PlayerHealth playerHealth = attacker.GetComponent<PlayerHealth>();
        PlayerStats playerStats = attacker.GetComponent<PlayerStats>();

        if (playerHealth != null && playerStats != null)
        {
            playerHealth.Heal(playerStats.MeleeKillHealAmount);

            playerStats.ReserveAmmo = Mathf.Min(
                playerStats.ReserveAmmo + playerStats.MeleeKillAmmoAmount,
                playerStats.MaxReserveAmmo
            );
        }
    }

    private void UpdateVisuals()
    {
        if (enemyRenderers == null || enemyRenderers.Length == 0) return;

        if (isStunned)
        {
            float blink = Mathf.PingPong(Time.time * stunBlinkSpeed, 1f);
            float pulse = 1f + Mathf.Sin(Time.time * stunScalePulseSpeed) * stunScalePulseAmount;
            transform.localScale = originalScale * pulse;

            for (int i = 0; i < enemyRenderers.Length; i++)
            {
                Renderer rend = enemyRenderers[i];
                if (rend == null || rend.material == null) continue;

                Color boostedStunColor = stunColor * stunColorIntensity;
                Color targetColor = Color.Lerp(originalColors[i], boostedStunColor, blink);
                rend.material.color = targetColor;
            }

            return;
        }

        transform.localScale = originalScale;

        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            Renderer rend = enemyRenderers[i];
            if (rend == null || rend.material == null) continue;

            float normalizedValue = Mathf.InverseLerp(0f, damageFlashDuration, damageCounter);
            Color targetColor = Color.Lerp(originalColors[i], damageFlashColor, normalizedValue);
            rend.material.color = targetColor;
        }
    }

    public void ApplyWaveScaling(float healthMultiplier, float newDamageMultiplier)
    {
        maxHealth = baseMaxHealth * healthMultiplier;
        currentHealth = maxHealth;
        damageMultiplier = newDamageMultiplier;
    }
}