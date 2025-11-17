using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Panels")]
    public GameObject mainMenu;
    public GameObject pauseMenu;
    public GameObject hud;
    public GameObject settingsMenu;
    public GameObject exitConfirmPanel;

    [Header("HUD")]
    public TextMeshProUGUI timerText;

    [Header("Configurações")]
    public Slider sensitivitySlider;
    public Slider volumeSlider;

    // -------------------------
    // VARIÁVEIS DE ESTADO
    // -------------------------
    public bool IsPaused { get; private set; } = false;
    public bool IsPlaying { get; private set; } = false;

    public float Sensitivity { get; private set; } = 300f; // sensibilidade padrão
    public float Volume { get; private set; } = 1f;

    private float survivalTime = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Start()
    {
        // Estado inicial do jogo
        Time.timeScale = 0f;
        mainMenu.SetActive(true);

        pauseMenu.SetActive(false);
        hud.SetActive(false);
        settingsMenu.SetActive(false);
        exitConfirmPanel.SetActive(false);

        // Carrega configurações
        Sensitivity = PlayerPrefs.GetFloat("sensitivity", 300f);
        Volume = PlayerPrefs.GetFloat("volume", 1f);

        sensitivitySlider.value = Sensitivity;
        volumeSlider.value = Volume;
    }

    void Update()
    {
        if (IsPlaying && !IsPaused)
        {
            survivalTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    // --------------------------------
    // ------- CONTROLE DO JOGO -------
    // --------------------------------

    public void StartGame()
    {
        IsPlaying = true;
        IsPaused = false;

        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
        exitConfirmPanel.SetActive(false);

        hud.SetActive(true);

        Time.timeScale = 1f;
        LockCursor();
    }

    public void PauseGame()
    {
        IsPaused = true;

        pauseMenu.SetActive(true);
        settingsMenu.SetActive(false);
        exitConfirmPanel.SetActive(false);

        Time.timeScale = 0f;
        UnlockCursor();
    }

    public void ResumeGame()
    {
        IsPaused = false;

        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
        exitConfirmPanel.SetActive(false);

        Time.timeScale = 1f;
        LockCursor();
    }

    public void OpenSettings()
    {
        settingsMenu.SetActive(true);
        pauseMenu.SetActive(false);
        mainMenu.SetActive(false);
    }

    public void CloseSettings()
    {
        if (!IsPlaying)
        {
            mainMenu.SetActive(true);
        }
        else
        {
            pauseMenu.SetActive(true);
        }

        settingsMenu.SetActive(false);
    }

    public void OpenExitConfirm()
    {
        exitConfirmPanel.SetActive(true);
        pauseMenu.SetActive(false);
        mainMenu.SetActive(false);
    }

    public void CloseExitConfirm()
    {
        exitConfirmPanel.SetActive(false);

        if (!IsPlaying)
            mainMenu.SetActive(true);
        else
            pauseMenu.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("SAINDO DO JOGO...");
    }

    // -------------------------------
    // --------- SLIDERS -------------
    // -------------------------------
    public void ChangeSensitivity(float value)
    {
        Sensitivity = value;
        PlayerPrefs.SetFloat("sensitivity", value);
    }

    public void ChangeVolume(float value)
    {
        Volume = value;
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("volume", value);
    }

    // -------------------------------
    // --------- TIMER ---------------
    // -------------------------------
    void UpdateTimerUI()
    {
        int min = Mathf.FloorToInt(survivalTime / 60);
        int sec = Mathf.FloorToInt(survivalTime % 60);

        timerText.text = $"{min:00}:{sec:00}";
    }

    // -------------------------------
    // -------- CURSOR ---------------
    // -------------------------------
    void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
