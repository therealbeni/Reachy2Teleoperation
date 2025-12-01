using UnityEngine;
using UnityEngine.UI;

public class MusicMenuUIManager : MonoBehaviour
{
    public static MusicMenuUIManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void HighlightTrack(int index)
    {
        HighlightNone(false);

        var item = transform.GetChild(index);
        item.localScale = new Vector3(1.4f, 1.4f, 1.4f);
        item.GetComponent<RawImage>().color = new Color32(255, 255, 255, 200);
    }

    public void HighlightNone(bool interactable = true)
    {
        foreach (Transform child in transform)
        {
            child.localScale = Vector3.one;

            if (interactable)
                child.GetComponent<RawImage>().color = new Color32(255, 255, 255, 150);
            else
                child.GetComponent<RawImage>().color = new Color32(80, 80, 80, 150);
        }
    }
}
