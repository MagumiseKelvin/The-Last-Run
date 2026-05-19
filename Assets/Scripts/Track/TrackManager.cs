using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Procedurally spawns, moves, and recycles track segments.
/// Notifies ObstacleSpawner and CollectibleSpawner when new segments are created.
/// </summary>
public class TrackManager : MonoBehaviour
{
    [Header("Track Prefabs")]
    [Tooltip("Array of track segment prefabs to randomly spawn")]
    public GameObject[] trackSegmentPrefabs;

    [Header("Track Settings")]
    [Tooltip("Length of each track segment on the Z axis")]
    public float segmentLength = 30f;
    [Tooltip("Number of segments to keep active ahead of the player")]
    public int segmentsAhead = 5;
    [Tooltip("Z position of the player (world reference point)")]
    public float playerZ = 0f;

    [Header("Despawn")]
    [Tooltip("How far behind the player a segment must be before recycling")]
    public float despawnDistance = 40f;

    private readonly List<GameObject> _activeSegments = new List<GameObject>();
    private readonly Queue<GameObject> _segmentPool   = new Queue<GameObject>();

    private float _spawnZ;

    private void Start()
    {
        _spawnZ = playerZ - segmentLength;

        // Spawn initial segments
        for (int i = 0; i < segmentsAhead + 2; i++)
            SpawnSegment();
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning) return;

        MoveSegments();
        RecycleOldSegments();
        EnsureEnoughSegments();
    }

    // ── Movement ──────────────────────────────────────────────────────────────

    private void MoveSegments()
    {
        float moveAmount = GameManager.Instance.GetCurrentSpeed() * Time.deltaTime;

        foreach (var seg in _activeSegments)
        {
            if (seg != null)
                seg.transform.Translate(0f, 0f, -moveAmount, Space.World);
        }

        _spawnZ -= moveAmount;
    }

    // ── Spawn ─────────────────────────────────────────────────────────────────

    private void SpawnSegment()
    {
        if (trackSegmentPrefabs == null || trackSegmentPrefabs.Length == 0)
        {
            Debug.LogWarning("[TrackManager] No track segment prefabs assigned!");
            return;
        }

        GameObject segment = GetFromPool();
        segment.transform.position = new Vector3(0f, 0f, _spawnZ);
        segment.SetActive(true);

        _activeSegments.Add(segment);
        _spawnZ += segmentLength;

        // Notify spawners
        TrackSegment ts = segment.GetComponent<TrackSegment>();
        if (ts != null)
        {
            ObstacleSpawner.Instance?.PopulateSegment(ts);
            CollectibleSpawner.Instance?.PopulateSegment(ts);
        }
    }

    private void EnsureEnoughSegments()
    {
        float furthestZ = _activeSegments.Count > 0
            ? _activeSegments[_activeSegments.Count - 1].transform.position.z
            : playerZ;

        while (furthestZ < playerZ + segmentsAhead * segmentLength)
        {
            SpawnSegment();
            furthestZ += segmentLength;
        }
    }

    // ── Recycle ───────────────────────────────────────────────────────────────

    private void RecycleOldSegments()
    {
        for (int i = _activeSegments.Count - 1; i >= 0; i--)
        {
            GameObject seg = _activeSegments[i];
            if (seg == null) { _activeSegments.RemoveAt(i); continue; }

            if (seg.transform.position.z < playerZ - despawnDistance)
            {
                ReturnToPool(seg);
                _activeSegments.RemoveAt(i);
            }
        }
    }

    // ── Pool ──────────────────────────────────────────────────────────────────

    private GameObject GetFromPool()
    {
        if (_segmentPool.Count > 0)
            return _segmentPool.Dequeue();

        int index = Random.Range(0, trackSegmentPrefabs.Length);
        return Instantiate(trackSegmentPrefabs[index], transform);
    }

    private void ReturnToPool(GameObject segment)
    {
        segment.SetActive(false);
        _segmentPool.Enqueue(segment);
    }
}
