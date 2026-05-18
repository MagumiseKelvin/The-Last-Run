using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Procedurally spawns, moves, and recycles track segments.
/// Track segments move toward the player (the player stays in place on Z).
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
    [Tooltip("Z position of the player (track spawns ahead of this)")]
    public float playerZ = 0f;

    [Header("Despawn")]
    [Tooltip("How far behind the player a segment must be before it's recycled")]
    public float despawnDistance = 40f;

    // Pool of active segments
    private readonly List<GameObject> _activeSegments = new List<GameObject>();
    // Object pool for recycling
    private readonly Queue<GameObject> _segmentPool = new Queue<GameObject>();

    private float _spawnZ;   // Z position where the next segment will spawn

    private void Start()
    {
        // Spawn initial segments starting just behind the player
        _spawnZ = playerZ - segmentLength; // one segment behind so player starts on track
        for (int i = 0; i < segmentsAhead + 2; i++)
        {
            SpawnSegment();
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning) return;

        MoveSegments();
        RecycleOldSegments();
        EnsureEnoughSegments();
    }

    // ── Segment Movement ──────────────────────────────────────────────────────

    private void MoveSegments()
    {
        float speed = GameManager.Instance.GetCurrentSpeed();
        float moveAmount = speed * Time.deltaTime;

        foreach (var seg in _activeSegments)
        {
            if (seg == null) continue;
            seg.transform.Translate(0f, 0f, -moveAmount, Space.World);
        }

        // Also shift the spawn Z so new segments spawn at the right place
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
    }

    private void EnsureEnoughSegments()
    {
        // Keep spawning until we have enough segments ahead
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
            if (seg == null)
            {
                _activeSegments.RemoveAt(i);
                continue;
            }

            // If segment has moved far enough behind the player, recycle it
            if (seg.transform.position.z < playerZ - despawnDistance)
            {
                ReturnToPool(seg);
                _activeSegments.RemoveAt(i);
            }
        }
    }

    // ── Object Pool ───────────────────────────────────────────────────────────

    private GameObject GetFromPool()
    {
        if (_segmentPool.Count > 0)
        {
            return _segmentPool.Dequeue();
        }

        // Pick a random prefab and instantiate
        int index = Random.Range(0, trackSegmentPrefabs.Length);
        return Instantiate(trackSegmentPrefabs[index], transform);
    }

    private void ReturnToPool(GameObject segment)
    {
        segment.SetActive(false);
        _segmentPool.Enqueue(segment);
    }
}
