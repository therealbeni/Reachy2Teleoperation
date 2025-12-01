using UnityEngine;
using UnityEngine.EventSystems;

public class MusicTrackSelectable : MonoBehaviour, IPointerClickHandler
{
    public int trackIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        MusicPlayer.Instance.PlayTrack(trackIndex);
    }
}
