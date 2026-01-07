using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WM
{
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        [Header("Wave Data")]
        [SerializeField] private StageWaves wavesData;
        [SerializeField] private int waveIndex = 0;
        private WaveData currentWaveData;

        [Header("Spawn Settings")]
        [SerializeField] private List<Transform> spawnPoints;
        [SerializeField] private Transform enemyParent;
        [SerializeField] private float groupRadius = 2f;

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
            StartWave();
        }

        public void NextWave()
        {
            waveIndex++;
            StartWave();
        }
      
        public void StartWave()
        {
            if (wavesData == null || waveIndex >= wavesData.WavesData.Count)
                return;

            currentWaveData = wavesData.WavesData[waveIndex];

            OnWaveChanged?.Invoke(waveIndex + 1);

            // Call SpawnHandler to spawn
            if (SpawnHandler.Instance != null)
            {
                SpawnHandler.Instance.SpawnWave(wavesData, currentWaveData, spawnPoints, enemyParent, groupRadius);
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
            return waveIndex + 1;
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