using UnityEngine;
using UnityEngine.UI;

public class DashUI : MonoBehaviour
{
    public Slider energyBar;
    PlayerMovement playerMovement;
    PlayerStats playerStats;

    void Start()
    {
       playerMovement = FindAnyObjectByType<PlayerMovement>();
        playerStats = FindAnyObjectByType<PlayerStats>();
    }

    void Update()
    {
        if (playerMovement == null || energyBar == null) return;

        float percent = Mathf.InverseLerp(playerStats.DashCooldown, 0, playerMovement.DashCooldownTimer);
        energyBar.value = percent;
    }
}