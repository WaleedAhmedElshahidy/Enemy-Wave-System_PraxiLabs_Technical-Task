using System;
using UnityEngine;
namespace WM
{
    [Serializable]
    public class WaveData
    {
        [Header("Wave Parameters")]
        public int enemyCount = 20;

        [Header("Enemy Composition")]

        [Range(0f, 100f)]
        public float meleePercent = 70f;
        [Range(0f, 100f)]
        public float rangedPercent = 30f;


    }
}
