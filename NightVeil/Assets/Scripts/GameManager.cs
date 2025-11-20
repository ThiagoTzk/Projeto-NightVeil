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
    public GameObject gameOverPanel;

    [Header("HUD - Textos Dinâmicos (APENAS NÚMEROS)")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI waveValueText;
    public TextMeshProUGUI pointsValueText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI healthValueText;
    public Slider healthBar;

    [Header("Game Over UI")]
    public TextMeshProUGUI finalTimeText;    // NOVO: Tempo de sobrevivência
    public TextMeshProUGUI finalPointsText;  // NOVO: Pontos finais
    public TextMeshProUGUI finalWaveText;    // Wave final

    [Header("Configurações")]
    public Slider sensitivitySlider;
    public Slider volumeSlider;
    public TextMeshProUGUI sensitivityValueText;

    [Header("Referências do Player")]
    public GameObject playerObject;
    public MonoBehaviour playerMovementScript;
    public MonoBehaviour playerCameraScript;

    public bool IsPaused { get; private set; } = false;
    public bool IsPlaying { get; private set; } = false;
    public float Sensitivity { get; private set; } = 300f;
    public float Volume { get; private set; } = 1f;

    private float survivalTime = 0f;
    private int currentWave = 1;
    private int playerPoints = 0;
    private int playerHealth = 100;
    private int currentAmmo = 30;
    private int maxAmmo = 120;

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
        gameOverPanel.SetActive(false);

        Time.timeScale = 1f;

        // Carrega configurações salvas
        LoadSettings();

        // Cursor livre no menu
        SetCursorState(false);

        // Desativa controles do jogador no menu
        SetPlayerControls(false);

        // Inicializa HUD
        UpdateHUD();
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
        gameOverPanel.SetActive(false);

        Time.timeScale = 1f;

        // Reseta stats do jogo
        survivalTime = 0f;
        currentWave = 1;
        playerPoints = 0;
        playerHealth = 100;
        currentAmmo = 30;
        maxAmmo = 120;

        UpdateHUD();

        // Ativa controles do jogador
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

        // Desativa controles durante pausa
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

        // Reativa controles
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
        gameOverPanel.SetActive(false);

        Time.timeScale = 1f;

        // Desativa controles no menu
        SetPlayerControls(false);
        SetCursorState(false);
    }

    public void GameOverToMenu()
    {
        Debug.Log("GameManager: Game Over -> Menu");
        ReturnToMainMenu();
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
    // ---------- HUD SYSTEM ---------
    // -------------------------------

    public void UpdateHUD()
    {
        // Timer (mantém formato original)
        UpdateTimerUI();

        // Wave - APENAS NÚMERO (ex: "01")
        if (waveValueText != null)
            waveValueText.text = currentWave.ToString("00");

        // Points - APENAS NÚMERO (ex: "0000")
        if (pointsValueText != null)
            pointsValueText.text = playerPoints.ToString("0000");

        // Ammo - mantém formato original
        if (ammoText != null)
            ammoText.text = $"{currentAmmo}/{maxAmmo}";

        // Health - APENAS NÚMERO (ex: "100")
        if (healthValueText != null)
            healthValueText.text = playerHealth.ToString();

        // Health Bar
        if (healthBar != null)
        {
            healthBar.value = playerHealth;
            healthBar.maxValue = 100;
        }
    }

    public void NextWave()
    {
        currentWave++;
        UpdateHUD();
        Debug.Log($"Iniciando Wave {currentWave}");
    }

    public void AddPoints(int points)
    {
        playerPoints += points;
        UpdateHUD();
    }

    public void PlayerTakeDamage(int damage)
    {
        playerHealth -= damage;
        if (playerHealth <= 0)
        {
            playerHealth = 0;
            GameOver();
        }
        UpdateHUD();
    }

    public void UpdateAmmo(int current, int max)
    {
        currentAmmo = current;
        maxAmmo = max;
        UpdateHUD();
    }

    // NOVO: Método para formatar o tempo em minutos e segundos
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return $"{minutes:00}:{seconds:00}";
    }

    // Método Game Over atualizado com tempo e pontos
    private void GameOver()
    {
        Debug.Log("Game Over!");

        IsPlaying = false;

        // Desativa HUD e ativa tela de Game Over
        hud.SetActive(false);
        gameOverPanel.SetActive(true);

        // Atualiza estatísticas finais
        if (finalTimeText != null)
            finalTimeText.text = $"TEMPO: {FormatTime(survivalTime)}";

        if (finalPointsText != null)
            finalPointsText.text = $"PONTOS: {playerPoints:0000}";

        if (finalWaveText != null)
            finalWaveText.text = $"WAVE: {currentWave:00}";

        // Desativa controles do jogador
        SetPlayerControls(false);

        // Libera cursor
        SetCursorState(false);

        // Pausa o jogo (opcional)
        Time.timeScale = 0f;
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

        // Atualiza texto da sensibilidade
        if (sensitivityValueText != null)
        {
            sensitivityValueText.text = Sensitivity.ToString("F1");
        }

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

        // Atualiza texto da sensibilidade
        if (sensitivityValueText != null)
        {
            sensitivityValueText.text = value.ToString("F1");
        }
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
        if (timerText != null)
        {
            timerText.text = FormatTime(survivalTime);
        }
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

    public int GetCurrentWave()
    {
        return currentWave;
    }

    public int GetPlayerHealth()
    {
        return playerHealth;
    }

    public void ResetGame()
    {
        survivalTime = 0f;
        UpdateTimerUI();
    }
}