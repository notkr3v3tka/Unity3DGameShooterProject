using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    public Slider volumeSlider;
    public Image volumeIcon;
    public Button muteButton;
    public Sprite muteIcon;
    public Sprite[] icons;

    private bool isMuted = false;
    private float lastVolume = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        volumeSlider.onValueChanged.AddListener(OnSliderChanged);
        muteButton.onClick.AddListener(ToggleMute);
        volumeSlider.value = 1f;
        UpdateIcon(volumeSlider.value);
        AudioListener.volume = volumeSlider.value;
    }

    void OnSliderChanged(float value)
    {
        if (isMuted && value > 0f)
            isMuted = false;

        AudioListener.volume = value;
        UpdateIcon(value);
    }

    void ToggleMute()
    {
        isMuted = !isMuted;
        if (isMuted)
        {
            lastVolume = volumeSlider.value;
            volumeSlider.value = 0f;
            AudioListener.volume = 0f;
            volumeIcon.sprite = muteIcon;
        }
        else
        {
            volumeSlider.value = lastVolume;
            AudioListener.volume = lastVolume;
            UpdateIcon(lastVolume);
        }
    }

    // Update is called once per frame
    void UpdateIcon(float value)
    {
        int index = Mathf.Clamp(Mathf.CeilToInt(value * icons.Length) - 1,0, icons.Length -1);
        volumeIcon.sprite = icons[index];
    }
}
