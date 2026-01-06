using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// MonoBehaviour that generates and visualizes the geodesic world.
/// Attach to an empty GameObject called "WorldGenerator" in the scene.
/// 
/// Subdivision Levels:
/// - 0 = 12 vertices (original icosahedron) - not recommended
/// - 1 = 42 vertices = 12 pentagons + 30 hexagons (default)
/// - 2 = 162 vertices = 12 pentagons + 150 hexagons (triple size)
/// - 3 = 642 vertices = 12 pentagons + 630 hexagons (massive world)
/// 
/// Reference: KB Section II.A, Bible Section III
/// </summary>
public class WorldGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    [SerializeField] private float worldRadius = 100f;
    [SerializeField] [Range(1, 3)] private int subdivisionLevel = 2;
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool showDebugGizmos = true;

    [Header("Room Sizing")]
    [SerializeField] private float hexRoomScale = 0.8f;
    [SerializeField] private float pentRoomScale = 1.0f;
    [SerializeField] private float wallThickness = 0.1f;

    [Header("Room Prefabs")]
    [SerializeField] private GameObject hexRoomPrefab;
    [SerializeField] private GameObject pentRoomPrefab;

    [Header("Generated Data (Read-Only)")]
    [SerializeField] private int vertexCount;
    [SerializeField] private int pentagonCount;
    [SerializeField] private int hexagonCount;
    [SerializeField] private int totalRoomCount;

    // Internal data
    private List<Vector3> _vertices;
    private List<int[]> _triangles;
    private List<int[]> _pentagons;
    private List<int[]> _hexagons;
    private List<Vector3> _roomCenters;
    private List<int>[] _roomAdjacency;
    private List<GameObject> _spawnedRooms = new List<GameObject>();

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateWorld();
        }
    }

    /// <summary>
    /// Generates the complete world structure with geodesic subdivision.
    /// </summary>
    [ContextMenu("Generate World")]
    public void GenerateWorld()
    {
        ClearWorld();

        // Step 1: Generate geodesic sphere with subdivisions
        (_vertices, _triangles) = GeodesicWorldGenerator.GenerateGeodesicSphere(subdivisionLevel, worldRadius);
        vertexCount = _vertices.Count;

        // Step 2: Convert to hexagonal/pentagonal rooms (dual graph)
        (_pentagons, _hexagons, _roomCenters) = GeodesicWorldGenerator.ConvertToHexagonalRooms(_vertices, _triangles);
        pentagonCount = _pentagons.Count;
        hexagonCount = _hexagons.Count;
        totalRoomCount = pentagonCount + hexagonCount;

        // Step 3: Build room adjacency for navigation
        // Note: In the dual graph, rooms are adjacent if they share a triangle face
        _roomAdjacency = new List<int>[totalRoomCount];
        for (int i = 0; i < totalRoomCount; i++)
            _roomAdjacency[i] = new List<int>();

        // Build adjacency from triangles (each triangle edge = adjacent rooms)
        foreach (var tri in _triangles)
        {
            AddRoomAdjacency(tri[0], tri[1]);
            AddRoomAdjacency(tri[1], tri[2]);
            AddRoomAdjacency(tri[2], tri[0]);
        }

        // Step 4: Spawn room prefabs (if assigned)
        SpawnRooms();

        Debug.Log($"[WorldGenerator] Generated: {vertexCount} vertices, {pentagonCount} VIP rooms, {hexagonCount} combat rooms, {_triangles.Count} triangular faces");
    }

    private void AddRoomAdjacency(int a, int b)
    {
        if (!_roomAdjacency[a].Contains(b)) _roomAdjacency[a].Add(b);
        if (!_roomAdjacency[b].Contains(a)) _roomAdjacency[b].Add(a);
    }

    /// <summary>
    /// Spawns room prefabs at room centers.
    /// </summary>
    private void SpawnRooms()
    {
        // Pentagon rooms (VIP/Special) - first pentagonCount entries
        for (int i = 0; i < _pentagons.Count; i++)
        {
            Vector3 center = _roomCenters[i];
            Vector3 normal = center.normalized;

            if (pentRoomPrefab != null)
            {
                GameObject room = Instantiate(pentRoomPrefab, center, Quaternion.LookRotation(Vector3.forward, normal), transform);
                room.name = $"PentRoom_{i} (VIP)";
                room.transform.localScale = Vector3.one * pentRoomScale;
                _spawnedRooms.Add(room);
            }
        }

        // Hexagon rooms (Combat) - remaining entries
        for (int i = 0; i < _hexagons.Count; i++)
        {
            int roomIndex = _pentagons.Count + i;
            Vector3 center = _roomCenters[roomIndex];
            Vector3 normal = center.normalized;

            if (hexRoomPrefab != null)
            {
                GameObject room = Instantiate(hexRoomPrefab, center, Quaternion.LookRotation(Vector3.forward, normal), transform);
                room.name = $"HexRoom_{i} (Combat)";
                room.transform.localScale = Vector3.one * hexRoomScale;
                _spawnedRooms.Add(room);
            }
        }
    }

    /// <summary>
    /// Clears all spawned rooms.
    /// </summary>
    [ContextMenu("Clear World")]
    public void ClearWorld()
    {
        foreach (var room in _spawnedRooms)
        {
            if (room != null)
            {
                if (Application.isPlaying)
                    Destroy(room);
                else
                    DestroyImmediate(room);
            }
        }
        _spawnedRooms.Clear();
    }

    /// <summary>
    /// Gets adjacent room indices for pathfinding.
    /// </summary>
    public List<int> GetAdjacentRooms(int roomIndex)
    {
        if (_roomAdjacency == null || roomIndex < 0 || roomIndex >= _roomAdjacency.Length)
            return new List<int>();
        return _roomAdjacency[roomIndex];
    }

    /// <summary>
    /// Gets a random starting room (always a hexagon/combat room).
    /// </summary>
    public int GetRandomStartRoom()
    {
        // Combat rooms start after pentagons
        return Random.Range(_pentagons.Count, totalRoomCount);
    }

    /// <summary>
    /// Checks if a room is a VIP/special room (pentagon).
    /// </summary>
    public bool IsVIPRoom(int roomIndex) => roomIndex < _pentagons.Count;

    /// <summary>
    /// Gets the world position of a room center.
    /// </summary>
    public Vector3 GetRoomCenter(int roomIndex)
    {
        if (_roomCenters == null || roomIndex < 0 || roomIndex >= _roomCenters.Count)
            return Vector3.zero;
        return _roomCenters[roomIndex];
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || _vertices == null || _vertices.Count == 0)
            return;

        // Draw room centers
        for (int i = 0; i < _roomCenters?.Count; i++)
        {
            // Pentagons in magenta, hexagons in cyan
            Gizmos.color = i < pentagonCount ? new Color(1f, 0f, 1f, 0.8f) : new Color(0f, 1f, 1f, 0.6f);
            float size = i < pentagonCount ? 2f : 1f;
            Gizmos.DrawSphere(_roomCenters[i], size);
        }

        // Draw triangular mesh wireframe
        if (_triangles != null)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Yellow
            foreach (var tri in _triangles)
            {
                Vector3 a = _vertices[tri[0]];
                Vector3 b = _vertices[tri[1]];
                Vector3 c = _vertices[tri[2]];
                Gizmos.DrawLine(a, b);
                Gizmos.DrawLine(b, c);
                Gizmos.DrawLine(c, a);
            }
        }

        // Draw room adjacency connections
        if (_roomAdjacency != null && _roomCenters != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.5f); // Green
            for (int i = 0; i < _roomAdjacency.Length; i++)
            {
                foreach (int neighbor in _roomAdjacency[i])
                {
                    if (neighbor > i) // Avoid drawing twice
                    {
                        Gizmos.DrawLine(_roomCenters[i], _roomCenters[neighbor]);
                    }
                }
            }
        }
    }
}
