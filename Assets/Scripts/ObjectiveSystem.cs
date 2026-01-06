using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Objective system for in-game goals.
/// Tracks kill counts, room clearing, time challenges.
/// 
/// Reference: KB Section VII - Progression
/// </summary>
public class ObjectiveSystem : MonoBehaviour
{
    public static ObjectiveSystem Instance { get; private set; }

    [System.Serializable]
    public class Objective
    {
        public string id;
        public string description;
        public ObjectiveType type;
        public int targetValue;
        public int currentValue;
        public bool isCompleted;
        public int rewardScore;
    }

    public enum ObjectiveType
    {
        KillEnemies,
        ClearRooms,
        ReachWave,
        AchieveCombo,
        CollectPickups,
        SurviveTime,
        DefeatBoss
    }

    [Header("Current Objectives")]
    [SerializeField] private List<Objective> activeObjectives = new List<Objective>();

    [Header("Settings")]
    [SerializeField] private int maxActiveObjectives = 3;

    // Events
    public System.Action<Objective> OnObjectiveCompleted;
    public System.Action<Objective> OnObjectiveProgress;

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
        // Generate initial objectives
        GenerateObjectives();
    }

    private void GenerateObjectives()
    {
        activeObjectives.Clear();

        // Kill enemies objective
        activeObjectives.Add(new Objective
        {
            id = "kill_10",
            description = "Kill 10 enemies",
            type = ObjectiveType.KillEnemies,
            targetValue = 10,
            currentValue = 0,
            rewardScore = 200
        });

        // Wave objective
        activeObjectives.Add(new Objective
        {
            id = "wave_3",
            description = "Reach Wave 3",
            type = ObjectiveType.ReachWave,
            targetValue = 3,
            currentValue = 0,
            rewardScore = 300
        });

        // Combo objective
        activeObjectives.Add(new Objective
        {
            id = "combo_5",
            description = "Get a 5x combo",
            type = ObjectiveType.AchieveCombo,
            targetValue = 5,
            currentValue = 0,
            rewardScore = 150
        });

        Debug.Log($"[Objectives] Generated {activeObjectives.Count} objectives");
    }

    /// <summary>
    /// Reports progress toward objectives.
    /// </summary>
    public void ReportProgress(ObjectiveType type, int value)
    {
        foreach (var obj in activeObjectives)
        {
            if (obj.isCompleted) continue;
            if (obj.type != type) continue;

            obj.currentValue += value;
            OnObjectiveProgress?.Invoke(obj);

            if (obj.currentValue >= obj.targetValue)
            {
                CompleteObjective(obj);
            }
        }
    }

    /// <summary>
    /// Sets a max value (for combos, waves).
    /// </summary>
    public void SetMaxValue(ObjectiveType type, int value)
    {
        foreach (var obj in activeObjectives)
        {
            if (obj.isCompleted) continue;
            if (obj.type != type) continue;

            if (value > obj.currentValue)
            {
                obj.currentValue = value;
                OnObjectiveProgress?.Invoke(obj);

                if (obj.currentValue >= obj.targetValue)
                {
                    CompleteObjective(obj);
                }
            }
        }
    }

    private void CompleteObjective(Objective obj)
    {
        obj.isCompleted = true;
        OnObjectiveCompleted?.Invoke(obj);

        Debug.Log($"[Objectives] COMPLETED: {obj.description} (+{obj.rewardScore} points)");

        // Award score
        GameUI ui = FindFirstObjectByType<GameUI>();
        if (ui != null)
            ui.AddScore(obj.rewardScore);

        // Generate replacement objective
        ReplaceObjective(obj);
    }

    private void ReplaceObjective(Objective old)
    {
        int index = activeObjectives.IndexOf(old);
        if (index < 0) return;

        // Generate harder version
        Objective newObj = null;

        switch (old.type)
        {
            case ObjectiveType.KillEnemies:
                int newKillTarget = old.targetValue + 15;
                newObj = new Objective
                {
                    id = $"kill_{newKillTarget}",
                    description = $"Kill {newKillTarget} enemies",
                    type = ObjectiveType.KillEnemies,
                    targetValue = newKillTarget,
                    currentValue = 0,
                    rewardScore = old.rewardScore + 100
                };
                break;

            case ObjectiveType.ReachWave:
                int newWave = old.targetValue + 2;
                newObj = new Objective
                {
                    id = $"wave_{newWave}",
                    description = $"Reach Wave {newWave}",
                    type = ObjectiveType.ReachWave,
                    targetValue = newWave,
                    currentValue = old.currentValue,
                    rewardScore = old.rewardScore + 150
                };
                break;

            case ObjectiveType.AchieveCombo:
                int newCombo = old.targetValue + 5;
                newObj = new Objective
                {
                    id = $"combo_{newCombo}",
                    description = $"Get a {newCombo}x combo",
                    type = ObjectiveType.AchieveCombo,
                    targetValue = newCombo,
                    currentValue = 0,
                    rewardScore = old.rewardScore + 75
                };
                break;

            default:
                newObj = new Objective
                {
                    id = "bonus_kill",
                    description = "Kill 25 more enemies",
                    type = ObjectiveType.KillEnemies,
                    targetValue = 25,
                    currentValue = 0,
                    rewardScore = 250
                };
                break;
        }

        if (newObj != null)
        {
            activeObjectives[index] = newObj;
            Debug.Log($"[Objectives] New objective: {newObj.description}");
        }
    }

    public List<Objective> GetActiveObjectives() => activeObjectives;
}
