using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    public TMP_Text ammoText;

    PlayerStats playerStats;
    private void Start()
    {
        playerStats = FindAnyObjectByType<PlayerStats>();
    }
    void Update()
    {

        ammoText.text = $"{playerStats.CurrentAmmoInMagazine} / {playerStats.ReserveAmmo}";
    }
}