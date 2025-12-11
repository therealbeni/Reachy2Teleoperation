using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIThemeElement : MonoBehaviour
{
    public enum ElementType { Background, Text, Button }
    public ElementType elementType;

    private Image image;
    private TextMeshProUGUI tmpText;

    void Awake()
    {
        image = GetComponent<Image>();
        tmpText = GetComponent<TextMeshProUGUI>();
    }

    public void ApplyTheme()
    {
        var theme = UIThemeManager.Instance;
        var data = theme.themeData;

        if (theme.IsDarkMode)
        {
            switch (elementType)
            {
                case ElementType.Background:
                    if (image) image.color = data.darkBackground;
                    break;

                case ElementType.Text:
                    if (tmpText) tmpText.color = data.darkText;
                    break;

                case ElementType.Button:
                    if (image) image.color = data.darkButton;
                    break;
            }
        }
        else
        {
            switch (elementType)
            {
                case ElementType.Background:
                    if (image) image.color = data.lightBackground;
                    break;

                case ElementType.Text:
                    if (tmpText) tmpText.color = data.lightText;
                    break;

                case ElementType.Button:
                    if (image) image.color = data.lightButton;
                    break;
            }
        }
    }
}
