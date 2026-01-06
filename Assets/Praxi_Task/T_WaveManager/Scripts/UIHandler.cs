using UnityEngine;
using TMPro;

public class UIHandler : MonoBehaviour
{

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI waveNumberText;
    [SerializeField] private TextMeshProUGUI enemyCountText;
    [SerializeField] private TextMeshProUGUI fpsText;

    [Header("FPS Settings")]
    [SerializeField] private float fpsUpdateInterval = 0.5f;

    // FPS calculation
    private float fpsTimer;
    private int frameCount;
    private float fps;

    void OnEnable()
    {
        // Subscribe to events
        WaveManager.OnWaveChanged += UpdateWaveNumber;
        SpawnHandler.OnEnemyCountChanged += UpdateEnemyCount;
    }

    void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        WaveManager.OnWaveChanged -= UpdateWaveNumber;
        SpawnHandler.OnEnemyCountChanged -= UpdateEnemyCount;
    }

    void Start()
    {
        // Initialize UI with current values
        if (WaveManager.Instance != null)
        {
            UpdateWaveNumber(WaveManager.Instance.GetCurrentWaveNumber());
        }

        if (SpawnHandler.Instance != null)
        {
            UpdateEnemyCount(SpawnHandler.Instance.GetLivingEnemyCount());
        }
    }

    void Update()
    {
        UpdateFPS();
    }



    private void UpdateWaveNumber(int waveNumber)
    {
        if (waveNumberText != null)
        {
            waveNumberText.text = $"Wave: {waveNumber}";
        }
    }

    private void UpdateEnemyCount(int enemyCount)
    {
        if (enemyCountText != null)
        {
            enemyCountText.text = $"Enemies: {enemyCount}";
        }
    }


    private void UpdateFPS()
    {
        if (fpsText == null) return;

        frameCount++;
        fpsTimer += Time.unscaledDeltaTime;

        if (fpsTimer >= fpsUpdateInterval)
        {
            fps = frameCount / fpsTimer;
            frameCount = 0;
            fpsTimer = 0f;

            string color = GetFPSColor(fps);
            fpsText.text = $"<color={color}>FPS: {Mathf.RoundToInt(fps)}</color>";
        }
    }

    private string GetFPSColor(float currentFPS)
    {
        if (currentFPS >= 60f)
            return "green";
        else if (currentFPS >= 30f)
            return "yellow";
        else
            return "red";
    }

}
