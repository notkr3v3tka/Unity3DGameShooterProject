using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Survival")]
    public float MaxHealth = 100f;
    public float DamageReduction = 0f;
    public float InvulnerabilityDuration = 0.35f;
    public float HealthRegenPerSecond = 1.5f;
    public float RegenDelayAfterDamage = 4f;

    [Header("Movement")]
    public float MoveSpeed = 6f;
    public float JumpHeight = 1.5f;
    public int MaxJumps = 2;
    public float DashSpeed = 14f;
    public float DashCooldown = 1f;

    [Header("Combat")]
    public float Damage = 25f;
    public float FireRate = 5f;
    public float ShootRange = 100f;
    public float ReloadDuration = 1.2f;

    [Header("Ammo")]
    public int MagazineSize = 12;
    public int CurrentAmmoInMagazine = 12;
    public int ReserveAmmo = 60;
    public int MaxReserveAmmo = 120;

    [Header("Special")]
    public int PierceCount = 0;
    public int MultiShot = 1;
    public int CardChoicesCount = 3;

    [Header("Projectile")]
    public float ProjectileSpeed = 35f;
    public float ProjectileLifetime = 3f;
    public float MultiShotSpreadAngle = 8f;

    [Header("Melee")]
    public float MeleeDamage = 35f;
    public float MeleeRange = 2f;
    public float MeleeRadius = 1.2f;
    public float MeleeCooldown = 0.6f;
    public float MeleeKnockbackForce = 10f;
    public float MeleeKnockbackDuration = 0.15f;

    [Header("Melee Kill Rewards")]
    public float MeleeKillHealAmount = 15f;
    public int MeleeKillAmmoAmount = 8;

    [Header("Stun")]
    public float StunChance = 0.1f;
    public float StunDuration = 2f;
}