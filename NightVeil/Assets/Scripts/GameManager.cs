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
    public TextMeshProUGUI finalTimeText;
    public TextMeshProUGUI finalPointsText;
    public TextMeshProUGUI finalWaveText;

    [Header("Configurações")]
    public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityValueText;

    [Header("Referências do Player")]
    public GameObject playerObject;
    public MonoBehaviour playerMovementScript;
    public MonoBehaviour playerCameraScript;

    public bool IsPaused { get; private set; } = false;
    public bool IsPlaying { get; private set; } = false;
    public float Sensitivity { get; private set; } = 300f;

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
            Debug.Log("✅ GameManager instanciado como Singleton");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        Debug.Log("GameManager: Iniciando menu principal");

        IsPlaying = false;
        IsPaused = false;

        mainMenu.SetActive(true);
        pauseMenu.SetActive(false);
        hud.SetActive(false);
        settingsMenu.SetActive(false);
        exitConfirmPanel.SetActive(false);
        gameOverPanel.SetActive(false);

        Time.timeScale = 1f;

        LoadSettings();

        SetCursorState(false);

        SetPlayerControls(false);

        UpdateHUD();
    }

    void Update()
    {
        if (IsPlaying && !IsPaused)
        {
            survivalTime += Time.deltaTime;
            UpdateTimerUI();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && IsPlaying)
        {
            if (IsPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void StartGame()
    {
        Debug.Log("GameManager: Iniciando jogo");

        IsPlaying = true;
        IsPaused = false;

        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
        exitConfirmPanel.SetActive(false);
        hud.SetActive(true);
        gameOverPanel.SetActive(false);

        Time.timeScale = 1f;

        survivalTime = 0f;
        currentWave = 1;
        playerPoints = 0;
        playerHealth = 100;
        currentAmmo = 30;
        maxAmmo = 120;

        UpdateHUD();

        SetPlayerControls(true);

        StartCoroutine(DelayedCursorLock());
    }

    private IEnumerator DelayedCursorLock()
    {
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
        SetPlayerControls(false);
        SetCursorState(false);
    }

    public void GameOverToMenu()
    {
        Debug.Log("GameManager: Game Over -> Menu");
        ReturnToMainMenu();
    }

    private void SetPlayerControls(bool enabled)
    {
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = enabled;
        }
        else
        {
            Debug.LogWarning("⚠️ Script de movimento do player não atribuído no GameManager");
        }

        if (playerCameraScript != null)
        {
            playerCameraScript.enabled = enabled;
        }
        else
        {
            Debug.LogWarning("⚠️ Script de câmera do player não atribuído no GameManager");
        }
    }

    public void UpdateHUD()
    {
        UpdateTimerUI();

        if (waveValueText != null)
            waveValueText.text = currentWave.ToString("00");

        if (pointsValueText != null)
            pointsValueText.text = playerPoints.ToString("0000");

        if (ammoText != null)
            ammoText.text = $"{currentAmmo}/{maxAmmo}";

        if (healthValueText != null)
            healthValueText.text = playerHealth.ToString();

        if (healthBar != null)
        {
            healthBar.value = playerHealth;
            healthBar.maxValue = 100;
        }
    }

    public void AddPoints(int points)
    {
        playerPoints += points;
        UpdateHUD();
        Debug.Log($"GameManager: +{points} pontos! Total: {playerPoints}");
    }

    public void PlayerTakeDamage(int damage)
    {
        playerHealth -= damage;

        Debug.Log($"GameManager: Jogador tomou {damage} de dano! Vida: {playerHealth}/100");

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

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return $"{minutes:00}:{seconds:00}";
    }

    private void GameOver()
    {
        Debug.Log("💀 Game Over!");
        IsPlaying = false;

        hud.SetActive(false);
        gameOverPanel.SetActive(true);

        if (finalTimeText != null)
            finalTimeText.text = $"TEMPO: {FormatTime(survivalTime)}";

        if (finalPointsText != null)
            finalPointsText.text = $"PONTOS: {playerPoints:0000}";

        if (finalWaveText != null)
            finalWaveText.text = $"WAVE: {currentWave:00}";

        SetPlayerControls(false);
        SetCursorState(false);

        Time.timeScale = 0f;
    }

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
        SaveSettings();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void LoadSettings()
    {
        Sensitivity = PlayerPrefs.GetFloat("sensitivity", 300f);

        if (sensitivitySlider != null)
            sensitivitySlider.value = Sensitivity;

        if (sensitivityValueText != null)
        {
            sensitivityValueText.text = Sensitivity.ToString("F1");
        }
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("sensitivity", Sensitivity);
        PlayerPrefs.Save();
    }

    public void ChangeSensitivity(float value)
    {
        Sensitivity = value;
        PlayerPrefs.SetFloat("sensitivity", value);
        if (sensitivityValueText != null)
        {
            sensitivityValueText.text = value.ToString("F1");
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = FormatTime(survivalTime);
        }
    }

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

    public float GetGameTime() => survivalTime;
    public int GetCurrentWave() => currentWave;
    public int GetPlayerHealth() => playerHealth;

    public void ResetGame()
    {
        survivalTime = 0f;
        UpdateTimerUI();
    }
}