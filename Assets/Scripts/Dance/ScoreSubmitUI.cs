using TMPro;
using UnityEngine;

public class ScoreSubmitUI : MonoBehaviour
{
    public TMP_InputField nameInput;
    public ReachyDanceGrader grader;
    public ScoreboardUI scoreboardUI;

    public void OnSubmit()
    {
        if (grader == null)
        {
            Debug.LogWarning("ScoreSubmitUI: No grader assigned.");
            return;
        }

        string playerName = string.IsNullOrWhiteSpace(nameInput.text)
                            ? "Player"
                            : nameInput.text;

        // ? Read the real numeric value
        float scoreValue = grader.AverageScorePercent;

        // Store as number
        HighscoreManager.Instance.AddScore(playerName, scoreValue);

        if (scoreboardUI != null)
            scoreboardUI.Refresh();
    }
}
