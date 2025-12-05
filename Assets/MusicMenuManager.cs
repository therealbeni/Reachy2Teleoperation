using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using TeleopReachy;

public class MusicMenuManager : Singleton<MusicMenuManager>
{
    private ControllersManager controllers;

    private bool isMusicMenuOpen;
    private bool canMenuOpen;

    private bool leftYPreviouslyPressed;

    private Coroutine menuHidingCoroutine;
    private bool menuHidingRequested;

    void Start()
    {
        controllers = ActiveControllerManager.Instance.ControllersManager;

        HideImmediatelyMusicMenu();
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
                if (!isMusicMenuOpen)
                {
                    ShowMusicMenu();
                }
                else
                {
                    HideImmediatelyMusicMenu();
                }
            }

            leftYPreviouslyPressed = leftYPressed;
        }

        if (menuHidingRequested)
        {
            transform.ActivateChildren(false);
            isMusicMenuOpen = false;
            menuHidingRequested = false;
        }
    }

    public void HideAfterSeconds(float delay = 0.5f)
    {
        if (menuHidingCoroutine != null)
            StopCoroutine(menuHidingCoroutine);

        menuHidingCoroutine = StartCoroutine(HideMusicMenu(delay));
    }

    void ShowMusicMenu()
    {
        transform.ActivateChildren(true);
        isMusicMenuOpen = true;
    }

    IEnumerator HideMusicMenu(float delay)
    {
        yield return new WaitForSeconds(delay);
        menuHidingRequested = true;
    }

    void HideImmediatelyMusicMenu()
    {
        if (menuHidingCoroutine != null)
            StopCoroutine(menuHidingCoroutine);

        transform.ActivateChildren(false);
        isMusicMenuOpen = false;
        canMenuOpen = true;
    }
}
