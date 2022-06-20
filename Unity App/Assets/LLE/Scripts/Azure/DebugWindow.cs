using Microsoft.Azure.SpatialAnchors.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MRTK.Tutorials.AzureSpatialAnchors
{
    public class DebugWindow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI debugText = default;

        private ScrollRect scrollRect;

        private void Awake()
        {
            // Cache references
            scrollRect = GetComponentInChildren<ScrollRect>();

            // Subscribe to log message events
            // https://answers.unity.com/questions/714590/thread-safety-and-debuglog.html
            //Application.logMessageReceived += HandleLog;
            Application.logMessageReceivedThreaded += HandleLog;
            

            // Set the starting text
            debugText.text = "";
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string message, string stackTrace, LogType type)
        {
            UnityDispatcher.InvokeOnAppThread(() =>
            {
                debugText.text += message + " \n";
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0;
            });
        }
        public void ClearLog()
        {
            UnityDispatcher.InvokeOnAppThread(() =>
            {
                debugText.text = "";
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0;
            });
        }

    }
}
