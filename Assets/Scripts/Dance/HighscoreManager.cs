using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class HighscoreEntry
{
    public string playerName;
    public float scorePercent;
}

[Serializable]
public class HighscoreData
{
    public List<HighscoreEntry> entries = new List<HighscoreEntry>();
}

public class HighscoreManager : MonoBehaviour
{
    public static HighscoreManager Instance { get; private set; }

    public string fileName = "dance_highscores.json";
    public int maxEntries = 5;

    private HighscoreData data = new HighscoreData();

    private string FilePath =>
        Path.Combine(Application.persistentDataPath, fileName);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("HighscoreManager: duplicate instance destroyed.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log($"HighscoreManager: Awake. Highscore file path = {FilePath}");

        Load();
    }

    // ------------------------------------------------------
    // PUBLIC API
    // ------------------------------------------------------

    public void AddScore(string name, float scorePercent)
    {
        var entry = new HighscoreEntry
        {
            playerName = string.IsNullOrWhiteSpace(name) ? "Player" : name,
            scorePercent = scorePercent
        };

        Debug.Log($"HighscoreManager: Adding score → {entry.playerName}: {entry.scorePercent}");

        data.entries.Add(entry);

        data.entries.Sort((a, b) => b.scorePercent.CompareTo(a.scorePercent));

        if (data.entries.Count > maxEntries)
        {
            Debug.Log($"HighscoreManager: Trimming list to {maxEntries} entries");
            data.entries.RemoveRange(maxEntries, data.entries.Count - maxEntries);
        }

        Save();
    }

    public IReadOnlyList<HighscoreEntry> GetEntries()
    {
        Debug.Log($"HighscoreManager: Returning {data.entries.Count} entries.");
        return data.entries;
    }

    // ------------------------------------------------------
    // SAVE & LOAD
    // ------------------------------------------------------

    private void Load()
    {
        if (!File.Exists(FilePath))
        {
            Debug.Log($"HighscoreManager: No existing highscore file found. Creating new.");
            data = new HighscoreData();
            return;
        }

        try
        {
            Debug.Log("HighscoreManager: Loading high score file...");
            string json = File.ReadAllText(FilePath);
            data = JsonUtility.FromJson<HighscoreData>(json);

            if (data == null || data.entries == null)
            {
                Debug.LogWarning("HighscoreManager: Loaded file was empty or invalid — resetting list.");
                data = new HighscoreData();
            }
            else
            {
                Debug.Log($"HighscoreManager: Loaded {data.entries.Count} scores.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"HighscoreManager: Failed to load highscores: {e}");
            data = new HighscoreData();
        }
    }

    private void Save()
    {
        try
        {
            Debug.Log($"HighscoreManager: Saving {data.entries.Count} highscores → {FilePath}");

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(FilePath, json);

            Debug.Log("HighscoreManager: Save complete.");
        }
        catch (Exception e)
        {
            Debug.LogError($"HighscoreManager: Save failed: {e}");
        }
    }
}
