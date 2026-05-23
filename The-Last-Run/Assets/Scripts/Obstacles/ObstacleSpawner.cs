using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns obstacles onto track segments as they are generated.
/// Uses an object pool for performance.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    public static ObstacleSpawner Instance { get; private set; }

    [Header("Obstacle Prefabs")]
    [Tooltip("List of obstacle prefabs that can be spawned")]
    public GameObject[] obstaclePrefabs;

    [Header("Spawn Settings")]
    [Tooltip("Probability (0-1) that an obstacle spawns on any given spawn point")]
    [Range(0f, 1f)]
    public float spawnChance = 0.4f;

    [Tooltip("Minimum number of safe segments at the start before obstacles appear")]
    public int safeSegmentsAtStart = 3;

    private int _segmentsSpawned = 0;

    // Object pools per prefab index
    private Dictionary<int, Queue<GameObject>> _pools = new Dictionary<int, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Called by TrackManager when a new segment becomes active.
    /// Populates the segment with obstacles.
    /// </summary>
    public void PopulateSegment(TrackSegment segment)
    {
        if (segment == null) return;
        if (segment.isSafeSegment) return;
        if (_segmentsSpawned < safeSegmentsAtStart) { _segmentsSpawned++; return; }

        _segmentsSpawned++;

        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

        // Try to spawn on each obstacle spawn point
        foreach (Transform spawnPoint in segment.obstacleSpawnPoints)
        {
            if (spawnPoint == null) continue;

            if (Random.value <= spawnChance)
            {
                SpawnObstacleAt(spawnPoint);
            }
        }
    }

    private void SpawnObstacleAt(Transform spawnPoint)
    {
        int prefabIndex = Random.Range(0, obstaclePrefabs.Length);
        GameObject obstacle = GetFromPool(prefabIndex);

        obstacle.transform.position = spawnPoint.position;
        obstacle.transform.rotation = spawnPoint.rotation;
        obstacle.transform.SetParent(spawnPoint.parent); // parent to segment so it moves with it
        obstacle.SetActive(true);
    }

    // ── Object Pool ───────────────────────────────────────────────────────────

    private GameObject GetFromPool(int prefabIndex)
    {
        if (!_pools.ContainsKey(prefabIndex))
            _pools[prefabIndex] = new Queue<GameObject>();

        if (_pools[prefabIndex].Count > 0)
            return _pools[prefabIndex].Dequeue();

        return Instantiate(obstaclePrefabs[prefabIndex]);
    }

    public void ReturnToPool(GameObject obstacle, int prefabIndex)
    {
        obstacle.SetActive(false);
        obstacle.transform.SetParent(transform);

        if (!_pools.ContainsKey(prefabIndex))
            _pools[prefabIndex] = new Queue<GameObject>();

        _pools[prefabIndex].Enqueue(obstacle);
    }
}
