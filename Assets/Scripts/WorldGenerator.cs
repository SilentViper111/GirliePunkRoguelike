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
    [SerializeField] private float worldRadius = 500f;
    [SerializeField] [Range(1, 3)] private int subdivisionLevel = 1;
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool showDebugGizmos = true;

    [Header("Mesh Settings")]
    [SerializeField] private bool generateMesh = true;
    [SerializeField] [Range(0f, 1f)] private float flattenAmount = 0.7f;
    [SerializeField] private bool addMeshCollider = true;

    [Header("Room Sizing")]
    [SerializeField] private float hexRoomScale = 0.8f;
    [SerializeField] private float pentRoomScale = 1.0f;
    [SerializeField] private float wallThickness = 0.1f;

    [Header("Room Prefabs")]
    [SerializeField] private GameObject hexRoomPrefab;
    [SerializeField] private GameObject pentRoomPrefab;

    [Header("Biome & Spawn Settings")]
    [SerializeField] private bool assignBiomes = true;
    [SerializeField] private bool spawnObstacles = true;
    [SerializeField] private bool generateOutlines = true;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private int playerSpawnRoomIndex = -1;

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
    private List<Vector3> _faceCenters;
    private List<int>[] _roomAdjacency;
    private List<GameObject> _spawnedRooms = new List<GameObject>();
    private List<RoomData> _roomDataList = new List<RoomData>();
    private BiomeGenerator _biomeGenerator;
    private RoomOutlineRenderer _outlineRenderer;
    private Mesh _worldMesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private MeshCollider _meshCollider;

    // Public accessor for world radius
    public float WorldRadius => worldRadius;

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

        // Step 4: Calculate face centers for mesh generation
        _faceCenters = new List<Vector3>();
        foreach (var tri in _triangles)
        {
            Vector3 center = (_vertices[tri[0]] + _vertices[tri[1]] + _vertices[tri[2]]) / 3f;
            _faceCenters.Add(center.normalized * worldRadius);
        }

        // Step 5: Generate the smooth mesh
        if (generateMesh)
        {
            GenerateMesh();
        }

        // Step 6: Spawn room prefabs (if assigned)
        SpawnRooms();

        // Step 7: Assign biomes and generate content
        if (assignBiomes)
        {
            AssignBiomes();
            if (spawnObstacles)
            {
                GenerateBiomeContent();
            }
        }

        // Step 8: Spawn player at random hexagon
        // DISABLED - PlayerSpawnFix component handles spawn positioning now
        // SpawnPlayer();

        // Step 9: Generate room outlines
        if (generateOutlines)
        {
            GenerateRoomOutlines();
        }

        Debug.Log($"[WorldGenerator] Generated: {vertexCount} vertices, {pentagonCount} VIP rooms, {hexagonCount} combat rooms, {_triangles.Count} triangular faces");
    }

    private void AddRoomAdjacency(int a, int b)
    {
        if (!_roomAdjacency[a].Contains(b)) _roomAdjacency[a].Add(b);
        if (!_roomAdjacency[b].Contains(a)) _roomAdjacency[b].Add(a);
    }

    /// <summary>
    /// Generates the smooth geodesic mesh with MeshFilter, MeshRenderer, and MeshCollider.
    /// </summary>
    private void GenerateMesh()
    {
        // Build the mesh using GeodesicMeshBuilder
        _worldMesh = GeodesicMeshBuilder.BuildMesh(
            _roomCenters,
            _pentagons,
            _hexagons,
            _faceCenters,
            flattenAmount
        );

        // Get or add MeshFilter
        _meshFilter = GetComponent<MeshFilter>();
        if (_meshFilter == null)
        {
            _meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        _meshFilter.sharedMesh = _worldMesh;

        // Get or add MeshRenderer
        _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshRenderer == null)
        {
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        // Apply neon floor material
        if (_meshRenderer.sharedMaterial == null)
        {
            _meshRenderer.sharedMaterial = GeodesicMeshBuilder.CreateNeonFloorMaterial();
        }

        // Get or add MeshCollider if enabled
        if (addMeshCollider)
        {
            _meshCollider = GetComponent<MeshCollider>();
            if (_meshCollider == null)
            {
                _meshCollider = gameObject.AddComponent<MeshCollider>();
            }
            _meshCollider.sharedMesh = _worldMesh;
        }

        Debug.Log($"[WorldGenerator] Mesh generated and applied with {(addMeshCollider ? "collider" : "no collider")}");
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
    /// Clears all spawned rooms and mesh resources.
    /// </summary>
    [ContextMenu("Clear World")]
    public void ClearWorld()
    {
        // Clear spawned room prefabs
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

        // Clear mesh
        if (_worldMesh != null)
        {
            if (Application.isPlaying)
                Destroy(_worldMesh);
            else
                DestroyImmediate(_worldMesh);
            _worldMesh = null;
        }

        // Reset mesh filter
        if (_meshFilter != null)
        {
            _meshFilter.sharedMesh = null;
        }
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
            return Vector3.up * worldRadius; // Default to top of sphere
        
        // Ensure position is on sphere surface at correct radius
        Vector3 center = _roomCenters[roomIndex];
        return center.normalized * worldRadius;
    }

    /// <summary>
    /// Assigns biomes to all rooms procedurally.
    /// </summary>
    private void AssignBiomes()
    {
        _roomDataList.Clear();
        System.Array biomeValues = System.Enum.GetValues(typeof(BiomeType));
        
        // Create RoomData for each room
        for (int i = 0; i < totalRoomCount; i++)
        {
            bool isPentagon = i < pentagonCount;
            int[] corners = isPentagon ? _pentagons[i] : _hexagons[i - pentagonCount];
            
            RoomData room = RoomData.Create(i, _roomCenters[i], corners, isPentagon);
            room.radius = worldRadius * 0.15f; // Approximate room radius
            
            // Assign random biome to non-VIP rooms
            if (!isPentagon)
            {
                room.biome = (BiomeType)biomeValues.GetValue(Random.Range(0, biomeValues.Length));
            }
            else
            {
                room.biome = BiomeType.VoidZone; // VIP rooms are special
            }
            
            // Copy adjacency
            if (_roomAdjacency != null && i < _roomAdjacency.Length)
            {
                room.adjacentRooms = new List<int>(_roomAdjacency[i]);
            }
            
            _roomDataList.Add(room);
        }
        
        Debug.Log($"[WorldGenerator] Assigned biomes to {_roomDataList.Count} rooms");
    }

    /// <summary>
    /// Generates obstacles and enemy spawn points using BiomeGenerator.
    /// </summary>
    private void GenerateBiomeContent()
    {
        _biomeGenerator = GetComponent<BiomeGenerator>();
        if (_biomeGenerator == null)
        {
            _biomeGenerator = gameObject.AddComponent<BiomeGenerator>();
        }

        // Create container for obstacles
        Transform obstacleContainer = transform.Find("Obstacles");
        if (obstacleContainer == null)
        {
            GameObject container = new GameObject("Obstacles");
            container.transform.SetParent(transform);
            obstacleContainer = container.transform;
        }

        foreach (var room in _roomDataList)
        {
            _biomeGenerator.GenerateRoomContent(room, worldRadius, obstacleContainer);
        }
        
        Debug.Log($"[WorldGenerator] Generated biome content for {_roomDataList.Count} rooms");
    }

    /// <summary>
    /// Spawns or moves player to a random hexagon room.
    /// </summary>
    private void SpawnPlayer()
    {
        Debug.Log($"[WorldGenerator] SpawnPlayer called. worldRadius={worldRadius}, totalRoomCount={totalRoomCount}");
        
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
            {
                playerObj = GameObject.Find("Player");
            }
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        if (playerTransform == null)
        {
            Debug.LogWarning("[WorldGenerator] No player found to spawn!");
            return;
        }
        
        Debug.Log($"[WorldGenerator] Found player: {playerTransform.name}");

        // Pick spawn room (random hexagon if not specified)
        if (playerSpawnRoomIndex < 0 || playerSpawnRoomIndex >= totalRoomCount)
        {
            playerSpawnRoomIndex = GetRandomStartRoom();
        }
        
        Debug.Log($"[WorldGenerator] Player spawn room index: {playerSpawnRoomIndex}, roomCenters count: {(_roomCenters != null ? _roomCenters.Count : 0)}");

        // Mark room as spawn
        if (playerSpawnRoomIndex < _roomDataList.Count)
        {
            _roomDataList[playerSpawnRoomIndex].isPlayerSpawn = true;
        }

        // Get spawn direction - use room center direction or default to UP
        Vector3 spawnDirection;
        if (_roomCenters != null && playerSpawnRoomIndex < _roomCenters.Count)
        {
            spawnDirection = _roomCenters[playerSpawnRoomIndex].normalized;
            Debug.Log($"[WorldGenerator] Using room center direction: {spawnDirection}");
        }
        else
        {
            spawnDirection = Vector3.up;
            Debug.Log("[WorldGenerator] Using default UP direction");
        }
        
        // Force spawn position to be ON the sphere surface at worldRadius
        float spawnHeight = 3f; // Units above surface
        Vector3 surfacePos = spawnDirection * worldRadius;
        Vector3 finalPos = surfacePos + spawnDirection * spawnHeight;
        
        Debug.Log($"[WorldGenerator] Final spawn: surfacePos={surfacePos}, finalPos={finalPos}, magnitude={finalPos.magnitude}");

        // Apply position
        playerTransform.position = finalPos;
        
        // Orient player to stand on sphere (up = outward from center)
        Vector3 forward = Vector3.ProjectOnPlane(Vector3.forward, spawnDirection);
        if (forward.sqrMagnitude < 0.01f)
            forward = Vector3.ProjectOnPlane(Vector3.right, spawnDirection);
        forward = forward.normalized;
        
        playerTransform.rotation = Quaternion.LookRotation(forward, spawnDirection);
        
        // Reset any velocity if rigidbody exists
        Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        Debug.Log($"[WorldGenerator] SUCCESS! Player spawned at {finalPos} (distance from center: {finalPos.magnitude})");
    }

    /// <summary>
    /// Gets the RoomData for a specific room index.
    /// </summary>
    public RoomData GetRoomData(int roomIndex)
    {
        if (roomIndex < 0 || roomIndex >= _roomDataList.Count) return null;
        return _roomDataList[roomIndex];
    }

    /// <summary>
    /// Generates glowing outline renderers for each room.
    /// </summary>
    private void GenerateRoomOutlines()
    {
        _outlineRenderer = GetComponent<RoomOutlineRenderer>();
        if (_outlineRenderer == null)
        {
            _outlineRenderer = gameObject.AddComponent<RoomOutlineRenderer>();
        }

        _outlineRenderer.GenerateOutlines(
            _roomCenters,
            _pentagons,
            _hexagons,
            _faceCenters,
            worldRadius,
            _roomDataList
        );
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || _vertices == null || _vertices.Count == 0)
            return;

        // Calculate face centers (these become the corners of rooms in dual graph)
        List<Vector3> faceCenters = new List<Vector3>();
        if (_triangles != null)
        {
            foreach (var tri in _triangles)
            {
                Vector3 center = (_vertices[tri[0]] + _vertices[tri[1]] + _vertices[tri[2]]) / 3f;
                // Project to sphere surface for clean visualization
                faceCenters.Add(center.normalized * worldRadius);
            }
        }

        // Draw Pentagon rooms (VIP) - Magenta outlines
        if (_pentagons != null && faceCenters.Count > 0)
        {
            Gizmos.color = new Color(1f, 0f, 1f, 1f); // Magenta
            foreach (var pent in _pentagons)
            {
                for (int i = 0; i < pent.Length; i++)
                {
                    Vector3 a = faceCenters[pent[i]];
                    Vector3 b = faceCenters[pent[(i + 1) % pent.Length]];
                    Gizmos.DrawLine(a, b);
                }
            }
        }

        // Draw Hexagon rooms (Combat) - Cyan outlines
        if (_hexagons != null && faceCenters.Count > 0)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.8f); // Cyan
            foreach (var hex in _hexagons)
            {
                for (int i = 0; i < hex.Length; i++)
                {
                    Vector3 a = faceCenters[hex[i]];
                    Vector3 b = faceCenters[hex[(i + 1) % hex.Length]];
                    Gizmos.DrawLine(a, b);
                }
            }
        }

        // Draw room centers as spheres
        if (_roomCenters != null)
        {
            for (int i = 0; i < _roomCenters.Count; i++)
            {
                // Pentagons larger and magenta, hexagons smaller and cyan
                Gizmos.color = i < pentagonCount ? new Color(1f, 0f, 1f, 0.9f) : new Color(0f, 1f, 1f, 0.6f);
                float size = i < pentagonCount ? 3f : 1.5f;
                Gizmos.DrawSphere(_roomCenters[i], size);
            }
        }

        // Optional: Draw room adjacency connections (green)
        if (_roomAdjacency != null && _roomCenters != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f); // Faint green
            for (int i = 0; i < _roomAdjacency.Length; i++)
            {
                foreach (int neighbor in _roomAdjacency[i])
                {
                    if (neighbor > i)
                    {
                        Gizmos.DrawLine(_roomCenters[i], _roomCenters[neighbor]);
                    }
                }
            }
        }
    }
}
