using UnityEngine;
using UnityEngine.UI;

public class UICardBinder : MonoBehaviour
{
    void Start()
    {
        Button[] cards = GetComponentsInChildren<Button>(true);
        foreach (Button card in cards)
        {
            card.onClick.AddListener(PlayCardSelectSound);
        }
    }

    void PlayCardSelectSound()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayCardSelectSound();
    }
}