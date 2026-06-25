using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;

    [Header("Visual")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image cardBorder;

    private UpgradeCardData cardData;
    private UpgradeCardManager cardManager;

    public void Setup(UpgradeCardData data, UpgradeCardManager manager)
    {
        cardData = data;
        cardManager = manager;

        if (cardNameText != null)
            cardNameText.text = data.cardName;

        if (descriptionText != null)
            descriptionText.text = data.description;

        if (rarityText != null)
            rarityText.text = data.rarity.ToString();

        if (iconImage != null)
            iconImage.sprite = data.icon;

        ApplyRarityColor(data.rarity);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnCardClicked);
        }
    }

    private void OnCardClicked()
    {
        if (cardManager != null && cardData != null)
        {
            cardManager.SelectCard(cardData);
        }
    }

    private void ApplyRarityColor(CardRarity rarity)
    {
        Color bg = Color.white;
        Color border = Color.white;
        Color textColor = Color.white;

        switch (rarity)
        {
            case CardRarity.Common:
                bg = new Color(0.7f, 0.7f, 0.7f);
                border = new Color(0.5f, 0.5f, 0.5f);
                textColor = Color.white;
                break;

            case CardRarity.Rare:
                bg = new Color(0.3f, 0.5f, 1f);
                border = new Color(0.1f, 0.3f, 0.9f);
                textColor = new Color(0.8f, 0.9f, 1f);
                break;

            case CardRarity.Epic:
                bg = new Color(0.6f, 0.3f, 1f);
                border = new Color(0.4f, 0.1f, 0.8f);
                textColor = new Color(0.9f, 0.8f, 1f);
                break;

            case CardRarity.Legendary:
                bg = new Color(1f, 0.6f, 0.1f);
                border = new Color(1f, 0.4f, 0f);
                textColor = new Color(1f, 0.9f, 0.5f);
                break;

            case CardRarity.Monster:
                bg = new Color(0.8f, 0.1f, 0.1f);
                border = new Color(0.4f, 0f, 0f);
                textColor = new Color(1f, 0.6f, 0.6f);
                break;
        }

        if (cardBackground != null)
            cardBackground.color = bg;

        if (cardBorder != null)
            cardBorder.color = border;

        if (rarityText != null)
            rarityText.color = textColor;
    }
}