using System;
using System.Collections.Generic;
using UnityEngine;
namespace WM
{
    [CreateAssetMenu(fileName = "StageWaves", menuName = "Scriptable Objects/StageWaves")]
    public class StageWaves : ScriptableObject
    {

        public List<Enemy> EnemyList;
        [SerializeField]
        public List<WaveData> WavesData;
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
