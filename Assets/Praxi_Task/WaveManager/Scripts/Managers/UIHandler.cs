using UnityEngine;
using TMPro;
using UnityEngine.UI;
namespace WM
{
    public class UIHandler : MonoBehaviour
    {
        [Header("UI Buttons References")]
        [SerializeField] private Button destroy_Btn;
        [SerializeField] private Button play_Pause_Btn;
        [SerializeField] private Button next_Wave_Btn;
        [SerializeField] private GameObject timer;

        [Header("UI Text References")]
        [SerializeField] private TextMeshProUGUI waveNumberText;
        [SerializeField] private TextMeshProUGUI enemyCountText;
        [SerializeField] private TextMeshProUGUI fpsText;
        [SerializeField] private TextMeshProUGUI countdownText;

        [Header("Button Text References")]
        [SerializeField] private TextMeshProUGUI play_Pause_Btn_Text;

        [Header("FPS Settings")]
        [SerializeField] private float updateRate = 0.5f;
        private float timerScale;
        private int frameCount;


        private int lastCountdownValue = -1;

        void OnEnable()
        {
            WaveManager.OnWaveChanged += UpdateWaveNumber;
            SpawnHandler.OnEnemyCountChanged += UpdateEnemyCount;
            WaveManager.OnCountdownUpdate += UpdateCountdown; 

            destroy_Btn.onClick.AddListener(OnDestroyWaveClicked);
            play_Pause_Btn.onClick.AddListener(OnPlay_PauseClicked);
            next_Wave_Btn.onClick.AddListener(OnNextWaveClicked);


        }

        void OnDisable()
        {
            WaveManager.OnWaveChanged -= UpdateWaveNumber;
            SpawnHandler.OnEnemyCountChanged -= UpdateEnemyCount;
            WaveManager.OnCountdownUpdate -= UpdateCountdown; 

            destroy_Btn.onClick.RemoveListener(OnDestroyWaveClicked);
            play_Pause_Btn.onClick.RemoveListener(OnPlay_PauseClicked);
            next_Wave_Btn.onClick.RemoveListener(OnNextWaveClicked);


        }

        void Start()
        {
            if (WaveManager.Instance != null)
            {
                UpdateWaveNumber(WaveManager.Instance.GetCurrentWaveNumber());
            }

            if (SpawnHandler.Instance != null)
            {
                UpdateEnemyCount(SpawnHandler.Instance.GetLivingEnemyCount());
            }

            // Initialize button text
            UpdatePlayPauseButtonText();
        }

        void Update()
        {
            UpdateFPS();
        }


        private void OnDestroyWaveClicked()
        {
            if (SpawnHandler.Instance != null)
            {
                SpawnHandler.Instance.KillAllEnemies();
                UpdateEnemyCount(0);
                WaveManager.Instance.OnWaveComplete();
            }
        }


        void OnNextWaveClicked()
        {
            timer.SetActive(false);
            play_Pause_Btn.interactable = false;

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.ForceNextWave();
            }
        }

        void OnPlay_PauseClicked()
        {
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.TogglePauseWaves();

                UpdatePlayPauseButtonText();

                UpdateTimerDisplay();

            }
        }


        private void UpdatePlayPauseButtonText()
        {
            if (play_Pause_Btn_Text == null)
            {
                return;
            }

            if (WaveManager.Instance == null) return;

            if (WaveManager.Instance.GameStops)
            {
                play_Pause_Btn_Text.text = "Resume";
            }
            else
            {
                play_Pause_Btn_Text.text = "Pause";
            }
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
                enemyCountText.text = $"{enemyCount} Monster";
            }
        }

        private void UpdateFPS()
        {
            frameCount++;
            timerScale += Time.unscaledDeltaTime;

            if (timerScale >= updateRate)
            {
                float fps = frameCount / timerScale;
                string color = GetFPSColor(fps);
                fpsText.text = $"<color={color}>FPS: {Mathf.RoundToInt(fps)}</color>";

                frameCount = 0;
                timerScale = 0f;
            }
            

        }

        private string GetFPSColor(float currentFPS)
        {
            if (currentFPS >= 120f)
                return "green";
            else if (currentFPS < 120f && currentFPS > 60f)
                return "yellow";
            else
                return "red";
            
        }
        
        private void UpdateCountdown(int seconds)
        {
            if (countdownText == null || timer == null) return;

            if (seconds > 0)
            {
                // Show timer
                timer.SetActive(true);
                play_Pause_Btn.interactable = true;

                // Store countdown value
                lastCountdownValue = seconds;

                // Check pause state
                if (WaveManager.Instance != null && WaveManager.Instance.GameStops)
                {
                    // PAUSED state
                    countdownText.text = "PAUSED";
                    countdownText.color = Color.red;
                }
                else
                {
                    // Counting down
                    countdownText.text = $"Next Wave in: {seconds}s";
                    countdownText.color = Color.yellow;
                }
            }
            else
            {
                // Hide timer when countdown reaches 0
                timer.SetActive(false);
                play_Pause_Btn.interactable= false;
                lastCountdownValue = -1;
            }
        }

        private void UpdateTimerDisplay()
        {
            if (countdownText == null || timer == null) return;
            if (WaveManager.Instance == null) return;

            // If timer is active
            if (timer.activeSelf)
            {
                if (WaveManager.Instance.GameStops)
                {
                    // Show PAUSED
                    countdownText.text = "PAUSED";
                    countdownText.color = Color.red;
                }
                // If not paused and we have a valid countdown value
                else if (lastCountdownValue > 0)
                {
                    // Show countdown
                    countdownText.text = $"Next Wave in: {lastCountdownValue}s";
                    countdownText.color = Color.yellow;
                }
            }
        }

    }
}