using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    public TMP_Text waveText;

    void Update()
    {
        waveText.text = "Wave: " + WaveManager.Instance.CurrentWaveNumber;
    }
}
