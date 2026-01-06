using System;
using UnityEngine;

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

    [Header("Spawn Settings")]
    public float spawnRate = 0.5f;
}
