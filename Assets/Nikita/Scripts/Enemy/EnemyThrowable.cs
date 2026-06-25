using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyThrowable : MonoBehaviour
{
    [Header("Throw Settings")]
    [SerializeField] private float recoverDelay = 0.6f;

    private Rigidbody rb;
    private NavMeshAgent agent;
    private EnemyAI meleeAI;
    private EnemyRangedAI rangedAI;
    private EnemyStats enemyStats;
    private Collider[] ownColliders;

    private bool isThrown;
    private bool recoveryStarted;
    private float impactDamage;
    private float impactKnockback;

    public bool IsThrown => isThrown;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        meleeAI = GetComponent<EnemyAI>();
        rangedAI = GetComponent<EnemyRangedAI>();
        enemyStats = GetComponent<EnemyStats>();
        ownColliders = GetComponentsInChildren<Collider>(true);

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = true;
        }
    }

    public void Throw(
        Vector3 startPosition,
        Vector3 direction,
        float throwForce,
        float upwardForce,
        float damage,
        float knockback,
        Collider[] ownerColliders
    )
    {
        if (isThrown) return;
        if (enemyStats != null && enemyStats.IsDead) return;

        isThrown = true;
        recoveryStarted = false;
        impactDamage = damage;
        impactKnockback = knockback;

        transform.position = startPosition;

        if (agent != null && agent.enabled)
        {
            agent.enabled = false;
        }

        if (meleeAI != null) meleeAI.enabled = false;
        if (rangedAI != null) rangedAI.enabled = false;

        if (ownerColliders != null)
        {
            foreach (Collider own in ownColliders)
            {
                if (own == null) continue;

                foreach (Collider ownerCol in ownerColliders)
                {
                    if (ownerCol == null) continue;
                    Physics.IgnoreCollision(own, ownerCol, true);
                }
            }
        }

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = direction.normalized * throwForce + Vector3.up * upwardForce;
        }

        StartCoroutine(AutoRecoverAfterTime(2.5f, ownerColliders));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isThrown) return;

        if (collision.collider.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.collider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(impactDamage);
            }

            PlayerMovement playerMovement = collision.collider.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                Vector3 knockDir = (collision.transform.position - transform.position).normalized;
                knockDir.y = 0f;
                playerMovement.AddForce(knockDir * impactKnockback);
            }
        }

        if (!recoveryStarted)
        {
            StartCoroutine(RecoverRoutine(recoverDelay));
        }
    }

    private IEnumerator AutoRecoverAfterTime(float time, Collider[] ownerColliders)
    {
        yield return new WaitForSeconds(time);

        if (!recoveryStarted)
        {
            StartCoroutine(RecoverRoutine(recoverDelay));
        }

        if (ownerColliders != null)
        {
            foreach (Collider own in ownColliders)
            {
                if (own == null) continue;

                foreach (Collider ownerCol in ownerColliders)
                {
                    if (ownerCol == null) continue;
                    Physics.IgnoreCollision(own, ownerCol, false);
                }
            }
        }
    }

    private IEnumerator RecoverRoutine(float delay)
    {
        if (recoveryStarted) yield break;
        recoveryStarted = true;

        yield return new WaitForSeconds(delay);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (agent != null)
        {
            agent.enabled = true;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 3f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                agent.Warp(hit.position);
            }
        }

        if (meleeAI != null) meleeAI.enabled = true;
        if (rangedAI != null) rangedAI.enabled = true;

        isThrown = false;
        recoveryStarted = false;
    }
}