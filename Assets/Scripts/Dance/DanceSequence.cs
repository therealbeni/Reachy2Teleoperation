using UnityEngine;

[CreateAssetMenu(fileName = "NewDanceSequence", menuName = "Reachy/Dance Sequence")]
public class DanceSequence : ScriptableObject
{
    public string sequenceName;

    public DanceStep[] steps;
}
