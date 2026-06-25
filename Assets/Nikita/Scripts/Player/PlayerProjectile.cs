using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    private Rigidbody rb;

    private float damage;
    private int remainingPierces;
    private float lifetime;
    private Transform owner;

    [Header("Balance")]
    [SerializeField] private float pierceDamageFalloff = 0.75f;

    private readonly HashSet<EnemyStats> hitEnemies = new HashSet<EnemyStats>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(
        Vector3 direction,
        float speed,
        float projectileDamage,
        int pierceCount,
        float projectileLifetime,
        Transform projectileOwner
    )
    {
        damage = projectileDamage;
        remainingPierces = pierceCount;
        lifetime = projectileLifetime;
        owner = projectileOwner;

        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            transform.forward = rb.linearVelocity.normalized;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyStats enemy = other.GetComponentInParent<EnemyStats>();

        if (enemy != null)
        {
            if (hitEnemies.Contains(enemy))
                return;

            hitEnemies.Add(enemy);

            enemy.TakeDamage(damage, false, owner);

            if (remainingPierces > 0)
            {
                remainingPierces--;
                damage *= pierceDamageFalloff;
                return;
            }

            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}