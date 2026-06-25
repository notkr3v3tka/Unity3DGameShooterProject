using UnityEngine;
using TMPro;

public class LifeUI : MonoBehaviour
{
    PlayerHealth player;
    public GameObject[] hearts;
    public TMP_Text helthText;

    private void Start()
    {
        player = FindAnyObjectByType<PlayerHealth>();
    }
    void Update()
    {
        UpdateHearts();
        //Debug.Log("HP: " + player.CurrentHealth);

    }

    public void UpdateHearts()
    {
        if (player == null || hearts.Length == 0) return;

        helthText.text = player.CurrentHealth.ToString("0");

        int currentHearts = Mathf.CeilToInt(
            player.CurrentHealth / (player.MaxHealth / hearts.Length)
        );

        //Debug.Log($"HP: {player.CurrentHealth} → Hearts: {currentHearts}");

        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].SetActive(i < currentHearts);
        }
    }
}