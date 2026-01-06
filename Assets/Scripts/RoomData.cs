using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Stores per-room data including biome type, content, and state.
/// Created by WorldGenerator during world generation.
/// 
/// Reference: Implementation Plan Phase 4
/// </summary>
[System.Serializable]
public class RoomData
{
    [Header("Identity")]
    public int roomIndex;
    public bool isVIP; // Pentagon = VIP, Hexagon = Combat/Biome
    public BiomeType biome;
    
    [Header("Geometry")]
    public Vector3 center;
    public Vector3 normal; // Points outward from sphere
    public float radius; // Approximate room radius
    public int[] cornerFaceIndices; // Indices of triangular faces forming corners
    
    [Header("Adjacency")]
    public List<int> adjacentRooms = new List<int>();
    
    [Header("Content")]
    public List<Vector3> obstaclePositions = new List<Vector3>();
    public List<Vector3> enemySpawnPoints = new List<Vector3>();
    
    [Header("State")]
    public bool isCleared = false;
    public bool isPlayerSpawn = false;
    public bool hasBeenVisited = false;

    /// <summary>
    /// Creates room data from generation parameters.
    /// </summary>
    public static RoomData Create(int index, Vector3 center, int[] corners, bool isPentagon)
    {
        return new RoomData
        {
            roomIndex = index,
            center = center,
            normal = center.normalized,
            isVIP = isPentagon,
            cornerFaceIndices = corners,
            biome = BiomeType.NeonCity // Default, will be assigned procedurally
        };
    }

    /// <summary>
    /// Gets a random position within the room for spawning.
    /// </summary>
    public Vector3 GetRandomPositionInRoom(float worldRadius)
    {
        // Random angle on the tangent plane
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(0f, radius * 0.7f); // Stay within 70% of room
        
        // Create tangent vectors
        Vector3 up = normal;
        Vector3 right = Vector3.Cross(up, Vector3.forward).normalized;
        if (right.sqrMagnitude < 0.01f)
            right = Vector3.Cross(up, Vector3.right).normalized;
        Vector3 forward = Vector3.Cross(right, up).normalized;
        
        // Offset position
        Vector3 offset = (right * Mathf.Cos(angle) + forward * Mathf.Sin(angle)) * distance;
        Vector3 position = center + offset;
        
        // Project back onto sphere surface
        return position.normalized * worldRadius;
    }
}
