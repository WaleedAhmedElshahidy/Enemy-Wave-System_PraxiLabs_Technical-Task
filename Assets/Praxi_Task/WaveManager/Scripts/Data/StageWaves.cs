using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
namespace WM
{
    [CreateAssetMenu(fileName = "StageWaves", menuName = "Scriptable Objects/StageWaves")]
    public class StageWaves : ScriptableObject
    {

        public List<Enemy> EnemyList;
        [SerializeField]
        public List<WaveData> wavesData;

        void OnValidate()
        {
            if(wavesData.Count == 0) return;
            wavesData[0].waveGroupStart = 1;
            for (int i = 1; i < wavesData.Count; i++)
            {
                wavesData[i].isInfinite = false;
                wavesData[i].waveGroupStart = wavesData[i - 1].waveGroupStart + wavesData[i - 1].wavesEffectedCount;
                wavesData[i - 1].FirstWaveEqual = wavesData[i -1].startEnemiesCount + wavesData[i-1].addedPerWave;
            }
            wavesData[^1].FirstWaveEqual = (wavesData[^1].startEnemiesCount + wavesData[^1].addedPerWave);
            wavesData[^1].isInfinite = true;
        }
    }

    [Serializable]
    public class WaveData
    {


        [Sirenix.OdinInspector.ReadOnly]  public bool isInfinite;
        [Sirenix.OdinInspector.ReadOnly] public int waveGroupStart;
        [Sirenix.OdinInspector.ReadOnly] public int FirstWaveEqual;

        [ShowIf("@!isInfinite")]
        public int wavesEffectedCount;

        [Header("Equation Parameters")]
        public int startEnemiesCount;
        public int addedPerWave;

    }

    [Serializable]
    public class Enemy
    {
        public EnemyType enemyType;
        public string enemyName;
        public GameObject enemyPrefab;
    }

    public enum EnemyType
    {
        Melee,
        Ranged
    }
}
