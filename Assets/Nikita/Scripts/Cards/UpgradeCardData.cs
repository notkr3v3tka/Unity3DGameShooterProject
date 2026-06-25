using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade Card", menuName = "Cards/Upgrade Card")]
public class UpgradeCardData : ScriptableObject
{
    public string cardName;

    [TextArea]
    public string description;

    public CardRarity rarity;
    public CardCategory category;
    public UpgradeType upgradeType;

    public float value;

    public Sprite icon;
}