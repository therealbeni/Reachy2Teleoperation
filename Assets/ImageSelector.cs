using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ImageSelector : MonoBehaviour
{
    [Header("UI")]
    public Image previewImage;

    [Header("Images to scroll through")]
    public List<Sprite> imageOptions = new List<Sprite>();

    private int currentIndex = 0;

    void Start()
    {
        if (imageOptions.Count > 0)
            UpdatePreview();
    }

    public void NextImage()
    {
        if (imageOptions.Count == 0) return;

        currentIndex = (currentIndex + 1) % imageOptions.Count;
        UpdatePreview();
    }

    public void PreviousImage()
    {
        if (imageOptions.Count == 0) return;

        currentIndex = (currentIndex - 1 + imageOptions.Count) % imageOptions.Count;
        UpdatePreview();
    }

    void UpdatePreview()
    {
        if (previewImage != null)
            previewImage.sprite = imageOptions[currentIndex];
    }

    public void ConfirmSelection()
    {
        SelectedImageData.SelectedIndex = currentIndex;
        SelectedImageData.SelectedSprite = imageOptions[currentIndex];
    }
}
