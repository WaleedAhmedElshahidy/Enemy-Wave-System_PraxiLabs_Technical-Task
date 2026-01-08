using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;

namespace WM
{
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        [Header("Wave Data")]
        [SerializeField] private StageWaves waveDataObj;
        [SerializeField] private int waveNumber = 1;



        [Header("Wave Control")]
        public bool GameStops = false;
        public bool isPaused = false;
        private Coroutine waveDelayCoroutine;

        public static event Action<int> OnWaveChanged;
        public static event Action<int> OnCountdownUpdate;



        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        private void Start()
        {
            StartWave(waveNumber);
        }
        [Button]
        public int GenWaveCount(int waveNumber)
        {
            for (int i = 0; i < waveDataObj.wavesData.Count; i++)
            {
                if (waveNumber < waveDataObj.wavesData[i].wavesEffectedCount + waveDataObj.wavesData[i].waveGroupStart || waveDataObj.wavesData[i].isInfinite)
                {
                    return waveDataObj.wavesData[i].startEnemiesCount + (waveNumber - waveDataObj.wavesData[i].waveGroupStart + 1) * waveDataObj.wavesData[i].addedPerWave;
                }
            }
            return -1;
        }
        public void NextWave()
        {
            waveNumber++;
            StartWave(waveNumber);
        }

        public void StartWave(int waveNumber)
        {
            if (waveDataObj == null)
                return;

            int enemyToSpownCount = GenWaveCount(waveNumber);





        OnWaveChanged?.Invoke(this.waveNumber);

            // Call SpawnHandler to spawn
            if (SpawnHandler.Instance != null)
            {
                SpawnHandler.Instance.SpawnWave(waveDataObj, enemyToSpownCount);
            }
        }
        public void OnWaveComplete()
        {
            // Start countdown to next wave
            if (waveDelayCoroutine != null)
            {
                StopCoroutine(waveDelayCoroutine); // Stop old countdown if exists
            }

            waveDelayCoroutine = StartCoroutine(WaveDelayCountdown());
        }

        private IEnumerator WaveDelayCountdown()
        {
            float countdown = 5f;
            int lastSecond = 5;

            // Fire initial countdown
            OnCountdownUpdate?.Invoke(5);

            while (countdown > 0)
            {
                if (GameStops)
                {
                    // Just keep firing current value while paused (UI will detect no change)
                    yield return null;
                    continue;
                }

                // Countdown continues
                countdown -= Time.deltaTime;

                // Update UI when second changes
                int currentSecond = Mathf.CeilToInt(countdown);
                if (currentSecond != lastSecond && currentSecond >= 0)
                {
                    lastSecond = currentSecond;
                    OnCountdownUpdate?.Invoke(currentSecond);
                }

                yield return null;
            }

            // Countdown complete - send 0 to hide UI
            OnCountdownUpdate?.Invoke(0);
            NextWave();
        }
        public void TogglePauseWaves()
        {
            GameStops = !GameStops;
        }

        public void ForceNextWave()
        {

            // Cancel countdown if running
            if (waveDelayCoroutine != null)
            {
                StopCoroutine(waveDelayCoroutine);
                waveDelayCoroutine = null;
            }

            // Start next wave NOW
            NextWave();
        }
        public int GetCurrentWaveNumber()
        {
            return waveNumber;
        }
        public void ResetAllDefenders()
        {
            // Reset all defense  (find by tag "Target")
            GameObject[] targetObjects = GameObject.FindGameObjectsWithTag("Target");

            foreach (GameObject targetObj in targetObjects)
            {

                if (targetObj == null || !targetObj.activeInHierarchy)
                    continue;

                Target target = targetObj.GetComponent<Target>();
                if (target != null && !target.IsDestroyed)
                {
                    target.ResetTarget();
                }
            }


        }

    }
}