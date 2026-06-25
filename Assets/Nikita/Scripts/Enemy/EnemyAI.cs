using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1f;

    private Transform player;
    private PlayerHealth playerHealth;
    private EnemyStats enemyStats;
    private float attackTimer;

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
    }

    private void Update()
    {
        if (player == null) return;
        if (agent == null) return;
        if (!agent.enabled) return;
        if (enemyStats != null && enemyStats.IsStunned) return;

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange)
        {
            ChasePlayer();
        }
        else
        {
            AttackPlayer();
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

        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        if (attackTimer <= 0f)
        {
            if (playerHealth != null)
            {
                float finalDamage = attackDamage;

                if (enemyStats != null)
                {
                    finalDamage *= enemyStats.DamageMultiplier;
                }

                playerHealth.TakeDamage(finalDamage);
            }

            attackTimer = attackCooldown;
        }
    }
}