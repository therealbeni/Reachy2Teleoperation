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

    private bool joystickRightPrev;
    private bool joystickLeftPrev;

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

        HandleJoystickTrackNavigation();

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

    void HandleJoystickTrackNavigation()
    {
        if (!isMusicMenuOpen)
            return;

        Vector2 joystick;
        controllers.leftHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystick);

        // Skip RIGHT
        if (joystick.x > 0.8f && !joystickRightPrev)
        {
            Debug.Log("MusicMenu: Next track");
            MusicSystem.Instance.NextDance();
        }

        // Skip LEFT
        if (joystick.x < -0.8f && !joystickLeftPrev)
        {
            Debug.Log("MusicMenu: Previous track");
            MusicSystem.Instance.PreviousDance();
        }

        joystickRightPrev = joystick.x > 0.8f;
        joystickLeftPrev  = joystick.x < -0.8f;
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
