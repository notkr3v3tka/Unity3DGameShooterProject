using UnityEngine;
using UnityEngine.UI;

public class ColorController : MonoBehaviour
{
    public Image colorPreview;
    public Sprite[] crosshairs;

    public void SetWhite()
    {
        SetColor(new Color(1f, 1f, 1f, 1f));
    }

    public void SetRed()
    {
        SetColor(new Color(1f, 0f, 0f, 1f));
    }

    public void SetGreen()
    {
        SetColor(new Color(0f, 1f, 0f, 1f));
    }

    public void SetBlue()
    {
        SetColor(new Color(0f, 0f, 1f, 1f));
    }

    void SetColor(Color newColor)
    {
        PlayerPrefs.SetFloat("CrosshairR", newColor.r);
        PlayerPrefs.SetFloat("CrosshairG", newColor.g);
        PlayerPrefs.SetFloat("CrosshairB", newColor.b);
        PlayerPrefs.SetFloat("CrosshairA", newColor.a);
        PlayerPrefs.Save();
        //Debug.Log("Saving color: " + newColor);

        int index = PlayerPrefs.GetInt("CrosshairIndex", 0);

        if (colorPreview != null && crosshairs.Length > index)
        {
            colorPreview.sprite = crosshairs[index];
            colorPreview.color = newColor;
        }
    }

    public void SyncPreview()
    {
        int index = PlayerPrefs.GetInt("CrosshairIndex", 0);

        float r = PlayerPrefs.GetFloat("CrosshairR", 1f);
        float g = PlayerPrefs.GetFloat("CrosshairG", 1f);
        float b = PlayerPrefs.GetFloat("CrosshairB", 1f);
        float a = PlayerPrefs.GetFloat("CrosshairA", 1f);
        Color currentColor = new Color(r, g, b, a);

        if (colorPreview != null && crosshairs.Length > index)
        {
            colorPreview.sprite = crosshairs[index];
            colorPreview.color = currentColor;
        }
    }
}
