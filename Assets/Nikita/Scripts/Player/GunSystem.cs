using UnityEngine;

public class GunSystem : MonoBehaviour
{
    private PlayerStats playerStats;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Settings")]
    [SerializeField] private LayerMask aimLayerMask;

    [Header("Balance")]
    [SerializeField] private int maxMultiShot = 4;

    private float fireCooldownTimer;
    private float reloadCooldownTimer;
    private bool isReloading;

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (UpgradeCardManager.Instance != null && UpgradeCardManager.Instance.IsCardSelectionOpen)
            return;

        HandleTimers();
        HandleShooting();
        HandleReload();
    }

    private void HandleTimers()
    {
        if (fireCooldownTimer > 0f)
        {
            fireCooldownTimer -= Time.deltaTime;
        }

        if (isReloading)
        {
            reloadCooldownTimer -= Time.deltaTime;

            if (reloadCooldownTimer <= 0f)
            {
                FinishReload();
            }
        }
    }

    private void HandleShooting()
    {
        if (isReloading) return;

        if (Input.GetMouseButton(0) && fireCooldownTimer <= 0f)
        {
            if (playerStats.CurrentAmmoInMagazine > 0 && !PauseMenu.Instance.isPaused)
            {
                Shoot();
                playerStats.CurrentAmmoInMagazine--;
                fireCooldownTimer = 1f / Mathf.Max(playerStats.FireRate, 0.01f);
            }
            else
            {
                StartReload();
            }
        }
    }

    private void HandleReload()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartReload();
        }

        if (Input.GetMouseButtonDown(0) && playerStats.CurrentAmmoInMagazine <= 0 && !isReloading)
        {
            StartReload();
        }
    }

    private void StartReload()
    {
        if (isReloading) return;
        if (playerStats.CurrentAmmoInMagazine >= playerStats.MagazineSize) return;
        if (playerStats.ReserveAmmo <= 0) return;

        isReloading = true;
        reloadCooldownTimer = playerStats.ReloadDuration;
    }

    private void FinishReload()
    {
        isReloading = false;

        int missingAmmo = playerStats.MagazineSize - playerStats.CurrentAmmoInMagazine;
        int ammoToLoad = Mathf.Min(missingAmmo, playerStats.ReserveAmmo);

        playerStats.CurrentAmmoInMagazine += ammoToLoad;
        playerStats.ReserveAmmo -= ammoToLoad;
    }

    private void Shoot()
    {
        if (projectilePrefab == null || shootPoint == null || playerCamera == null) return;

        Vector3 aimDirection = GetAimDirection();

        int projectileCount = Mathf.Clamp(Mathf.Max(1, playerStats.MultiShot), 1, maxMultiShot);

        float damagePerProjectile = CalculateDamagePerProjectile(projectileCount);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayLaserShootSound();
        }

        if (projectileCount == 1)
        {
            SpawnProjectile(aimDirection, damagePerProjectile);
            return;
        }

        float totalSpread = playerStats.MultiShotSpreadAngle;
        float halfSpread = totalSpread * 0.5f;

        for (int i = 0; i < projectileCount; i++)
        {
            float t = projectileCount == 1 ? 0f : (float)i / (projectileCount - 1);
            float angleOffset = Mathf.Lerp(-halfSpread, halfSpread, t);

            Vector3 spreadDirection = Quaternion.AngleAxis(angleOffset, playerCamera.transform.up) * aimDirection;
            SpawnProjectile(spreadDirection.normalized, damagePerProjectile);
        }
    }

    private float CalculateDamagePerProjectile(int projectileCount)
    {
        float multiplier = 1f / Mathf.Sqrt(projectileCount);
        return playerStats.Damage * multiplier;
    }

    private Vector3 GetAimDirection()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, playerStats.ShootRange, aimLayerMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * playerStats.ShootRange;
        }

        return (targetPoint - shootPoint.position).normalized;
    }

    private void SpawnProjectile(Vector3 direction, float projectileDamage)
    {
        GameObject projectileObject = Instantiate(
            projectilePrefab,
            shootPoint.position,
            Quaternion.LookRotation(direction)
        );

        PlayerProjectile projectile = projectileObject.GetComponent<PlayerProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(
                direction,
                playerStats.ProjectileSpeed,
                projectileDamage,
                playerStats.PierceCount,
                playerStats.ProjectileLifetime,
                transform
            );
        }
    }
}