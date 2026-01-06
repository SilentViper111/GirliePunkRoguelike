using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Local leaderboard for high scores.
/// Persists using PlayerPrefs.
/// 
/// Reference: KB Section VII - Progression
/// </summary>
public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    [System.Serializable]
    public class LeaderboardEntry
    {
        public string playerName;
        public int score;
        public int wavesReached;
        public string date;
    }

    [Header("Settings")]
    [SerializeField] private int maxEntries = 10;
    [SerializeField] private string defaultPlayerName = "PLAYER";

    [Header("Current Run")]
    [SerializeField] private string currentPlayerName;
    [SerializeField] private int currentScore;
    [SerializeField] private int currentWave;

    private List<LeaderboardEntry> _entries = new List<LeaderboardEntry>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentPlayerName = PlayerPrefs.GetString("PlayerName", defaultPlayerName);
        LoadLeaderboard();
    }

    /// <summary>
    /// Sets the player name.
    /// </summary>
    public void SetPlayerName(string name)
    {
        currentPlayerName = string.IsNullOrEmpty(name) ? defaultPlayerName : name;
        PlayerPrefs.SetString("PlayerName", currentPlayerName);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Updates current run stats.
    /// </summary>
    public void UpdateCurrentRun(int score, int wave)
    {
        currentScore = score;
        currentWave = wave;
    }

    /// <summary>
    /// Submits current run to leaderboard.
    /// </summary>
    public bool SubmitScore()
    {
        return SubmitScore(currentPlayerName, currentScore, currentWave);
    }

    /// <summary>
    /// Submits a score to the leaderboard.
    /// Returns true if it's a new high score.
    /// </summary>
    public bool SubmitScore(string name, int score, int waves)
    {
        LeaderboardEntry entry = new LeaderboardEntry
        {
            playerName = name,
            score = score,
            wavesReached = waves,
            date = System.DateTime.Now.ToString("yyyy-MM-dd")
        };

        _entries.Add(entry);
        _entries = _entries.OrderByDescending(e => e.score).Take(maxEntries).ToList();
        
        SaveLeaderboard();

        bool isHighScore = _entries.IndexOf(entry) == 0 && _entries.Count > 1;
        if (isHighScore)
        {
            Debug.Log($"[Leaderboard] NEW HIGH SCORE: {score}!");
        }

        return _entries.Contains(entry);
    }

    /// <summary>
    /// Gets leaderboard entries.
    /// </summary>
    public List<LeaderboardEntry> GetEntries() => _entries;

    /// <summary>
    /// Gets the current high score.
    /// </summary>
    public int GetHighScore() => _entries.Count > 0 ? _entries[0].score : 0;

    /// <summary>
    /// Gets rank of a score.
    /// </summary>
    public int GetRank(int score)
    {
        int rank = 1;
        foreach (var entry in _entries)
        {
            if (score >= entry.score) return rank;
            rank++;
        }
        return rank;
    }

    /// <summary>
    /// Checks if score would make the leaderboard.
    /// </summary>
    public bool WouldMakeLeaderboard(int score)
    {
        if (_entries.Count < maxEntries) return true;
        return score > _entries.Last().score;
    }

    private void SaveLeaderboard()
    {
        for (int i = 0; i < maxEntries; i++)
        {
            if (i < _entries.Count)
            {
                PlayerPrefs.SetString($"leaderboard_{i}_name", _entries[i].playerName);
                PlayerPrefs.SetInt($"leaderboard_{i}_score", _entries[i].score);
                PlayerPrefs.SetInt($"leaderboard_{i}_waves", _entries[i].wavesReached);
                PlayerPrefs.SetString($"leaderboard_{i}_date", _entries[i].date);
            }
            else
            {
                PlayerPrefs.DeleteKey($"leaderboard_{i}_name");
                PlayerPrefs.DeleteKey($"leaderboard_{i}_score");
                PlayerPrefs.DeleteKey($"leaderboard_{i}_waves");
                PlayerPrefs.DeleteKey($"leaderboard_{i}_date");
            }
        }
        PlayerPrefs.Save();
    }

    private void LoadLeaderboard()
    {
        _entries.Clear();

        for (int i = 0; i < maxEntries; i++)
        {
            string name = PlayerPrefs.GetString($"leaderboard_{i}_name", "");
            if (string.IsNullOrEmpty(name)) break;

            _entries.Add(new LeaderboardEntry
            {
                playerName = name,
                score = PlayerPrefs.GetInt($"leaderboard_{i}_score", 0),
                wavesReached = PlayerPrefs.GetInt($"leaderboard_{i}_waves", 0),
                date = PlayerPrefs.GetString($"leaderboard_{i}_date", "")
            });
        }
    }

    /// <summary>
    /// Clears the leaderboard.
    /// </summary>
    public void ClearLeaderboard()
    {
        _entries.Clear();
        SaveLeaderboard();
    }
}
