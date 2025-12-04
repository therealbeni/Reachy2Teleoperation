using UnityEngine;
using System.Collections;
using UnityEngine.XR;
using TeleopReachy;

public class MusicMenuManager : MonoBehaviour
{
    public static MusicMenuManager Instance;

    public bool IsMenuOpen { get; private set; }

    private bool rightSecondaryButtonPrev;
    private bool canMenuOpen = true;

    private ControllersManager controllers;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        controllers = ActiveControllerManager.Instance.ControllersManager;
        HideMenuImmediate();
    }

    void Update()
    {
        bool rightSecondaryPressed = false;

        controllers.rightHandDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out rightSecondaryPressed);

        if (canMenuOpen && rightSecondaryPressed && !rightSecondaryButtonPrev)
        {
            if (!IsMenuOpen)
                ShowMenu();
            else
                HideMenuImmediate();
        }

        rightSecondaryButtonPrev = rightSecondaryPressed;

        
    }

    public void ShowMenu()
    {
        transform.ActivateChildren(true); // same helper used in EmotionMenu
        IsMenuOpen = true;
    }

    public void HideMenuImmediate()
    {
        transform.ActivateChildren(false);
        IsMenuOpen = false;
    }
}
