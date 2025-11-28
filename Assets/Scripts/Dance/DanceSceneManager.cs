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
        }

        void FixUserOrigin()
        {
            EventManager.TriggerEvent(EventNames.OnFixUserOrigin);
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

