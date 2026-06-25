using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private EnemyStats enemyStats;
    [SerializeField] private Image fillImage;

    [Header("Colors")]
    private Color normalColor = new Color(0.2f, 1f, 0.2f);
    private Color stunnedColor = new Color(1f, 0.7f, 0f);

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (enemyStats == null || fillImage == null) return;

        float healthPercent = 0f;

        if (enemyStats.MaxHealth > 0f)
        {
            healthPercent = enemyStats.CurrentHealth / enemyStats.MaxHealth;
        }

        fillImage.transform.localScale = new Vector3(healthPercent, 1f, 1f);

        fillImage.color = enemyStats.IsStunned ? stunnedColor : normalColor;

        if (mainCamera != null)
        {
            transform.forward = mainCamera.transform.forward;
        }
    }
}