using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TeleopReachy
{
    
    public class DanceSceneManager : Singleton<DanceSceneManager>
    {
        public InitializationState initializationState { get; private set; }

        [SerializeField]
        private Button leaveMirrorSceneButton;

        [SerializeField]
        private Transform resetPositionButton;

        [SerializeField]
        private Button leaveMirrorSceneButtonRobotLocked;

        [SerializeField]
        private Transform menuWarningLockPosition;

        private Transform userOrigin;

        private RobotConfig robotConfig;
        private RobotStatus robotStatus;

        private ConnectionStatus connectionStatus;

        private ControllersManager controllers;

        public float indicatorTimer { get; private set; }
        private const float minIndicatorTimer = 0.0f;

        public UnityEvent event_OnDanceInitializationStepChanged;

        // Jukebox / speakers controls (added)
        [SerializeField]
        private Transform jukebox;

        [SerializeField]
        private Transform speakers;

        [SerializeField]
        private Transform reachy_viz;

        [Header("Auto-placement")]
       
        [SerializeField]
        private float objectsHeightOffset = -0.0f;

        // Start is called before the first frame update
        void Start()
        {
            userOrigin = UserTrackerManager.Instance.transform;

            //resetPositionButton.gameObject.SetActive(false);

            controllers = ActiveControllerManager.Instance.ControllersManager;

            // Reposition when origin is fixed (keeps X/Z, only adjusts Y + yaw)
            EventManager.StartListening(EventNames.OnFixUserOrigin, MakeObjectsFaceUserOrigin);

            ResetPosition();
            if (Robot.IsCurrentRobotVirtual()) initializationState = InitializationState.NoInitializationRequired;
            else initializationState = InitializationState.WaitingForRobotReady;

            //Maybe add Listener for robotReadytoDance


            robotStatus = RobotDataManager.Instance.RobotStatus;
            robotConfig = RobotDataManager.Instance.RobotConfig;

            connectionStatus = ConnectionStatus.Instance;

            if (connectionStatus.IsRobotReady()) RobotReadyForDance();

        }

        //private void SetRobotCompliantBeforeQuittingScene()
        //{
        //    if (!robotStatus.IsRobotCompliant())
        //    {
        //        menuWarningLockPosition.ActivateChildren(true);
        //        menuWarningLockPosition.GetComponent<ExitOnLockedPositionUIManager>().QuitTransitionRoom();
        //        TeleoperationManager.Instance.AskForRobotSmoothlyCompliant();
        //        RobotDataManager.Instance.RobotStatus.event_OnRobotFullyCompliant.AddListener(BackToConnectionScene);
        //    }
        //    else
        //    {
        //        BackToConnectionScene();
        //    }
        //}

        public void ResetPosition()
        {
            FixUserOrigin();
            MakeObjectsFaceUserOrigin(); // ensure jukebox & speakers placed/oriented after origin fix
        }

        void FixUserOrigin()
        {
            EventManager.TriggerEvent(EventNames.OnFixUserOrigin);
        }

        private void MakeObjectsFaceUserOrigin()
        {
            if (userOrigin == null) userOrigin = UserTrackerManager.Instance.transform;

            // Compute target Y from the user origin; userOrigin is already placed relative to headset in UserTrackerManager
            float targetY = userOrigin.position.y + objectsHeightOffset;

            if (jukebox != null)
            {
                // keep original X/Z, only adjust Y and yaw to face the user
                Vector3 jukeboxPos = jukebox.position;
                jukeboxPos.y = targetY;
                jukebox.position = jukeboxPos;

                // align yaw to user origin (keep only Y rotation)
                //jukebox.rotation = Quaternion.Euler(0f, userOrigin.eulerAngles.y, 0f);
            }

            if (speakers != null)
            {
                // keep original X/Z, only adjust Y and yaw to face the user
                Vector3 speakersPos = speakers.position;
                speakersPos.y = targetY;
                speakers.position = speakersPos;

                //speakers.rotation = Quaternion.Euler(0f, userOrigin.eulerAngles.y, 0f);

                // do not change children X/Z positions — keep original layout
                // If you want to nudge left/right children you can still access them here, but leave default behavior to preserve scene layout
            }

            if (reachy_viz != null)
            {
                // keep original X/Z, only adjust Y and yaw to face the user
                Vector3 reachyVizPos = reachy_viz.position;
                reachyVizPos.y = targetY;
                reachy_viz.position = reachyVizPos;
                //reachy_viz.rotation = Quaternion.Euler(0f, userOrigin.eulerAngles.y, 0f);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }


        protected void ValidateUserOrigin()
        {
            ResetPosition();
            initializationState = InitializationState.ReadyForTeleop;
            event_OnDanceInitializationStepChanged.Invoke();
            resetPositionButton.gameObject.SetActive(true);

        }

        protected void RobotReadyForDance()
        {
            initializationState = InitializationState.WaitingForUserOriginValidation;
            event_OnDanceInitializationStepChanged.Invoke();
        }

        protected void BackToConnectionScene()
        {
            EventManager.TriggerEvent(EventNames.OnReinitializeLimitsRequested);
        }
    }
}

