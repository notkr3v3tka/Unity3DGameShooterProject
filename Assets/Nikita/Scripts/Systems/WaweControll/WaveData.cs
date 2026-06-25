using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WaveData
{
    public int waveNumber;

    [Header("Enemies")]
    public List<WaveEnemyGroup> enemyGroups = new List<WaveEnemyGroup>();

    [Header("Scaling")]
    public float healthMultiplier = 1f;
    public float damageMultiplier = 1f;

    [Header("Spawn Pacing")]
    public int batchSize = 3;
    public float delayBetweenBatches = 1f;
}