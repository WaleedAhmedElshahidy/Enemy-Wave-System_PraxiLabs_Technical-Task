using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WM
{
    public class SpawnHandler : MonoBehaviour
    {
        public static SpawnHandler Instance { get; private set; }

        private List<GameObject> MeleeEnemy = new List<GameObject>();
        private List<GameObject> RangedEnemy = new List<GameObject>();

        [Header("Enemy Tracking")]
        [SerializeField] private List<GameObject> livingEnemies = new List<GameObject>();
        [SerializeField] private List<GameObject> diedMeleeEnemies = new List<GameObject>();
        [SerializeField] private List<GameObject> diedRangedEnemies = new List<GameObject>();

        public static event Action<int> OnEnemyCountChanged;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SpawnWave(StageWaves wavesData, WaveData currentWaveData, List<Transform> spawnPoints, Transform enemyParent, float groupRadius)
        {
            MeleeEnemy.Clear();
            RangedEnemy.Clear();

            foreach (Enemy enemy in wavesData.EnemyList)
            {
                if (enemy.enemyPrefab == null) continue;

                if (enemy.enemyType == EnemyType.Melee)
                {
                    MeleeEnemy.Add(enemy.enemyPrefab);
                }
                else if (enemy.enemyType == EnemyType.Ranged)
                {
                    RangedEnemy.Add(enemy.enemyPrefab);
                }
            }

            int totalEnemies = currentWaveData.enemyCount;
            int meleeCount = Mathf.RoundToInt(totalEnemies * currentWaveData.meleePercent / 100f);
            int rangedCount = totalEnemies - meleeCount;

            int enemiesPerPoint = totalEnemies / spawnPoints.Count;
            int remainder = totalEnemies % spawnPoints.Count;

            int meleeLeft = meleeCount;
            int rangedLeft = rangedCount;

            for (int i = 0; i < spawnPoints.Count; i++)
            {
                Transform center = spawnPoints[i];

                int countForThisPoint = enemiesPerPoint;
                if (remainder > 0)
                {
                    countForThisPoint++;
                    remainder--;
                }

                int meleeForGroup = Mathf.Min(
                    Mathf.RoundToInt(countForThisPoint * currentWaveData.meleePercent / 100f),
                    meleeLeft
                );
                int rangedForGroup = countForThisPoint - meleeForGroup;

                if (rangedForGroup > rangedLeft)
                {
                    rangedForGroup = rangedLeft;
                    meleeForGroup = countForThisPoint - rangedForGroup;
                }

                SpawnGroup(center, meleeForGroup, rangedForGroup, enemyParent, groupRadius);

                meleeLeft -= meleeForGroup;
                rangedLeft -= rangedForGroup;


            }
        }



        private void SpawnGroup(Transform center, int meleeCount, int rangedCount, Transform enemyParent, float groupRadius)
        {
            for (int i = 0; i < meleeCount; i++)
            {
                if (MeleeEnemy.Count == 0) continue;

                if (diedMeleeEnemies.Count > 0)
                {
                    GameObject pooledEnemy = diedMeleeEnemies[0];
                    diedMeleeEnemies.RemoveAt(0);
                    ReuseEnemy(pooledEnemy, center, groupRadius);
                }
                else
                {
                    GameObject prefab = MeleeEnemy[UnityEngine.Random.Range(0, MeleeEnemy.Count)];
                    SpawnEnemy(prefab, center, enemyParent, groupRadius);
                }
            }

            for (int i = 0; i < rangedCount; i++)
            {
                if (RangedEnemy.Count == 0) continue;

                if (diedRangedEnemies.Count > 0)
                {
                    GameObject pooledEnemy = diedRangedEnemies[0];
                    diedRangedEnemies.RemoveAt(0);
                    ReuseEnemy(pooledEnemy, center, groupRadius);
                }
                else
                {
                    GameObject prefab = RangedEnemy[UnityEngine.Random.Range(0, RangedEnemy.Count)];
                    SpawnEnemy(prefab, center, enemyParent, groupRadius);
                }
            }
        }

        private void ReuseEnemy(GameObject enemy, Transform center, float groupRadius)
        {
            Vector3 offset = UnityEngine.Random.insideUnitCircle * groupRadius;
            Vector3 spawnPos = center.position + new Vector3(offset.x, 0, offset.y);
            enemy.transform.position = spawnPos;

            EnemyController controller = enemy.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.ResetEnemy();
            }

            enemy.SetActive(true);
            livingEnemies.Add(enemy);
            OnEnemyCountChanged?.Invoke(livingEnemies.Count);
        }

        private void SpawnEnemy(GameObject prefab, Transform center, Transform enemyParent, float groupRadius)
        {
            Vector3 offset = UnityEngine.Random.insideUnitCircle * groupRadius;
            Vector3 spawnPos = center.position + new Vector3(offset.x, 0, offset.y);

            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity, enemyParent);
            livingEnemies.Add(enemy);
            OnEnemyCountChanged?.Invoke(livingEnemies.Count);
        }

        public void OnEnemyDied(GameObject enemy)
        {
            if (livingEnemies.Contains(enemy))
            {
                livingEnemies.Remove(enemy);
            }

            EnemyController controller = enemy.GetComponent<EnemyController>();
            if (controller != null)
            {
                if (controller.enemyType == EnemyType.Melee)
                {
                    if (!diedMeleeEnemies.Contains(enemy))
                        diedMeleeEnemies.Add(enemy);
                }
                else if (controller.enemyType == EnemyType.Ranged)
                {
                    if (!diedRangedEnemies.Contains(enemy))
                        diedRangedEnemies.Add(enemy);
                }
            }

            OnEnemyCountChanged?.Invoke(livingEnemies.Count);
            // Check if wave complete
            if (livingEnemies.Count == 0)
            {
                WaveManager.Instance.OnWaveComplete();
            }
        }
        public int GetLivingEnemyCount()
        {
            return livingEnemies.Count;
        }

        public void KillAllEnemies()
        {

            List<GameObject> enemiesToKill = new List<GameObject>(livingEnemies);

            foreach (GameObject enemy in enemiesToKill)
            {
                if (enemy != null && enemy.activeInHierarchy)
                {
                    EnemyController controller = enemy.GetComponent<EnemyController>();

                    if (controller != null)
                    {
                        controller.Die();
                    }
                }
            }

            WaveManager.Instance.ResetAllDefenders();
        }

    }
}
