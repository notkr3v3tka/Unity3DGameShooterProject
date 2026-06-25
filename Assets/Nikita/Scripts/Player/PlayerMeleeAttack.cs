using System.Collections.Generic;
using UnityEngine;

public class PlayerMeleeAttack : MonoBehaviour
{
    private PlayerStats playerStats;

    [Header("References")]
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private Transform meleeEffectSpawnPoint;
    [SerializeField] private GameObject meleeEffectPrefab;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private LayerMask enemyLayerMask;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip meleeSwingClip;
    [SerializeField] private AudioClip meleeHitClip;

    [Header("Effect Settings")]
    [SerializeField] private float meleeEffectLifetime = 0.4f;
    [SerializeField] private float hitEffectLifetime = 0.25f;

    [Header("Melee Visual Tuning")]
    [SerializeField] private float baseVisualRange = 2f;
    [SerializeField] private float baseVisualRadius = 1.2f;

    [SerializeField] private Vector3 baseEffectScale = Vector3.one;

    [SerializeField] private float forwardOffsetPerRange = 0.5f;
    [SerializeField] private float leftOffsetPerRadius = 0.5f;
    [SerializeField] private float backwardOffsetPerRadius = 0.15f;

    private float meleeCooldownTimer;

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (UpgradeCardManager.Instance != null && UpgradeCardManager.Instance.IsCardSelectionOpen)
            return;

        if (meleeCooldownTimer > 0f)
        {
            meleeCooldownTimer -= Time.deltaTime;
        }

        if (Input.GetMouseButton(1) && meleeCooldownTimer <= 0f)
        {
            PerformMeleeAttack();
            PlayMeleeEffect();
            PlaySwingSound();

            meleeCooldownTimer = playerStats.MeleeCooldown;
        }
    }

    private void PerformMeleeAttack()
    {
        if (playerStats == null || attackOrigin == null) return;

        HashSet<EnemyStats> hitEnemies = new HashSet<EnemyStats>();
        bool hitSomeone = false;

        float maxAngle = Mathf.Atan2(playerStats.MeleeRadius, playerStats.MeleeRange) * Mathf.Rad2Deg + 10f;

        Vector3 capsuleStart = attackOrigin.position + attackOrigin.forward * 0.2f;
        Vector3 capsuleEnd = attackOrigin.position + attackOrigin.forward * playerStats.MeleeRange;

        Collider[] hits = Physics.OverlapCapsule(
            capsuleStart,
            capsuleEnd,
            playerStats.MeleeRadius,
            enemyLayerMask
        );

        foreach (Collider hit in hits)
        {
            EnemyStats enemy = hit.GetComponentInParent<EnemyStats>();
            if (enemy == null) continue;
            if (enemy.IsDead) continue;
            if (hitEnemies.Contains(enemy)) continue;

            Vector3 pointToCheck = hit.ClosestPoint(attackOrigin.position);
            Vector3 directionToEnemy = (pointToCheck - attackOrigin.position).normalized;

            float angleToEnemy = Vector3.Angle(attackOrigin.forward, directionToEnemy);
            if (angleToEnemy > maxAngle) continue;

            hitEnemies.Add(enemy);
            hitSomeone = true;

            Vector3 knockbackDirection = (enemy.transform.position - transform.position).normalized;
            knockbackDirection.y = 0f;

            enemy.ApplyKnockback(
                knockbackDirection,
                playerStats.MeleeKnockbackForce,
                playerStats.MeleeKnockbackDuration
            );

            enemy.TakeDamage(playerStats.MeleeDamage, true, transform);

            PlayHitEffect(pointToCheck);
        }

        if (hitSomeone)
        {
            PlayHitSound();
        }
    }

    private void TryHitEnemy(Collider collider, Vector3 hitPoint, float maxAngle, HashSet<EnemyStats> hitEnemies, ref bool hitSomeone)
    {
        EnemyStats enemy = collider.GetComponentInParent<EnemyStats>();
        if (enemy == null) return;
        if (hitEnemies.Contains(enemy)) return;
        if (enemy.IsDead) return;

        Vector3 directionToEnemy = (enemy.transform.position - attackOrigin.position).normalized;
        float angleToEnemy = Vector3.Angle(attackOrigin.forward, directionToEnemy);

        if (angleToEnemy > maxAngle)
            return;

        hitEnemies.Add(enemy);
        hitSomeone = true;

        Vector3 knockbackDirection = (enemy.transform.position - transform.position).normalized;
        knockbackDirection.y = 0f;

        enemy.ApplyKnockback(
            knockbackDirection,
            playerStats.MeleeKnockbackForce,
            playerStats.MeleeKnockbackDuration
        );

        enemy.TakeDamage(playerStats.MeleeDamage, true, transform);
        PlayHitEffect(hitPoint);
    }

    private void PlayMeleeEffect()
    {
        if (meleeEffectPrefab == null) return;
        if (attackOrigin == null || playerStats == null) return;

        Transform visualPoint = meleeEffectSpawnPoint != null ? meleeEffectSpawnPoint : attackOrigin;

        float rangeDelta = playerStats.MeleeRange - baseVisualRange;
        float radiusDelta = playerStats.MeleeRadius - baseVisualRadius;

        Vector3 effectScale = baseEffectScale;
        effectScale.z += radiusDelta;
        effectScale.y += radiusDelta * 0.15f;

        Vector3 spawnPosition = visualPoint.position;
        spawnPosition -= visualPoint.right * rangeDelta;

        float visualBackOffset = radiusDelta * 0.5f;
        spawnPosition -= visualPoint.forward * visualBackOffset;

        GameObject effect = Instantiate(
            meleeEffectPrefab,
            spawnPosition,
            visualPoint.rotation
        );

        effect.transform.localScale = effectScale;

        Destroy(effect, meleeEffectLifetime);
    }

    private void PlayHitEffect(Vector3 position)
    {
        if (hitEffectPrefab == null) return;

        GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
        Destroy(effect, hitEffectLifetime);
    }

    private void PlaySwingSound()
    {
        if (audioSource == null) return;
        if (meleeSwingClip == null) return;

        audioSource.PlayOneShot(meleeSwingClip);
    }

    private void PlayHitSound()
    {
        if (audioSource == null) return;
        if (meleeHitClip == null) return;

        audioSource.PlayOneShot(meleeHitClip);
    }

    private void OnDrawGizmosSelected()
    {
        PlayerStats stats = GetComponent<PlayerStats>();
        if (attackOrigin == null || stats == null) return;

        Gizmos.color = Color.red;

        Vector3 start = attackOrigin.position;
        Vector3 end = attackOrigin.position + attackOrigin.forward * stats.MeleeRange;

        Gizmos.DrawWireSphere(start, stats.MeleeRadius);
        Gizmos.DrawWireSphere(end, stats.MeleeRadius);
        Gizmos.DrawLine(start + attackOrigin.right * stats.MeleeRadius, end + attackOrigin.right * stats.MeleeRadius);
        Gizmos.DrawLine(start - attackOrigin.right * stats.MeleeRadius, end - attackOrigin.right * stats.MeleeRadius);
        Gizmos.DrawLine(start + attackOrigin.up * stats.MeleeRadius, end + attackOrigin.up * stats.MeleeRadius);
        Gizmos.DrawLine(start - attackOrigin.up * stats.MeleeRadius, end - attackOrigin.up * stats.MeleeRadius);
    }
}