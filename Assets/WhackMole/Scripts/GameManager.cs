using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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
    private Camera mainCamera;
    private int   score;
    private float timeRemaining;
    public  bool  IsGameRunning { get; private set; }

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
            // 土台の位置に生成するが、子にはしない。
            // 子にすると土台の非均一スケール(例:0.8,0.5,0.8)を継承して上下動が潰れ、
            // さらに土台のColliderがクリックを横取りしてOnMouseDownがモグラに届かないため。
            Transform baseT = moleBases[i];
            MoleController mole = Instantiate(molePrefab, baseT.position, baseT.rotation);
            moles[i] = mole;
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

    private void HandleClick()
    {
        // 新 Input System でマウス左クリックを検出し、カメラからレイを飛ばして当たったモグラを叩く。
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        // 土台など手前のColliderに遮られても拾えるよう、レイ上の全ヒットからモグラを探す。
        foreach (RaycastHit hit in Physics.RaycastAll(ray))
        {
            if (hit.collider.TryGetComponent(out MoleController mole))
            {
                mole.TryHit();
                break;
            }
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
        int m = Mathf.FloorToInt(timeRemaining / 60f);
        int s = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = $"Last Time: {m:D2}:{s:D2}";
    }
}
