using UnityEngine;

public class EnemyFallKillZone : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool countAsKill = true;

    private void OnTriggerEnter(Collider other)
    {
        EnemyStats enemyStats = other.GetComponentInParent<EnemyStats>();
        if (enemyStats == null) return;
        if (enemyStats.IsDead) return;

        if (countAsKill)
        {
            enemyStats.KillSilently();
        }
        else
        {
            Destroy(enemyStats.gameObject);
        }
    }
}