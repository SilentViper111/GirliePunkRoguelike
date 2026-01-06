using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tracks room clearing and progression.
/// Links with biome system for rewards.
/// 
/// Reference: KB Section III - Biome System
/// </summary>
public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    [System.Serializable]
    public class RoomStatus
    {
        public RoomData roomData;
        public bool isCleared;
        public bool hasBeenVisited;
        public int enemiesSpawned;
        public int enemiesKilled;
    }

    [Header("Room Tracking")]
    [SerializeField] private List<RoomStatus> rooms = new List<RoomStatus>();
    [SerializeField] private RoomStatus currentRoom;

    [Header("Clearing Rewards")]
    [SerializeField] private int baseRewardScore = 500;
    [SerializeField] private float pickupSpawnChance = 0.5f;

    // Events
    public System.Action<RoomStatus> OnRoomEntered;
    public System.Action<RoomStatus> OnRoomCleared;

    private Transform _player;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;
    }

    private void Update()
    {
        if (_player == null) return;

        // Check which room player is in
        UpdateCurrentRoom();
    }

    /// <summary>
    /// Initializes rooms from world generator.
    /// </summary>
    public void InitializeRooms(List<RoomData> roomDataList)
    {
        rooms.Clear();
        foreach (var data in roomDataList)
        {
            rooms.Add(new RoomStatus
            {
                roomData = data,
                isCleared = false,
                hasBeenVisited = false,
                enemiesSpawned = 0,
                enemiesKilled = 0
            });
        }
        Debug.Log($"[RoomManager] Initialized {rooms.Count} rooms");
    }

    private void UpdateCurrentRoom()
    {
        // Find room player is in based on position
        Vector3 playerPos = _player.position;
        float minDist = float.MaxValue;
        RoomStatus closest = null;

        foreach (var room in rooms)
        {
            if (room.roomData == null) continue;
            
            float dist = Vector3.Distance(playerPos, room.roomData.center);
            if (dist < minDist)
            {
                minDist = dist;
                closest = room;
            }
        }

        if (closest != null && closest != currentRoom)
        {
            EnterRoom(closest);
        }
    }

    private void EnterRoom(RoomStatus room)
    {
        currentRoom = room;

        if (!room.hasBeenVisited)
        {
            room.hasBeenVisited = true;
            Debug.Log($"[RoomManager] First visit to room (Biome: {room.roomData.biome})");
        }

        OnRoomEntered?.Invoke(room);
    }

    /// <summary>
    /// Reports an enemy killed in the current room.
    /// </summary>
    public void ReportEnemyKilled()
    {
        if (currentRoom == null) return;

        currentRoom.enemiesKilled++;

        // Check if room cleared
        if (currentRoom.enemiesKilled >= currentRoom.enemiesSpawned && currentRoom.enemiesSpawned > 0)
        {
            ClearRoom(currentRoom);
        }
    }

    /// <summary>
    /// Reports enemies spawned in a room.
    /// </summary>
    public void ReportEnemiesSpawned(int count, Vector3 position)
    {
        // Find room by position
        RoomStatus room = FindRoomByPosition(position);
        if (room != null)
        {
            room.enemiesSpawned += count;
        }
    }

    private RoomStatus FindRoomByPosition(Vector3 position)
    {
        float minDist = float.MaxValue;
        RoomStatus closest = null;

        foreach (var room in rooms)
        {
            if (room.roomData == null) continue;
            
            float dist = Vector3.Distance(position, room.roomData.center);
            if (dist < minDist)
            {
                minDist = dist;
                closest = room;
            }
        }

        return closest;
    }

    private void ClearRoom(RoomStatus room)
    {
        if (room.isCleared) return;

        room.isCleared = true;
        OnRoomCleared?.Invoke(room);

        Debug.Log($"[RoomManager] ROOM CLEARED! (Biome: {room.roomData.biome})");

        // Rewards
        GameUI ui = FindFirstObjectByType<GameUI>();
        if (ui != null)
            ui.AddScore(baseRewardScore);

        // Spawn pickup
        if (Random.value < pickupSpawnChance && PickupSpawner.Instance != null)
        {
            PickupSpawner.Instance.TrySpawnPickupAtPosition(room.roomData.center);
        }
    }

    public RoomStatus GetCurrentRoom() => currentRoom;
    public int GetClearedRoomCount() => rooms.FindAll(r => r.isCleared).Count;
    public int GetTotalRoomCount() => rooms.Count;
}
