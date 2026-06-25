using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;
    public GameObject BackgroundOverlay;

    public static PauseMenu Instance;

    public Crosshair crosshair;

    public bool isPaused = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Debug.Log("Esc gedruekt");
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        bool isCardSelectionOpen = UpgradeCardManager.Instance != null && UpgradeCardManager.Instance.IsCardSelectionOpen;

        pauseMenuPanel.SetActive(true);

        if (!isCardSelectionOpen)
        {
            BackgroundOverlay.SetActive(true);
        }

        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);

        bool isCardSelectionOpen = UpgradeCardManager.Instance != null && UpgradeCardManager.Instance.IsCardSelectionOpen;

        if (!isCardSelectionOpen)
        {
            BackgroundOverlay.SetActive(false);
        }

        Time.timeScale = 1f;
        isPaused = false;

        if (UpgradeCardManager.Instance != null && UpgradeCardManager.Instance.IsCardSelectionOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (crosshair != null)
            crosshair.ApplySettings();
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScene");
    }
}
