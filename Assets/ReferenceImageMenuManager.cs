using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using TeleopReachy;

public class ReferenceImageMenuManager : Singleton<ReferenceImageMenuManager>
{
    private ControllersManager controllers;

    private bool isMenuOpen;
    private bool canMenuOpen;

    private bool leftYPreviouslyPressed;

    private Coroutine menuHidingCoroutine;
    private bool menuHidingRequested;

    [Header("UI")]
    public Image referenceImageDisplay; // Image showing selected reference

    void Start()
    {
        controllers = ActiveControllerManager.Instance.ControllersManager;

        // Set selected image (new line)
        if (SelectedImageData.SelectedSprite != null)
            referenceImageDisplay.sprite = SelectedImageData.SelectedSprite;

        HideImmediatelyMenu();
        canMenuOpen = true;
        menuHidingRequested = false;
    }

    void Update()
    {
        bool leftYPressed = false;

        if (canMenuOpen)
        {
            if (controllers.leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out leftYPressed)
                && leftYPressed
                && !leftYPreviouslyPressed)
            {
                if (!isMenuOpen)
                {
                    ShowMenu();
                }
                else
                {
                    HideImmediatelyMenu();
                }
            }

            leftYPreviouslyPressed = leftYPressed;
        }

        if (menuHidingRequested)
        {
            transform.ActivateChildren(false);
            isMenuOpen = false;
            menuHidingRequested = false;
        }
    }

    public void HideAfterSeconds(float delay = 0.5f)
    {
        if (menuHidingCoroutine != null)
            StopCoroutine(menuHidingCoroutine);

        menuHidingCoroutine = StartCoroutine(HideMenu(delay));
    }

    void ShowMenu()
    {
        transform.ActivateChildren(true);
        isMenuOpen = true;
    }

    IEnumerator HideMenu(float delay)
    {
        yield return new WaitForSeconds(delay);
        menuHidingRequested = true;
    }

    void HideImmediatelyMenu()
    {
        if (menuHidingCoroutine != null)
            StopCoroutine(menuHidingCoroutine);

        transform.ActivateChildren(false);
        isMenuOpen = false;
        canMenuOpen = true;
    }
}
