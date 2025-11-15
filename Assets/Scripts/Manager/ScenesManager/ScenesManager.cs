using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

namespace TeleopReachy
{
    public class ScenesManager : Singleton<ScenesManager>
    {
        public GameObject userTracker = null;
        public GameObject userInput = null;
        public GameObject ground = null;
        public GameObject XROrigin = null;

        // ---------------------------------------------------------------------
        // Scene name constants (must match .unity file names exactly)
        // ---------------------------------------------------------------------

        // Entry and menu
        private const string CONNECTION_SCENE = "ConnectionScene";
        private const string MENU_SCENE = "MenuScene";

        // Safety scenes for the three modes
        private const string SAFETY_DANCE_AFTER_REACHY = "SafetySceneDanceReachy"; // Mode 1: Dance after Reachy (no game scene yet)
        private const string SAFETY_TABLETOP = "SafetySceneTabletop";    // Mode 2: Tabletop
        private const string SAFETY_DANCE_TELEOP = "SafetyScene";            // Mode 3: Dance Teleop

        // Robot data / shared backend
        private const string ROBOT_DATA_SCENE = "RobotDataScene";

        // Mirror scenes
        private const string MIRROR_DEFAULT = "MirrorScene";                // Original Pollen mirror (not used from menu yet)
        private const string MIRROR_TABLETOP = "TabletopMirrorScene";        // Mode 2 mirror
        private const string MIRROR_DANCE = "DanceMirrorScene";           // Mode 3 mirror

        // Teleoperation scenes
        private const string TELEOP_DEFAULT = "TeleoperationScene";             // Original Pollen teleop (not used from menu yet)
        private const string TELEOP_TABLETOP = "TabletopTeleoperationScene";     // Mode 2 teleop
        private const string TELEOP_DANCE = "DanceTeleoperationScene";        // Mode 3 teleop

        // ---------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------

        private void Start()
        {
            // BaseScene (build index 0) starts with this manager.
            // First thing: show ConnectionScene with tracking OFF.
            LoadConnectionScene();

            // Global quit
            EventManager.StartListening(EventNames.QuitApplication, QuitApplication);

            // Back from any VR context (mirror or teleop) to connection
            EventManager.StartListening(EventNames.EnterConnectionScene, LoadConnectionSceneEndUnloadVRScenes);

            // ConnectionScene → MenuScene after successful connect
            EventManager.StartListening(EventNames.QuitConnectionScene, UnloadConnectionSceneAndLoadMenuScene);

            // Menu → Safety (three modes)
            EventManager.StartListening(EventNames.EnterSafetyDanceAfterReachyScene, LoadSafetyDanceAfterReachyEndUnloadMenu);
            EventManager.StartListening(EventNames.EnterSafetyTabletopScene, LoadSafetyTabletopEndUnloadMenu);
            EventManager.StartListening(EventNames.EnterSafetyDanceWithReachyScene, LoadSafetyDanceTeleopEndUnloadMenu);

            // Safety → Mirror
            // Mode 1: DanceAfterReachy (placeholder, no game scene yet)
            EventManager.StartListening(EventNames.EnterMirrorScene, UnloadSafetyDanceAfterReachyAndPlaceholder);

            // Mode 2: Tabletop → TabletopMirrorScene
            EventManager.StartListening(EventNames.EnterTabletopMirrorScene, UnloadSafetyTabletopAndLoadTabletopMirror);

            // Mode 3: Dance Teleop → DanceMirrorScene
            EventManager.StartListening(EventNames.EnterDanceMirrorScene, UnloadSafetyDanceTeleopAndLoadDanceMirror);

            // Mirror ↔ Teleoperation (all modes)
            EventManager.StartListening(EventNames.EnterTeleoperationScene, LoadTeleoperationSceneAndUnloadCurrentMirror);
            EventManager.StartListening(EventNames.QuitTeleoperationScene, UnloadTeleoperationSceneAndLoadCorrespondingMirror);

            EventManager.StartListening(EventNames.EnterMenuFromSafetyScene, UnloadSafetyLoadMenu);

            // X-Ray UI
            EventManager.StartListening(EventNames.ShowXRay, ShowXRay);
            EventManager.StartListening(EventNames.HideXRay, HideXRay);
        }

        private void QuitApplication()
        {
            Debug.Log("Exiting app");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ---------------------------------------------------------------------
        // Helper utilities
        // ---------------------------------------------------------------------

        private void UnloadSafetyLoadMenu()
        {
            UnloadSceneIfLoaded(SAFETY_DANCE_AFTER_REACHY);
            UnloadSceneIfLoaded(SAFETY_TABLETOP);
            UnloadSceneIfLoaded(SAFETY_DANCE_TELEOP);
            ground.SetActive(true);
            SetTrackingEnabled(false); // non-VR
            if (!SceneManager.GetSceneByName(MENU_SCENE).isLoaded)
                SceneManager.LoadScene(MENU_SCENE, LoadSceneMode.Additive);
        }

        private void SetTrackingEnabled(bool enabled)
        {
            if (userTracker != null) userTracker.SetActive(enabled);
            if (userInput != null) userInput.SetActive(enabled);
        }

        private void UnloadSceneIfLoaded(string sceneName)
        {
            if (SceneManager.GetSceneByName(sceneName).isLoaded)
                SceneManager.UnloadSceneAsync(sceneName);
        }

        private void UnloadAllMirrorScenes()
        {
            UnloadSceneIfLoaded(MIRROR_DEFAULT);
            UnloadSceneIfLoaded(MIRROR_TABLETOP);
            UnloadSceneIfLoaded(MIRROR_DANCE);
            ground.SetActive(false);
        }

        private void UnloadAllTeleopScenes()
        {
            UnloadSceneIfLoaded(TELEOP_DEFAULT);
            UnloadSceneIfLoaded(TELEOP_TABLETOP);
            UnloadSceneIfLoaded(TELEOP_DANCE);
        }

        // ---------------------------------------------------------------------
        // Connection / Menu
        // ---------------------------------------------------------------------

        private void LoadConnectionScene()
        {
            Debug.Log("Loading Connection Scene");

            ground.SetActive(true);
            SetTrackingEnabled(false); // no VR in Connection

            if (!SceneManager.GetSceneByName(CONNECTION_SCENE).isLoaded)
                SceneManager.LoadScene(CONNECTION_SCENE, LoadSceneMode.Additive);
        }

        private void LoadConnectionSceneEndUnloadVRScenes()
        {
            // Return from any VR context (mirror or teleop) to connection
            UnloadSceneIfLoaded(ROBOT_DATA_SCENE);
            UnloadAllMirrorScenes();
            UnloadAllTeleopScenes();
            UnloadSceneIfLoaded(MENU_SCENE);

            SetTrackingEnabled(false);
            LoadConnectionScene();
        }

        private void UnloadConnectionSceneAndLoadMenuScene()
        {
            // Called when ConnectionManager triggers QuitConnectionScene
            UnloadSceneIfLoaded(CONNECTION_SCENE);

            ground.SetActive(true);
            SetTrackingEnabled(false); // still non-VR

            if (!SceneManager.GetSceneByName(MENU_SCENE).isLoaded)
                SceneManager.LoadScene(MENU_SCENE, LoadSceneMode.Additive);
        }

        // --------------------------------------------------------------------
        // Menu -> Safety
        // --------------------------------------------------------------------

        private void LoadSafetyDanceAfterReachyEndUnloadMenu()
        {
            UnloadSceneIfLoaded(MENU_SCENE);
            LoadSafetyScene(SAFETY_DANCE_AFTER_REACHY);
        }

        private void LoadSafetyTabletopEndUnloadMenu()
        {
            UnloadSceneIfLoaded(MENU_SCENE);
            LoadSafetyScene(SAFETY_TABLETOP);
        }

        private void LoadSafetyDanceTeleopEndUnloadMenu()
        {
            UnloadSceneIfLoaded(MENU_SCENE);
            LoadSafetyScene(SAFETY_DANCE_TELEOP);
        }

        private void LoadSafetyScene(string safetySceneName)
        {
            Debug.Log("Loading Safety Scene: " + safetySceneName);

            ground.SetActive(true);
            SetTrackingEnabled(false); // still non-VR

            if (!SceneManager.GetSceneByName(safetySceneName).isLoaded)
                SceneManager.LoadScene(safetySceneName, LoadSceneMode.Additive);
        }

        // --------------------------------------------------------------------
        // Safety -> Mirror
        // --------------------------------------------------------------------

        // MODE 1: DanceAfterReachy – currently only logs; no real game/mirror yet.
        private void UnloadSafetyDanceAfterReachyAndPlaceholder()
        {
            Debug.Log("[ScenesManager] Mode 1 (DanceAfterReachy) next scene not implemented yet.");
            // When you have a follow-up scene, implement it here.
        }

        // MODE 2: Tabletop
        private void UnloadSafetyTabletopAndLoadTabletopMirror()
        {
            UnloadSceneIfLoaded(SAFETY_TABLETOP);
            SetTrackingEnabled(true); // entering VR
            StartCoroutine(LoadRobotDataSceneAndMirrorScene(MIRROR_TABLETOP));
        }

        // MODE 3: Dance Teleop
        private void UnloadSafetyDanceTeleopAndLoadDanceMirror()
        {
            UnloadSceneIfLoaded(SAFETY_DANCE_TELEOP);
            SetTrackingEnabled(true); // entering VR
            StartCoroutine(LoadRobotDataSceneAndMirrorScene(MIRROR_DANCE));
        }

        private IEnumerator LoadRobotDataSceneAndMirrorScene(string mirrorSceneName)
        {
            // Pollen’s original sequence: load RobotDataScene, fire event, then mirror.
            if (!SceneManager.GetSceneByName(ROBOT_DATA_SCENE).isLoaded)
            {
                SceneManager.LoadScene(ROBOT_DATA_SCENE, LoadSceneMode.Additive);
                yield return null;
                EventManager.TriggerEvent(EventNames.RobotDataSceneLoaded);
            }

            LoadMirrorScene(mirrorSceneName);
        }

        private void LoadMirrorScene(string mirrorSceneName)
        {
            ground.SetActive(true);
            StartCoroutine(LoadTransitionRoom(mirrorSceneName));
        }

        private IEnumerator LoadTransitionRoom(string mirrorSceneName)
        {
            // Wait until RobotDataScene is fully loaded
            while (!SceneManager.GetSceneByName(ROBOT_DATA_SCENE).isLoaded)
                yield return null;

            if (!SceneManager.GetSceneByName(mirrorSceneName).isLoaded)
                SceneManager.LoadScene(mirrorSceneName, LoadSceneMode.Additive);

            yield return null;
            EventManager.TriggerEvent(EventNames.MirrorSceneLoaded);
        }

        // --------------------------------------------------------------------
        // MIRROR -> TELEOPERATION  (keep Pollen logic, only scene name differs)
        // --------------------------------------------------------------------

        private void LoadTeleoperationSceneAndUnloadCurrentMirror()
        {
            string teleopSceneName = TELEOP_DEFAULT;

            if (SceneManager.GetSceneByName(MIRROR_TABLETOP).isLoaded)
                teleopSceneName = TELEOP_TABLETOP;
            else if (SceneManager.GetSceneByName(MIRROR_DANCE).isLoaded)
                teleopSceneName = TELEOP_DANCE;

            StartCoroutine(LoadTeleoperationRoom(teleopSceneName));

            // Exit any mirror scene; tracking stays ON
            UnloadAllMirrorScenes();
        }

        private IEnumerator LoadTeleoperationRoom(string teleopSceneName)
        {
            SceneManager.LoadScene(teleopSceneName, LoadSceneMode.Additive);
            yield return null;
            EventManager.TriggerEvent(EventNames.TeleoperationSceneLoaded);
        }

        // --------------------------------------------------------------------
        // TELEOPERATION -> MIRROR (back to correct variant)
        // --------------------------------------------------------------------

        private void UnloadTeleoperationSceneAndLoadCorrespondingMirror()
        {
            string mirrorSceneName = MIRROR_DEFAULT;

            if (SceneManager.GetSceneByName(TELEOP_TABLETOP).isLoaded)
                mirrorSceneName = MIRROR_TABLETOP;
            else if (SceneManager.GetSceneByName(TELEOP_DANCE).isLoaded)
                mirrorSceneName = MIRROR_DANCE;

            UnloadAllTeleopScenes();
            LoadMirrorScene(mirrorSceneName);
        }

        // --------------------------------------------------------------------
        // X-Ray helpers
        // --------------------------------------------------------------------

        void ShowXRay()
        {
            ToggleXRRayInteractors(true);
        }

        void HideXRay()
        {
            ToggleXRRayInteractors(false);
        }

        void ToggleXRRayInteractors(bool activated)
        {
            if (XROrigin == null) return;

            XRInteractorLineVisual[] xrlines = XROrigin.GetComponentsInChildren<XRInteractorLineVisual>();
            foreach (XRInteractorLineVisual xr in xrlines)
                xr.enabled = activated;
        }
    }
}