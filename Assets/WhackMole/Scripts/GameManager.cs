using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private const int SecondsPerMinute = 60;

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 30f;

    [Header("UI References")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;

    [Header("Moles")]
    [SerializeField] private MoleController molePrefab;
    [SerializeField] private Transform[]    moleBases;

    [Header("Result UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text   resultText;

    private MoleController[] moles;
    private Camera           mainCamera;
    private int              score;
    private float            timeRemaining;
    public  bool             IsGameRunning { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        mainCamera    = Camera.main;
        score         = 0;
        timeRemaining = gameDuration;
        IsGameRunning = true;
        if (resultPanel != null) resultPanel.SetActive(false);
        UpdateScoreUI();
        UpdateTimerUI();
        SpawnMoles();
        foreach (var mole in moles) mole.StartMole();
    }

    private void SpawnMoles()
    {
        moles = new MoleController[moleBases.Length];
        for (int i = 0; i < moleBases.Length; i++)
        {
            // 土台のスケールを継承して上下動が潰れないよう、子にせずワールド座標で生成する
            Transform spawnPoint = moleBases[i];
            moles[i] = Instantiate(molePrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }

    private void Update()
    {
        if (!IsGameRunning) return;

        HandleClick();

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            GameOver();
        }
        UpdateTimerUI();
    }

    // クリック位置にレイを飛ばし、当たったモグラを叩く（新 Input System 用）
    private void HandleClick()
    {
        if (mainCamera == null || Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit) &&
            hit.collider.TryGetComponent(out MoleController mole))
        {
            mole.TryHit();
        }
    }

    private void GameOver()
    {
        IsGameRunning = false;
        foreach (var mole in moles) mole.StopMole();

        if (resultPanel != null) resultPanel.SetActive(true);
        if (resultText  != null) resultText.text = $"Finish!\nScore: {score:D2}";
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void AddScore()
    {
        if (!IsGameRunning) return;
        score++;
        UpdateScoreUI();
    }

    private void UpdateScoreUI() => scoreText.text = $"Score:{score:D2}";

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / SecondsPerMinute);
        int seconds = Mathf.FloorToInt(timeRemaining % SecondsPerMinute);
        timerText.text = $"Last Time: {minutes:D2}:{seconds:D2}";
    }
}
