using UnityEngine;
using UnityEngine.UI;

public class CrosshairSelector : MonoBehaviour
{
    public Image preview;  
    public Sprite[] crosshairs;
    public Crosshair crosshair; 

    void Start()
    {
        if (crosshair == null)
        {
            crosshair = FindFirstObjectByType<Crosshair>();
        }

        InitPreview();
    }

    void InitPreview()
    {
        int index = PlayerPrefs.GetInt("CrosshairIndex", 0);

        if (preview != null && crosshairs.Length > index)
        {
            preview.sprite = crosshairs[index]; 
        }
    }

    public void SelectCrosshair(int index)
    {
        if (preview != null)
            preview.sprite = crosshairs[index]; 

        PlayerPrefs.SetInt("CrosshairIndex", index);
        PlayerPrefs.Save();

        if (crosshair != null)
            crosshair.ApplySettings();     
    }
}