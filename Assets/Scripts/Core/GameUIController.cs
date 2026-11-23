
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using ForestAdventure;

public class GameUIController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject startPanel;
    public GameObject hudPanel;
    public GameObject pausePanel;
    public GameObject resultPanel;

    [Header("Buttons")]
    public Button startButton;
    public Button pauseButton;
    public Button resumeButton;
    public Button restartButton;

    [Header("HUD Text")]
    public TMP_Text timerText;
    public TMP_Text phaseText;
    public TMP_Text killsText;
    public TMP_Text gemsText;

    [Header("Result Text")]
    public TMP_Text resultTitle;
    public TMP_Text resultStats;

    [Header("Player")]
    public PlayerController playerController;
    public PlayerCombat playerCombat;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioSource musicSource;
    public AudioClip backgroundMusic;
    public AudioClip clickSound;
    public AudioClip startSound;
    public AudioClip pauseSound;
    public AudioClip winSound;
    public AudioClip loseSound;

    private GameManager gm;
    private bool started;
    private bool paused;
    private bool finished;

    private Color normalTimeColor = Color.white;
    private Color warningTimeColor = Color.red;

    private void Awake()
    {
        // 進入場景先停止遊戲時間
        Time.timeScale = 0f;
    }

    private void Start()
    {
        gm = GameManager.Instance;

        if (gm == null)
        {
            Debug.LogError(
                "[GameUI] 找不到 GameManager！");
            return;
        }

        gm.PhaseChanged += OnPhaseChanged;

        // 自動連接按鈕事件
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseGame);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (playerController != null)
            playerController.enabled = false;

        if (playerCombat != null)
            playerCombat.enabled = false;

        ShowPanels(true, false, false, false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (musicSource != null &&
            backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        UpdateHUD();
    }

    private void Update()
    {
        if (gm == null) return;

        if (started && !finished &&
            Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused)
                ResumeGame();
            else
                PauseGame();
        }

        UpdateHUD();
    }

    private void UpdateHUD()
    {
        if (gm == null) return;

        int seconds = Mathf.CeilToInt(gm.TimeLeft);
        int minutes = seconds / 60;
        int remainder = seconds % 60;

        if (timerText != null)
        {
            timerText.text =
                $"{minutes:00}:{remainder:00}";

            timerText.color =
                gm.TimeLeft <= 10f
                ? warningTimeColor
                : normalTimeColor;
        }

        if (phaseText != null)
        {
            switch (gm.Phase)
            {
                case GamePhase.Explore:
                    phaseText.text = "第一關：森林探索";
                    break;
                case GamePhase.Hunting:
                    phaseText.text = "第二關：動物狩獵";
                    break;
                case GamePhase.Boss:
                    phaseText.text = "第三關：熊 Boss";
                    break;
                case GamePhase.Won:
                    phaseText.text = "挑戰成功";
                    break;
                case GamePhase.Lost:
                    phaseText.text = "挑戰失敗";
                    break;
            }
        }

        if (killsText != null)
            killsText.text =
                $"擊殺：{gm.Kills}/{gm.requiredKills}";

        if (gemsText != null)
            gemsText.text =
                $"寶石：{gm.GemsCollectedTotal}/" +
                gm.requiredCollectedGems;
    }

    public void StartGame()
    {
        if (gm == null || started) return;

        started = true;
        paused = false;
        finished = false;

        Time.timeScale = 1f;

        SetPlayerControl(true);
        ShowPanels(false, true, false, false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PlaySound(startSound);

        Debug.Log("[GameUI] 遊戲開始");
    }

    public void PauseGame()
    {
        if (!started || paused || finished)
            return;

        paused = true;
        Time.timeScale = 0f;

        SetPlayerControl(false);
        ShowPanels(false, false, true, false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PlaySound(pauseSound);
    }

    public void ResumeGame()
    {
        if (!paused || finished) return;

        paused = false;
        Time.timeScale = 1f;

        SetPlayerControl(true);
        ShowPanels(false, true, false, false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PlaySound(clickSound);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        if (gm != null)
            gm.ResetRun();

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    private void OnPhaseChanged(GamePhase phase)
    {
        if (phase == GamePhase.Won)
            ShowResult(true);

        else if (phase == GamePhase.Lost)
            ShowResult(false);
    }

    private void ShowResult(bool won)
    {
        finished = true;
        paused = false;

        Time.timeScale = 0f;
        SetPlayerControl(false);

        ShowPanels(false, false, false, true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (resultTitle != null)
            resultTitle.text =
                won ? "挑戰成功！" : "挑戰失敗！";

        if (resultStats != null)
        {
            resultStats.text =
                $"擊敗動物：{gm.Kills}\n" +
                $"收集寶石：{gm.GemsCollectedTotal}\n" +
                $"最終關卡：{gm.LastPlayablePhase}";
        }

        if (musicSource != null)
            musicSource.Stop();

        PlaySound(won ? winSound : loseSound);

        Debug.Log(
            won ? "[GameUI] WIN" : "[GameUI] LOST"
        );
    }

    private void SetPlayerControl(bool enabled)
    {
        if (playerController != null)
            playerController.enabled = enabled;

        if (playerCombat != null)
            playerCombat.enabled = enabled;
    }

    private void ShowPanels(
        bool showStart,
        bool showHUD,
        bool showPause,
        bool showResult)
    {
        if (startPanel != null)
            startPanel.SetActive(showStart);

        if (hudPanel != null)
            hudPanel.SetActive(showHUD);

        if (pausePanel != null)
            pausePanel.SetActive(showPause);

        if (resultPanel != null)
            resultPanel.SetActive(showResult);
    }

    private void PlaySound(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }

    private void OnDestroy()
    {
        if (gm != null)
            gm.PhaseChanged -= OnPhaseChanged;

        Time.timeScale = 1f;
    }
}
