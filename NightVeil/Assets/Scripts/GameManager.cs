using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

    [Header("Referências do Player")]
    public GameObject playerObject; // Arraste o GameObject do jogador aqui
    public MonoBehaviour playerMovementScript; // Script de movimento do jogador
    public MonoBehaviour playerCameraScript; // Script da câmera

    // -------------------------
    // VARIÁVEIS DE ESTADO
    // -------------------------
    public bool IsPaused { get; private set; } = false;
    public bool IsPlaying { get; private set; } = false;
    public float Sensitivity { get; private set; } = 300f;
    public float Volume { get; private set; } = 1f;

    private float survivalTime = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Estado inicial - Menu Principal
        Debug.Log("GameManager: Iniciando menu principal");

        IsPlaying = false;
        IsPaused = false;

        // Configura UI
        mainMenu.SetActive(true);
        pauseMenu.SetActive(false);
        hud.SetActive(false);
        settingsMenu.SetActive(false);
        exitConfirmPanel.SetActive(false);

        Time.timeScale = 1f;

        // Carrega configurações salvas
        LoadSettings();

        // Cursor livre no menu
        SetCursorState(false);

        // **CRUCIAL: Desativa controles do jogador no menu**
        SetPlayerControls(false);
    }

    void Update()
    {
        // Atualiza timer durante o jogo
        if (IsPlaying && !IsPaused)
        {
            survivalTime += Time.deltaTime;
            UpdateTimerUI();
        }

        // Controle de pausa com ESC
        if (Input.GetKeyDown(KeyCode.Escape) && IsPlaying)
        {
            if (IsPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    // --------------------------------
    // ------- CONTROLE DO JOGO -------
    // --------------------------------

    public void StartGame()
    {
        Debug.Log("GameManager: Iniciando jogo");

        IsPlaying = true;
        IsPaused = false;

        // Transição de UI
        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
        exitConfirmPanel.SetActive(false);
        hud.SetActive(true);

        Time.timeScale = 1f;

        // **CRUCIAL: Ativa controles do jogador**
        SetPlayerControls(true);

        // Delay para garantir que tudo está inicializado antes de travar cursor
        StartCoroutine(DelayedCursorLock());
    }

    private IEnumerator DelayedCursorLock()
    {
        // Espera até o final do frame atual
        yield return new WaitForEndOfFrame();

        Debug.Log("GameManager: Travando cursor para jogo");
        SetCursorState(true);
    }

    public void PauseGame()
    {
        Debug.Log("GameManager: Pausando jogo");

        IsPaused = true;
        pauseMenu.SetActive(true);
        hud.SetActive(false);
        Time.timeScale = 0f;

        // **CRUCIAL: Desativa controles durante pausa**
        SetPlayerControls(false);
        SetCursorState(false);
    }

    public void ResumeGame()
    {
        Debug.Log("GameManager: Retomando jogo");

        IsPaused = false;
        pauseMenu.SetActive(false);
        hud.SetActive(true);
        Time.timeScale = 1f;

        // **CRUCIAL: Reativa controles**
        SetPlayerControls(true);
        StartCoroutine(DelayedCursorLock());
    }

    public void ReturnToMainMenu()
    {
        Debug.Log("GameManager: Voltando ao menu principal");

        IsPlaying = false;
        IsPaused = false;
        survivalTime = 0f;

        mainMenu.SetActive(true);
        pauseMenu.SetActive(false);
        hud.SetActive(false);
        settingsMenu.SetActive(false);
        exitConfirmPanel.SetActive(false);

        Time.timeScale = 1f;

        // **CRUCIAL: Desativa controles no menu**
        SetPlayerControls(false);
        SetCursorState(false);
    }

    // -------------------------------
    // ----- CONTROLES DO PLAYER -----
    // -------------------------------

    private void SetPlayerControls(bool enabled)
    {
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = enabled;
            Debug.Log($"Controles de movimento: {enabled}");
        }
        else
        {
            Debug.LogWarning("Script de movimento do player não atribuído no GameManager");
        }

        if (playerCameraScript != null)
        {
            playerCameraScript.enabled = enabled;
            Debug.Log($"Controles de câmera: {enabled}");
        }
        else
        {
            Debug.LogWarning("Script de câmera do player não atribuído no GameManager");
        }
    }

    // -------------------------------
    // ---------- SETTINGS -----------
    // -------------------------------

    public void OpenSettings()
    {
        settingsMenu.SetActive(true);

        if (IsPlaying)
            pauseMenu.SetActive(false);
        else
            mainMenu.SetActive(false);
    }

    public void CloseSettings()
    {
        settingsMenu.SetActive(false);

        if (IsPlaying)
            pauseMenu.SetActive(true);
        else
            mainMenu.SetActive(true);
    }

    public void OpenExitConfirm()
    {
        exitConfirmPanel.SetActive(true);

        if (IsPlaying)
            pauseMenu.SetActive(false);
        else
            mainMenu.SetActive(false);
    }

    public void CloseExitConfirm()
    {
        exitConfirmPanel.SetActive(false);

        if (IsPlaying)
            pauseMenu.SetActive(true);
        else
            mainMenu.SetActive(true);
    }

    public void ExitGame()
    {
        Debug.Log("GameManager: Saindo do jogo");

        // Salva configurações antes de sair
        SaveSettings();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    // -------------------------------
    // --------- CONFIGURAÇÕES -------
    // -------------------------------

    private void LoadSettings()
    {
        Sensitivity = PlayerPrefs.GetFloat("sensitivity", 300f);
        Volume = PlayerPrefs.GetFloat("volume", 1f);

        if (sensitivitySlider != null)
            sensitivitySlider.value = Sensitivity;

        if (volumeSlider != null)
            volumeSlider.value = Volume;

        AudioListener.volume = Volume;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("sensitivity", Sensitivity);
        PlayerPrefs.SetFloat("volume", Volume);
        PlayerPrefs.Save();
    }

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

    private void SetCursorState(bool locked)
    {
        if (locked)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // -------------------------------
    // -------- GETTERS --------------
    // -------------------------------

    public float GetGameTime()
    {
        return survivalTime;
    }

    public void ResetGame()
    {
        survivalTime = 0f;
        UpdateTimerUI();
    }
}