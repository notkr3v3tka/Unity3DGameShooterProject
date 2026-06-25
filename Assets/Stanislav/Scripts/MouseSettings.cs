using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MouseSettings : MonoBehaviour
{
    public Slider sensitivitySlider;
    public TMP_Text sensitivityText;

    public float mouseSensitivity = 1.0f;

    void Start()
    {
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1.0f);

        sensitivitySlider.value = mouseSensitivity;
        sensitivityText.text = mouseSensitivity.ToString("0.00");

        sensitivitySlider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnSliderChanged(float value)
    {
        mouseSensitivity = value;
        sensitivityText.text = value.ToString("0.00");

        PlayerPrefs.SetFloat("MouseSensitivity", value);
    }

    public void ResetSensitivity()
    {
        mouseSensitivity = 1.0f;
        sensitivitySlider.value = mouseSensitivity;
        sensitivityText.text = mouseSensitivity.ToString("0.00");
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
        PlayerPrefs.Save();
    }
}
