using TMPro;
using UnityEngine;

public class ScoreboardUI : MonoBehaviour
{
    public TMP_Text[] lines;  // assign 5 TMP text objects

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        var mgr = HighscoreManager.Instance;
        if (mgr == null) return;

        var entries = mgr.GetEntries();

        for (int i = 0; i < lines.Length; i++)
        {
            if (i < entries.Count)
            {
                lines[i].text = $"{i+1}. {entries[i].playerName} - {entries[i].scorePercent:F1}%";
            }
            else
            {
                lines[i].text = "---";
            }
        }
    }
}
