using UnityEngine;
using System.Collections;
using UnityEngine.XR;
using TeleopReachy;

public class MusicMenuManager : MonoBehaviour
{
    public static MusicMenuManager Instance;

    public bool IsMenuOpen { get; private set; }

    private bool leftPrimaryButtonPrev;
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
        bool leftPrimaryPressed = false;

        // Detect Y button like EmotionMenu
        if (canMenuOpen &&
            controllers.leftHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out leftPrimaryPressed))
        {
            if (leftPrimaryPressed && !leftPrimaryButtonPrev)
            {
                if (!IsMenuOpen)
                    ShowMenu();
                else
                    HideMenuImmediate();
            }

            leftPrimaryButtonPrev = leftPrimaryPressed;
        }
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
