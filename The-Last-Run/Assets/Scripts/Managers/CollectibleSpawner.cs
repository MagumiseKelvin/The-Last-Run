using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Phase 4 — Spawns collectibles (coins and relics) onto track segments.
/// Supports line patterns, arc patterns, and random scatter.
/// Uses object pooling for performance.
/// </summary>
public class CollectibleSpawner : MonoBehaviour
{
    public static CollectibleSpawner Instance { get; private set; }

    // ── Prefabs ───────────────────────────────────────────────────────────────
    [Header("Collectible Prefabs")]
    [Tooltip("Standard coin prefab")]
    public GameObject coinPrefab;
    [Tooltip("Rare relic prefab — worth more points")]
    public GameObject relicPrefab;

    // ── Spawn Settings ────────────────────────────────────────────────────────
    [Header("Spawn Settings")]
    [Tooltip("Chance (0-1) that a collectible pattern spawns on a segment")]
    [Range(0f, 1f)]
    public float spawnChance = 0.7f;

    [Tooltip("Chance (0-1) that a relic spawns instead of coins")]
    [Range(0f, 1f)]
    public float relicChance = 0.1f;

    [Tooltip("Number of coins in a line pattern")]
    public int linePatternCount = 5;

    [Tooltip("Spacing between coins in a line pattern")]
    public float lineSpacing = 1.5f;

    [Tooltip("Height above track to spawn coins")]
    public float coinHeight = 0.8f;

    [Tooltip("Minimum segments before collectibles start spawning")]
    public int safeSegmentsAtStart = 2;

    // ── Lane Config ───────────────────────────────────────────────────────────
    [Header("Lane Config")]
    public float laneWidth = 2.5f;
    public int laneCount = 3;

    // ── Pool ──────────────────────────────────────────────────────────────────
    private Queue<GameObject> _coinPool = new Queue<GameObject>();
    private Queue<GameObject> _relicPool = new Queue<GameObject>();

    private int _segmentsProcessed = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by TrackManager when a new segment is spawned.
    /// Populates it with a collectible pattern.
    /// </summary>
    public void PopulateSegment(TrackSegment segment)
    {
        if (segment == null || segment.isSafeSegment) return;
        if (_segmentsProcessed < safeSegmentsAtStart) { _segmentsProcessed++; return; }
        _segmentsProcessed++;

        if (Random.value > spawnChance) return;

        // Pick a random lane
        int lane = Random.Range(0, laneCount);
        float laneX = (lane - laneCount / 2) * laneWidth;

        // Pick a pattern
        int pattern = Random.Range(0, 3);
        switch (pattern)
        {
            case 0: SpawnLinePattern(segment, laneX);   break;
            case 1: SpawnZigzagPattern(segment);        break;
            case 2: SpawnArcPattern(segment, laneX);    break;
        }

        // Chance to also spawn a relic on a random spawn point
        if (Random.value < relicChance && segment.collectibleSpawnPoints != null
            && segment.collectibleSpawnPoints.Length > 0)
        {
            Transform pt = segment.GetRandomCollectibleSpawnPoint();
            if (pt != null) SpawnRelic(pt.position, segment.transform);
        }
    }

    // ── Patterns ──────────────────────────────────────────────────────────────

    /// <summary>Spawns a straight line of coins down one lane.</summary>
    private void SpawnLinePattern(TrackSegment segment, float laneX)
    {
        Vector3 segPos = segment.transform.position;
        float startZ = segPos.z + 4f;

        for (int i = 0; i < linePatternCount; i++)
        {
            Vector3 pos = new Vector3(laneX, segPos.y + coinHeight, startZ + i * lineSpacing);
            SpawnCoin(pos, segment.transform);
        }
    }

    /// <summary>Spawns coins in a zigzag across two lanes.</summary>
    private void SpawnZigzagPattern(TrackSegment segment)
    {
        Vector3 segPos = segment.transform.position;
        float startZ = segPos.z + 4f;
        float leftX  = (-1) * laneWidth;
        float rightX = (1)  * laneWidth;

        for (int i = 0; i < linePatternCount; i++)
        {
            float x = (i % 2 == 0) ? leftX : rightX;
            Vector3 pos = new Vector3(x, segPos.y + coinHeight, startZ + i * lineSpacing);
            SpawnCoin(pos, segment.transform);
        }
    }

    /// <summary>Spawns coins in a low arc (jump to collect).</summary>
    private void SpawnArcPattern(TrackSegment segment, float laneX)
    {
        Vector3 segPos = segment.transform.position;
        float startZ = segPos.z + 4f;
        int count = linePatternCount;

        for (int i = 0; i < count; i++)
        {
            // Parabolic height — peaks in the middle
            float t = (float)i / (count - 1);
            float arcHeight = Mathf.Sin(t * Mathf.PI) * 2.5f;
            Vector3 pos = new Vector3(laneX, segPos.y + coinHeight + arcHeight, startZ + i * lineSpacing);
            SpawnCoin(pos, segment.transform);
        }
    }

    // ── Spawn Helpers ─────────────────────────────────────────────────────────

    private void SpawnCoin(Vector3 position, Transform parent)
    {
        if (coinPrefab == null) return;
        GameObject coin = GetFromPool(_coinPool, coinPrefab);
        coin.transform.position = position;
        coin.transform.SetParent(parent);
        coin.SetActive(true);
    }

    private void SpawnRelic(Vector3 position, Transform parent)
    {
        if (relicPrefab == null) return;
        GameObject relic = GetFromPool(_relicPool, relicPrefab);
        relic.transform.position = position + Vector3.up * coinHeight;
        relic.transform.SetParent(parent);
        relic.SetActive(true);
    }

    // ── Pool ──────────────────────────────────────────────────────────────────

    private GameObject GetFromPool(Queue<GameObject> pool, GameObject prefab)
    {
        if (pool.Count > 0)
        {
            var obj = pool.Dequeue();
            if (obj != null) return obj;
        }
        return Instantiate(prefab, transform);
    }

    public void ReturnCoinToPool(GameObject coin)
    {
        coin.SetActive(false);
        coin.transform.SetParent(transform);
        _coinPool.Enqueue(coin);
    }

    public void ReturnRelicToPool(GameObject relic)
    {
        relic.SetActive(false);
        relic.transform.SetParent(transform);
        _relicPool.Enqueue(relic);
    }
}
