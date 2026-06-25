using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeCardManager : MonoBehaviour
{
    public static UpgradeCardManager Instance;

    [Header("Card Database")]
    [SerializeField] private List<UpgradeCardData> allCards = new List<UpgradeCardData>();

    [Header("UI - Main")]
    [SerializeField] private GameObject cardSelectionPanel;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private UpgradeCardUI cardUIPrefab;
    [SerializeField] private GameObject cardBackgroundOverlay;

    [Header("UI - Category Selection")]
    [SerializeField] private GameObject categorySelectionPanel;
    [SerializeField] private Button gunButton;
    [SerializeField] private Button meleeButton;
    [SerializeField] private Button movementButton;
    [SerializeField] private Button survivalButton;
    [SerializeField] private TMP_Text categoryTitleText;

    [Header("Rarity Chances")]
    [Range(0f, 100f)][SerializeField] private float commonChance = 60f;
    [Range(0f, 100f)][SerializeField] private float rareChance = 25f;
    [Range(0f, 100f)][SerializeField] private float epicChance = 10f;
    [Range(0f, 100f)][SerializeField] private float legendaryChance = 4f;
    [Range(0f, 100f)][SerializeField] private float monsterChance = 1f;

    [Header("Card Choice Settings")]
    [SerializeField] private int maxCardChoices = 5;

    private PlayerStats playerStats;
    private Action onSelectionFinished;

    private CardCategory selectedCategory;
    private bool categoryChosen;

    public bool IsCardSelectionOpen { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
        }

        if (playerStats != null)
        {
            playerStats.CardChoicesCount = Mathf.Clamp(playerStats.CardChoicesCount, 1, maxCardChoices);
        }

        if (cardSelectionPanel != null)
            cardSelectionPanel.SetActive(false);

        if (categorySelectionPanel != null)
            categorySelectionPanel.SetActive(false);

        BindCategoryButtons();

        IsCardSelectionOpen = false;
    }

    private void BindCategoryButtons()
    {
        if (gunButton != null)
        {
            gunButton.onClick.RemoveAllListeners();
            gunButton.onClick.AddListener(() => SelectCategory(CardCategory.Gun));
        }

        if (meleeButton != null)
        {
            meleeButton.onClick.RemoveAllListeners();
            meleeButton.onClick.AddListener(() => SelectCategory(CardCategory.Melee));
        }

        if (movementButton != null)
        {
            movementButton.onClick.RemoveAllListeners();
            movementButton.onClick.AddListener(() => SelectCategory(CardCategory.Movement));
        }

        if (survivalButton != null)
        {
            survivalButton.onClick.RemoveAllListeners();
            survivalButton.onClick.AddListener(() => SelectCategory(CardCategory.Survival));
        }
    }

    public void OpenCardSelection(Action finishedCallback = null)
    {
        if (IsCardSelectionOpen) return;
        if (playerStats == null) return;
        if (allCards.Count == 0) return;

        onSelectionFinished = finishedCallback;
        IsCardSelectionOpen = true;
        categoryChosen = false;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (cardBackgroundOverlay != null)
            cardBackgroundOverlay.SetActive(true);

        if (cardSelectionPanel != null)
            cardSelectionPanel.SetActive(false);

        if (categorySelectionPanel != null)
            categorySelectionPanel.SetActive(true);

        ClearCards();

        if (categoryTitleText != null)
            categoryTitleText.text = "Choose Upgrade Category";
    }

    public void SelectCategory(CardCategory category)
    {
        selectedCategory = category;
        categoryChosen = true;

        if (categorySelectionPanel != null)
            categorySelectionPanel.SetActive(false);

        ShowCardsForSelectedCategory();
    }

    private void ShowCardsForSelectedCategory()
    {
        if (!categoryChosen) return;
        if (cardSelectionPanel == null) return;
        if (cardContainer == null) return;
        if (cardUIPrefab == null) return;

        cardSelectionPanel.SetActive(true);
        ClearCards();

        int cardCountToShow = Mathf.Min(playerStats.CardChoicesCount, maxCardChoices);
        List<UpgradeCardData> selectedCards = GetRandomCards(cardCountToShow, selectedCategory);

        foreach (UpgradeCardData cardData in selectedCards)
        {
            UpgradeCardUI cardUI = Instantiate(cardUIPrefab, cardContainer);
            cardUI.Setup(cardData, this);
        }
    }

    public void SelectCard(UpgradeCardData cardData)
    {
        if (!IsCardSelectionOpen) return;

        ApplyCard(cardData);

        ClearCards();

        if (cardSelectionPanel != null)
            cardSelectionPanel.SetActive(false);

        if (categorySelectionPanel != null)
            categorySelectionPanel.SetActive(false);

        if (cardBackgroundOverlay != null)
            cardBackgroundOverlay.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        IsCardSelectionOpen = false;
        categoryChosen = false;

        onSelectionFinished?.Invoke();
        onSelectionFinished = null;
    }

    private void ApplyCard(UpgradeCardData cardData)
    {
        if (playerStats == null || cardData == null) return;

        switch (cardData.upgradeType)
        {
            case UpgradeType.Damage:
                playerStats.Damage += cardData.value;
                break;

            case UpgradeType.MoveSpeed:
                playerStats.MoveSpeed += cardData.value;
                break;

            case UpgradeType.JumpHeight:
                playerStats.JumpHeight += cardData.value;
                break;

            case UpgradeType.DashCooldownReduction:
                playerStats.DashCooldown = Mathf.Max(0.1f, playerStats.DashCooldown - cardData.value);
                break;

            case UpgradeType.MagazineSize:
                playerStats.MagazineSize += Mathf.RoundToInt(cardData.value);
                playerStats.CurrentAmmoInMagazine = playerStats.MagazineSize;
                break;

            case UpgradeType.ReloadSpeed:
                playerStats.ReloadDuration = Mathf.Max(0.1f, playerStats.ReloadDuration - cardData.value);
                break;

            case UpgradeType.FireRate:
                playerStats.FireRate += cardData.value;
                break;

            case UpgradeType.ExtraCardChoice:
                playerStats.CardChoicesCount = Mathf.Min(
                    playerStats.CardChoicesCount + Mathf.RoundToInt(cardData.value),
                    maxCardChoices
                );
                break;

            case UpgradeType.PierceCount:
                playerStats.PierceCount += Mathf.RoundToInt(cardData.value);
                break;

            case UpgradeType.MultiShot:
                playerStats.MultiShot += Mathf.RoundToInt(cardData.value);
                break;

            case UpgradeType.MeleeDamage:
                playerStats.MeleeDamage += cardData.value;
                break;

            case UpgradeType.MeleeRange:
                playerStats.MeleeRange += cardData.value;
                break;

            case UpgradeType.MeleeRadius:
                playerStats.MeleeRadius += cardData.value;
                break;

            case UpgradeType.DamageReduction:
                playerStats.DamageReduction = Mathf.Clamp(playerStats.DamageReduction + cardData.value, 0f, 0.4f);
                break;

            case UpgradeType.HealthRegen:
                playerStats.HealthRegenPerSecond += cardData.value;
                break;

            case UpgradeType.MaxHealth:
                PlayerHealth playerHealth = playerStats.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.IncreaseMaxHealth(cardData.value);
                }
                break;
        }

        Debug.Log("Applied card: " + cardData.cardName);
    }

    private List<UpgradeCardData> GetRandomCards(int count, CardCategory category)
    {
        List<UpgradeCardData> result = new List<UpgradeCardData>();
        List<UpgradeCardData> usedCards = new List<UpgradeCardData>();

        List<UpgradeCardData> categoryPool = new List<UpgradeCardData>();

        foreach (UpgradeCardData card in allCards)
        {
            if (card == null) continue;
            if (!IsCardAllowed(card)) continue;

            if (card.category == category || card.category == CardCategory.Universal)
            {
                categoryPool.Add(card);
            }
        }

        count = Mathf.Min(count, categoryPool.Count);

        int safety = 100;

        while (result.Count < count && safety > 0)
        {
            safety--;

            CardRarity selectedRarity = RollRarity();
            UpgradeCardData card = GetRandomCardByRarity(selectedRarity, usedCards, categoryPool);

            if (card == null)
            {
                card = GetRandomCardFromAll(usedCards, categoryPool);
            }

            if (card != null && !usedCards.Contains(card))
            {
                result.Add(card);
                usedCards.Add(card);
            }
        }

        return result;
    }

    private CardRarity RollRarity()
    {
        float totalChance = commonChance + rareChance + epicChance + legendaryChance + monsterChance;

        if (totalChance <= 0f)
            return CardRarity.Common;

        float roll = UnityEngine.Random.Range(0f, totalChance);

        if (roll < commonChance)
            return CardRarity.Common;

        roll -= commonChance;
        if (roll < rareChance)
            return CardRarity.Rare;

        roll -= rareChance;
        if (roll < epicChance)
            return CardRarity.Epic;

        roll -= epicChance;
        if (roll < legendaryChance)
            return CardRarity.Legendary;

        return CardRarity.Monster;
    }

    private UpgradeCardData GetRandomCardByRarity(CardRarity rarity, List<UpgradeCardData> excludedCards, List<UpgradeCardData> sourcePool)
    {
        List<UpgradeCardData> pool = new List<UpgradeCardData>();

        foreach (UpgradeCardData card in sourcePool)
        {
            if (card == null) continue;
            if (excludedCards.Contains(card)) continue;
            if (card.rarity == rarity)
            {
                pool.Add(card);
            }
        }

        if (pool.Count == 0)
            return null;

        int randomIndex = UnityEngine.Random.Range(0, pool.Count);
        return pool[randomIndex];
    }

    private UpgradeCardData GetRandomCardFromAll(List<UpgradeCardData> excludedCards, List<UpgradeCardData> sourcePool)
    {
        List<UpgradeCardData> pool = new List<UpgradeCardData>();

        foreach (UpgradeCardData card in sourcePool)
        {
            if (card == null) continue;
            if (excludedCards.Contains(card)) continue;
            pool.Add(card);
        }

        if (pool.Count == 0)
            return null;

        int randomIndex = UnityEngine.Random.Range(0, pool.Count);
        return pool[randomIndex];
    }

    private bool IsCardAllowed(UpgradeCardData card)
    {
        if (card == null) return false;
        if (playerStats == null) return false;

        if (card.upgradeType == UpgradeType.ExtraCardChoice &&
            playerStats.CardChoicesCount >= maxCardChoices)
        {
            return false;
        }

        return true;
    }

    private void ClearCards()
    {
        if (cardContainer == null) return;

        for (int i = cardContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(cardContainer.GetChild(i).gameObject);
        }
    }
}