using UnityEngine;
using UnityEngine.AI;

public class EnemyRangedAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject arrowPrefab;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 12f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float arrowSpeed = 20f;
    [SerializeField] private float arrowDamage = 12f;

    [Header("Line Of Sight")]
    [SerializeField] private LayerMask lineOfSightMask;
    [SerializeField] private float playerAimHeight = 1.0f;

    [Header("Reposition")]
    [SerializeField] private float preferredDistanceFromPlayer = 10f;
    [SerializeField] private float repositionCooldown = 1.2f;
    [SerializeField] private float repositionReachDistance = 1.2f;
    [SerializeField] private float repositionSampleRadius = 2f;
    [SerializeField] private int repositionAttempts = 8;

    private Transform player;
    private EnemyStats enemyStats;
    private float attackTimer;

    private float repositionTimer;
    private Vector3 repositionTarget;
    private bool hasRepositionTarget;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        enemyStats = GetComponent<EnemyStats>();

        if (agent != null)
        {
            agent.avoidancePriority = Random.Range(20, 80);
        }
    }

    private void Update()
    {
        if (player == null) return;
        if (agent == null) return;
        if (!agent.enabled) return;
        if (!agent.isOnNavMesh) return;
        if (enemyStats != null && enemyStats.IsStunned) return;

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }

        if (repositionTimer > 0f)
        {
            repositionTimer -= Time.deltaTime;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange)
        {
            hasRepositionTarget = false;
            ChasePlayer();
            return;
        }

        if (HasLineOfSightToPlayer())
        {
            hasRepositionTarget = false;
            AttackPlayer();
        }
        else
        {
            Reposition();
        }
    }

    private void ChasePlayer()
    {
        if (!agent.enabled) return;
        if (!agent.isOnNavMesh) return;

        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.isStopped = true;

        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        if (attackTimer <= 0f)
        {
            ShootArrow();
            attackTimer = attackCooldown;
        }
    }

    private void Reposition()
    {
        agent.isStopped = false;

        bool reachedTarget = hasRepositionTarget &&
                             Vector3.Distance(transform.position, repositionTarget) <= repositionReachDistance;

        if ((!hasRepositionTarget || reachedTarget) && repositionTimer <= 0f)
        {
            FindRepositionTarget();
            repositionTimer = repositionCooldown;
        }

        if (hasRepositionTarget)
        {
            agent.SetDestination(repositionTarget);
        }
        else
        {
            Vector3 fallbackDir = (transform.position - player.position).normalized;
            if (fallbackDir == Vector3.zero)
            {
                fallbackDir = transform.right;
            }

            Vector3 fallbackPos = player.position + fallbackDir * preferredDistanceFromPlayer;

            if (NavMesh.SamplePosition(fallbackPos, out NavMeshHit fallbackHit, repositionSampleRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(fallbackHit.position);
            }
            else
            {
                agent.SetDestination(player.position);
            }
        }
    }

    private void FindRepositionTarget()
    {
        hasRepositionTarget = false;

        Vector3 dirFromPlayer = (transform.position - player.position).normalized;
        if (dirFromPlayer == Vector3.zero)
        {
            dirFromPlayer = transform.right;
        }

        for (int i = 0; i < repositionAttempts; i++)
        {
            float randomAngle = Random.Range(-120f, 120f);
            Vector3 rotatedDir = Quaternion.Euler(0f, randomAngle, 0f) * dirFromPlayer;
            Vector3 candidate = player.position + rotatedDir * preferredDistanceFromPlayer;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit navHit, repositionSampleRadius, NavMesh.AllAreas))
            {
                if (HasLineOfSightFromPosition(navHit.position))
                {
                    repositionTarget = navHit.position;
                    hasRepositionTarget = true;
                    return;
                }
            }
        }
    }

    private bool HasLineOfSightToPlayer()
    {
        if (shootPoint == null || player == null) return false;

        Vector3 origin = shootPoint.position;
        Vector3 target = player.position + Vector3.up * playerAimHeight;
        Vector3 direction = (target - origin).normalized;
        float distance = Vector3.Distance(origin, target);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, lineOfSightMask))
        {
            return hit.transform.CompareTag("Player");
        }

        return false;
    }

    private bool HasLineOfSightFromPosition(Vector3 testPosition)
    {
        if (player == null) return false;

        Vector3 origin = testPosition + Vector3.up * 1.2f;
        Vector3 target = player.position + Vector3.up * playerAimHeight;
        Vector3 direction = (target - origin).normalized;
        float distance = Vector3.Distance(origin, target);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, lineOfSightMask))
        {
            return hit.transform.CompareTag("Player");
        }

        return false;
    }

    private void ShootArrow()
    {
        if (arrowPrefab == null || shootPoint == null || player == null) return;

        GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);

        Vector3 targetDirection = (player.position + Vector3.up * playerAimHeight - shootPoint.position).normalized;
        arrow.transform.forward = targetDirection;

        ArrowProjectile arrowProjectile = arrow.GetComponent<ArrowProjectile>();
        if (arrowProjectile != null)
        {
            float finalDamage = arrowDamage;

            if (enemyStats != null)
            {
                finalDamage *= enemyStats.DamageMultiplier;
            }

            arrowProjectile.Initialize(targetDirection, arrowSpeed, finalDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (hasRepositionTarget)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(repositionTarget, 0.25f);
            Gizmos.DrawLine(transform.position, repositionTarget);
        }
    }
}