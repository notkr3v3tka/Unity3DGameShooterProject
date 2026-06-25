using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject meleeEnemyPrefab;
    [SerializeField] private GameObject rangedEnemyPrefab;
    [SerializeField] private GameObject tankEnemyPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Wave Settings")]
    [SerializeField] private List<WaveData> waves = new List<WaveData>();

    [Header("Fallback Scaling")]
    [SerializeField] private int extraMeleePerFallbackWave = 2;
    [SerializeField] private float fallbackHealthGrowthPerWave = 0.15f;
    [SerializeField] private float fallbackDamageGrowthPerWave = 0.08f;
    [SerializeField] private int fallbackBatchSize = 4;
    [SerializeField] private float fallbackBatchDelay = 1f;

    public int CurrentWaveNumber { get; private set; }

    private int enemiesAlive;
    private bool waveInProgress;
    private bool spawningWave;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartNextWave();
    }

    public void StartNextWave()
    {
        if (spawningWave) return;
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        CurrentWaveNumber++;
        WaveData waveData = GetWaveData(CurrentWaveNumber);

        StartCoroutine(SpawnWaveRoutine(waveData));
    }

    private IEnumerator SpawnWaveRoutine(WaveData waveData)
    {
        spawningWave = true;
        waveInProgress = true;

        List<WaveEnemyType> spawnQueue = BuildSpawnQueue(waveData);

        enemiesAlive = spawnQueue.Count;

        int batchSize = Mathf.Max(1, waveData.batchSize);
        float batchDelay = Mathf.Max(0f, waveData.delayBetweenBatches);

        int spawnedInCurrentBatch = 0;

        foreach (WaveEnemyType enemyType in spawnQueue)
        {
            SpawnEnemy(enemyType, waveData.healthMultiplier, waveData.damageMultiplier);

            spawnedInCurrentBatch++;

            if (spawnedInCurrentBatch >= batchSize)
            {
                spawnedInCurrentBatch = 0;

                if (batchDelay > 0f)
                {
                    yield return new WaitForSeconds(batchDelay);
                }
            }
        }

        spawningWave = false;
    }

    private void SpawnEnemy(WaveEnemyType enemyType, float healthMultiplier, float damageMultiplier)
    {
        GameObject prefab = GetPrefabByType(enemyType);
        if (prefab == null) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemyObject = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        EnemyStats enemyStats = enemyObject.GetComponent<EnemyStats>();
        if (enemyStats != null)
        {
            enemyStats.ApplyWaveScaling(healthMultiplier, damageMultiplier);
        }
    }

    private GameObject GetPrefabByType(WaveEnemyType enemyType)
    {
        switch (enemyType)
        {
            case WaveEnemyType.Melee:
                return meleeEnemyPrefab;

            case WaveEnemyType.Ranged:
                return rangedEnemyPrefab;

            case WaveEnemyType.Tank:
                return tankEnemyPrefab;
        }

        return null;
    }

    private List<WaveEnemyType> BuildSpawnQueue(WaveData waveData)
    {
        List<WaveEnemyType> queue = new List<WaveEnemyType>();

        foreach (WaveEnemyGroup group in waveData.enemyGroups)
        {
            for (int i = 0; i < group.count; i++)
            {
                queue.Add(group.enemyType);
            }
        }

        Shuffle(queue);
        return queue;
    }

    private WaveData GetWaveData(int waveNumber)
    {
        foreach (WaveData wave in waves)
        {
            if (wave.waveNumber == waveNumber)
            {
                return wave;
            }
        }

        return GenerateFallbackWave(waveNumber);
    }

    private WaveData GenerateFallbackWave(int waveNumber)
    {
        WaveData fallback = new WaveData();
        fallback.waveNumber = waveNumber;
        fallback.healthMultiplier = 1f + (waveNumber - 1) * fallbackHealthGrowthPerWave;
        fallback.damageMultiplier = 1f + (waveNumber - 1) * fallbackDamageGrowthPerWave;
        fallback.batchSize = fallbackBatchSize;
        fallback.delayBetweenBatches = fallbackBatchDelay;

        int meleeCount = 5 + (waveNumber - 1) * extraMeleePerFallbackWave;
        int rangedCount = Mathf.Max(0, waveNumber - 2);
        int tankCount = waveNumber >= 5 ? Mathf.FloorToInt((waveNumber - 3) / 3f) : 0;

        fallback.enemyGroups.Add(new WaveEnemyGroup { enemyType = WaveEnemyType.Melee, count = meleeCount });

        if (rangedCount > 0)
        {
            fallback.enemyGroups.Add(new WaveEnemyGroup { enemyType = WaveEnemyType.Ranged, count = rangedCount });
        }

        if (tankCount > 0)
        {
            fallback.enemyGroups.Add(new WaveEnemyGroup { enemyType = WaveEnemyType.Tank, count = tankCount });
        }

        return fallback;
    }

    public void OnEnemyKilled()
    {
        if (!waveInProgress) return;

        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);

        if (enemiesAlive <= 0 && !spawningWave)
        {
            waveInProgress = false;
            OnWaveCompleted();
        }
    }

    private void OnWaveCompleted()
    {
        if (UpgradeCardManager.Instance != null)
        {
            UpgradeCardManager.Instance.OpenCardSelection(StartNextWave);
        }
        else
        {
            StartNextWave();
        }
    }

    private void Shuffle(List<WaveEnemyType> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            WaveEnemyType temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}