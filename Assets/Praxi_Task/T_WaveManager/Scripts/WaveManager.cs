using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private Coroutine waveDelayCoroutine;


    
    public static event Action<int> OnWaveChanged;


    // Enemy prefab lists
    private List<GameObject> MeleeEnemy = new List<GameObject>();
    private List<GameObject> RangedEnemy = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ===== TEST ONLY =====
    private void Start()
    {
        StartWave();
    }

    public void NextWave()
    {
        waveIndex++;
        StartWave();
    }
    public int GetCurrentWaveNumber()
    {
        return waveIndex + 1;
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
        float countdown = 5f; // 5 seconds


        while (countdown > 0)
        {
            if (GameStops)
            {
                yield return null; // Wait a frame
                continue; // Don't decrease countdown
            }

            // Countdown continues
            countdown -= Time.deltaTime;

            yield return null;
        }

        NextWave();
    }

}