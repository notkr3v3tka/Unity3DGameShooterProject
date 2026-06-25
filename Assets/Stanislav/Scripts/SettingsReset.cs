using UnityEngine;

public class SettingsReset : MonoBehaviour
{
    public void ResetSettings()
    {
        PlayerPrefs.DeleteKey("CrosshairR");
        PlayerPrefs.DeleteKey("CrosshairG");
        PlayerPrefs.DeleteKey("CrosshairB");
        PlayerPrefs.DeleteKey("CrosshairA");
        PlayerPrefs.DeleteKey("CrosshairIndex");

        PlayerPrefs.DeleteKey("MouseSensitivity");

        PlayerPrefs.Save();

        Crosshair crosshair = FindFirstObjectByType<Crosshair>();
        if (crosshair != null)
            crosshair.ApplySettings();

        CrosshairSelector selector = FindFirstObjectByType<CrosshairSelector>();
        if (selector != null)
            selector.SelectCrosshair(0);

        MouseSettings mouse = FindFirstObjectByType<MouseSettings>();
        if (mouse != null)
            mouse.ResetSensitivity();

        //Debug.Log("Settings reset!");
    }
}
