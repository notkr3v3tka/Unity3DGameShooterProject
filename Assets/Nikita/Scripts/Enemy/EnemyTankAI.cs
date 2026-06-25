using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyTankAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform throwSpawnPoint;
    [SerializeField] private LayerMask throwableEnemyMask;

    [Header("General")]
    [SerializeField] private float chaseRange = 20f;
    [SerializeField] private float specialAttackRange = 10f;
    [SerializeField] private float attackDecisionCooldown = 3f;

    [Header("Charge Attack")]
    [SerializeField] private float chargeWindup = 0.6f;
    [SerializeField] private float chargeDuration = 1.2f;
    [SerializeField] private float chargeSpeed = 12f;
    [SerializeField] private float chargeDamage = 25f;
    [SerializeField] private float chargeKnockbackForce = 12f;
    [SerializeField] private float chargeHitRadius = 1.5f;

    [Header("Throw Attack")]
    [SerializeField] private float throwSearchRadius = 8f;
    [SerializeField] private float throwCooldown = 4f;
    [SerializeField] private float throwForce = 14f;
    [SerializeField] private float throwUpwardForce = 5f;
    [SerializeField] private float thrownImpactDamage = 20f;
    [SerializeField] private float thrownImpactKnockback = 10f;

    private Transform player;
    private PlayerHealth playerHealth;
    private EnemyStats enemyStats;
    private float decisionTimer;
    private bool isBusy;
    private bool chargeHasHit;
    private Collider[] ownColliders;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            playerHealth = playerObject.GetComponent<PlayerHealth>();
        }

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        enemyStats = GetComponent<EnemyStats>();
        ownColliders = GetComponentsInChildren<Collider>(true);
    }

    private void Update()
    {
        if (player == null) return;
        if (agent == null) return;
        if (!agent.enabled) return;
        if (!agent.isOnNavMesh) return;
        if (enemyStats != null && enemyStats.IsStunned) return;
        if (isBusy) return;

        if (decisionTimer > 0f)
        {
            decisionTimer -= Time.deltaTime;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > chaseRange)
        {
            ChasePlayer();
            return;
        }

        if (distanceToPlayer > specialAttackRange)
        {
            ChasePlayer();
            return;
        }

        if (decisionTimer <= 0f)
        {
            TryUseSpecialAttack();
        }
        else
        {
            ChasePlayer();
        }
    }

    private void ChasePlayer()
    {
        agent.isStopped = false;
        agent.speed = 2.5f;
        agent.SetDestination(player.position);
    }

    private void TryUseSpecialAttack()
    {
        Collider nearbyEnemy = FindThrowableEnemy();

        if (nearbyEnemy != null && Random.value < 0.5f)
        {
            StartCoroutine(ThrowAttackRoutine(nearbyEnemy));
        }
        else
        {
            StartCoroutine(ChargeAttackRoutine());
        }
    }

    private Collider FindThrowableEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, throwSearchRadius, throwableEnemyMask);

        float closestDistance = float.MaxValue;
        Collider closest = null;

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            EnemyStats stats = hit.GetComponentInParent<EnemyStats>();
            if (stats == null) continue;
            if (stats.IsDead) continue;

            EnemyTankAI tank = hit.GetComponentInParent<EnemyTankAI>();
            if (tank != null) continue;

            EnemyThrowable throwable = hit.GetComponentInParent<EnemyThrowable>();
            if (throwable == null) continue;
            if (throwable.IsThrown) continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = hit;
            }
        }

        return closest;
    }

    private IEnumerator ThrowAttackRoutine(Collider targetCollider)
    {
        isBusy = true;
        decisionTimer = throwCooldown;

        agent.isStopped = true;

        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0f;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        yield return new WaitForSeconds(0.4f);

        if (targetCollider != null)
        {
            EnemyThrowable throwable = targetCollider.GetComponentInParent<EnemyThrowable>();

            if (throwable != null && player != null)
            {
                Vector3 spawnPosition = throwSpawnPoint != null
                    ? throwSpawnPoint.position
                    : transform.position + transform.forward * 1.5f + Vector3.up;

                Vector3 dir = (player.position + Vector3.up * 1f - spawnPosition).normalized;

                float finalThrownDamage = thrownImpactDamage;
                if (enemyStats != null)
                {
                    finalThrownDamage *= enemyStats.DamageMultiplier;
                }

                throwable.Throw(
                    spawnPosition,
                    dir,
                    throwForce,
                    throwUpwardForce,
                    finalThrownDamage,
                    thrownImpactKnockback,
                    ownColliders
                );
            }
        }

        yield return new WaitForSeconds(0.3f);

        agent.isStopped = false;
        isBusy = false;
    }

    private IEnumerator ChargeAttackRoutine()
    {
        isBusy = true;
        decisionTimer = attackDecisionCooldown;
        chargeHasHit = false;

        agent.isStopped = true;

        Vector3 chargeDirection = (player.position - transform.position).normalized;
        chargeDirection.y = 0f;

        if (chargeDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(chargeDirection);
        }

        yield return new WaitForSeconds(chargeWindup);

        float timer = 0f;

        while (timer < chargeDuration)
        {
            transform.position += chargeDirection * chargeSpeed * Time.deltaTime;

            CheckChargeHit();

            timer += Time.deltaTime;
            yield return null;
        }

        isBusy = false;
    }

    private void CheckChargeHit()
    {
        if (chargeHasHit) return;
        if (player == null) return;

        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 1f, chargeHitRadius);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            PlayerHealth health = hit.GetComponent<PlayerHealth>();
            if (health != null)
            {
                float finalDamage = chargeDamage;

                if (enemyStats != null)
                {
                    finalDamage *= enemyStats.DamageMultiplier;
                }

                health.TakeDamage(finalDamage);
            }

            PlayerMovement movement = hit.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                Vector3 knockDir = (hit.transform.position - transform.position).normalized;
                knockDir.y = 0f;
                movement.AddForce(knockDir * chargeKnockbackForce);
            }

            chargeHasHit = true;
            break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, specialAttackRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, throwSearchRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 1f, chargeHitRadius);
    }
}