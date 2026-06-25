using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    public Image image;
    public Sprite[] crosshairs;

    void Start()
    {
        ApplySettings();
    }

    public void ApplySettings()
    {
        int index = PlayerPrefs.GetInt("CrosshairIndex", 0);
        if (crosshairs.Length > index)
            image.sprite = crosshairs[index];

        float r = PlayerPrefs.GetFloat("CrosshairR", 1f);
        float g = PlayerPrefs.GetFloat("CrosshairG", 1f);
        float b = PlayerPrefs.GetFloat("CrosshairB", 1f);
        float a = PlayerPrefs.GetFloat("CrosshairA", 1f);

        image.color = new Color(r, g, b, a);
        //Debug.Log("ApplySettings ist an");
    }
}
