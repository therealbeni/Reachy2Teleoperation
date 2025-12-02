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

        [Header("Auto-placement")]
        [SerializeField]
        private float distanceToObjects = 2.0f;

        [SerializeField]
        private float objectsHeightOffset = -0.0f;

        // Start is called before the first frame update
        void Start()
        {
            userOrigin = UserTrackerManager.Instance.transform;

            //resetPositionButton.gameObject.SetActive(false);

            controllers = ActiveControllerManager.Instance.ControllersManager;

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

            if (jukebox != null)
            {
                jukebox.position = userOrigin.TransformPoint(Vector3.forward * distanceToObjects);
                jukebox.position = new Vector3(jukebox.position.x, jukebox.position.y + objectsHeightOffset, jukebox.position.z);
                jukebox.rotation = userOrigin.localRotation;
            }

            if (speakers != null)
            {
                // place speakers slightly to the sides of the jukebox
                Vector3 rightOffset = userOrigin.TransformDirection(Vector3.right * 0.7f); // tweak as needed
                Vector3 leftOffset = userOrigin.TransformDirection(Vector3.right * -0.7f);

                // Primary speakers container centered in front of user, then children can be left/right
                speakers.position = userOrigin.TransformPoint(Vector3.forward * (distanceToObjects - 0.1f));
                speakers.position = new Vector3(speakers.position.x, speakers.position.y + objectsHeightOffset, speakers.position.z);
                speakers.rotation = userOrigin.localRotation;

                // If speakers has two child transforms named "Left" and "Right" you can position them like this:
                Transform left = speakers.Find("Left");
                Transform right = speakers.Find("Right");
                if (left != null) left.position = speakers.position + leftOffset;
                if (right != null) right.position = speakers.position + rightOffset;
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

