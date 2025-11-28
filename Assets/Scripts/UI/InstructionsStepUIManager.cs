using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


namespace TeleopReachy
{
    public class InstructionsStepUIManager : MonoBehaviour
    {
        private MirrorSceneManager sceneManager;

        private DanceSceneManager danceSceneManager;

        [SerializeField]
        private InitializationState instructionsStep;

        private bool needUpdateInstructions;

        void Start()
        {
            sceneManager = MirrorSceneManager.Instance;
            danceSceneManager = DanceSceneManager.Instance;
            needUpdateInstructions = false;

            if (sceneManager != null)
                sceneManager.event_OnTeleopInitializationStepChanged.AddListener(CheckInstructions);
            else if (danceSceneManager != null)
                danceSceneManager.event_OnDanceInitializationStepChanged.AddListener(CheckInstructions);

            CheckInstructions();
        }

        void CheckInstructions()
        {
            needUpdateInstructions = true;
        }

        void Update()
        {
            if(needUpdateInstructions)
            {
                needUpdateInstructions = false;

                if (danceSceneManager != null)
                {
                    transform.ActivateChildren(danceSceneManager.initializationState == instructionsStep);
                    return;
                }
                if (sceneManager != null)
                {
                    transform.ActivateChildren(sceneManager.initializationState == instructionsStep);
                    return;
                }
                    
            }
        }
    }
}
