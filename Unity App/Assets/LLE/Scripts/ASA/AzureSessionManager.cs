using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.SpatialAnchors.Unity;
using System.Timers;

using Microsoft.Azure.SpatialAnchors;
using UnityEngine.UI;



using UnityEngine.Events;
using LLE.Rx;
using System;
using LLE.Unity;

namespace LLE.ASA
{
    //[RequireComponent(typeof(SpatialAnchorManager))]
    public class AzureSessionManager : MonoBehaviour
    {

        public enum WatchMode { IDENTIFIER, NEAR_ANCHOR, NEAR_DEVICE, NA }
        public int NearAnchorDistance = 999;
        public int NearAnchorMaxCount = 35;
        public UnityEvent<GameObject> OnAnchorCreated { get; private set; } = new UnityEvent<GameObject>();
        public UnityEvent<AnchorWrapper> OnAnchorLocated = new UnityEvent<AnchorWrapper>();
        public UnityEvent<string> OnAnchorDeleted = new UnityEvent<string>();
        public UnityEvent OnLocateComplete = new UnityEvent();

        public GameObject parentAnchor;
        public GameObject locatedAnchorPrefab;

        [Tooltip("If set the azure session will be started automatically")]
        public bool isStartSession = false;
        [Tooltip("Delay in [ms]")]
        public int AzureStartUpDelay = 5000;

        public Toggle toggleIsStartSession;
        public Toggle toggleIsBypassCache;

        public bool isSessionBypassCache = false;
        public AbstractAzureWrapper AzureAdapter;

        public static AzureSessionManager currentAzureManager;

        public UnityEvent<bool> OnSessionReady = new UnityEvent<bool>();

        private bool mSessionStarted;
        private static string SESSION_AUTO_START = "seesion_auto_start";
        private static string SESSION_BYPASS_CACHE = "seesion_bypass_cache";
        private bool isCreatingAnchor;

        private void Awake()
        {
            if (AzureAdapter == null)
                AzureAdapter = GetComponent<AbstractAzureWrapper>();
            if (locatedAnchorPrefab.GetComponent<ToggleVisibility>() == null)
                Debug.LogError("AzureSession: located anchor prefab requires a configured ToggleVisibilty component!");
        }
        void Start()
        {

            Debug.Log("Start ASM");
            AzureAdapter.callbacks.OnAnchorCreated.AddListener(go =>
            {
                Debug.Log("ASM: OnAnchorCreated -> " + go.name);
                isCreatingAnchor = false;
                OnAnchorCreated.Invoke(go);
            }
            );

            AzureAdapter.callbacks.OnAnchorLocated.AddListener(a => OnAnchorLocated.Invoke(a));
            AzureAdapter.callbacks.OnAnchorDeleted.AddListener(a => OnAnchorDeleted.Invoke(a));

            AzureAdapter.callbacks.OnLocateComplete.AddListener(() =>
            {
                Debug.Log("ASM: OnLocateComplete -> invoke event");
                OnLocateComplete.Invoke();
            });

            //AzureAdapter.callbacks.OnNearAnchor.AddListener(nearList => OnNearAnchor.Invoke(nearList));
            if (toggleIsBypassCache != null && toggleIsStartSession != null)
            {
                isStartSession = PlayerPrefs.GetInt(SESSION_AUTO_START, 0) == 1;
                isSessionBypassCache = PlayerPrefs.GetInt(SESSION_BYPASS_CACHE, 0) == 1;

                toggleIsStartSession.SetIsOnWithoutNotify(isStartSession);
                toggleIsBypassCache.SetIsOnWithoutNotify(isSessionBypassCache);
            }
            else
            {
                isStartSession = true;
                isSessionBypassCache = true;
            }
            if (isStartSession)
            {
                Timer timer = new Timer(AzureStartUpDelay);
                timer.Elapsed += (s, e) =>
                {
                    // required?
                    UnityDispatcher.InvokeOnAppThread(() =>
                    {
                        Debug.Log("Delayed start of Azure Seesion...");
                        StartSession();
                    });
                };
                timer.AutoReset = false;
                timer.Enabled = true;
            }
            currentAzureManager = this;

            Debug.Log("Start ASM...done");
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();

        }
        public void StartSession()
        {
            mSessionStarted = true;
            AzureAdapter.StartSession();
            AzureAdapter.SetBypassCache(isSessionBypassCache);

            Timer timer = new Timer(2000);
            timer.Elapsed += (s, e) =>
            {
                Debug.Log("AzureSeesion: OnSeesionReady Invoke");
                OnSessionReady.Invoke(true);
            };
            timer.AutoReset = false;
            timer.Enabled = true;
        }
        public void ResetSession()
        {
            Debug.Log("Reset session");
            AzureAdapter.ResetSession();
        }
        public void SetSessionBypassCache(bool isBypassCache)
        {
            Debug.Log("Seesion: bypass cache = " + isBypassCache);
            this.isSessionBypassCache = isBypassCache;
            AzureAdapter.SetBypassCache(isSessionBypassCache);

            PlayerPrefs.SetInt(SESSION_BYPASS_CACHE, isBypassCache ? 1 : 0);
            PlayerPrefs.Save();
        }
        public void SetAutoStart(bool isAutoStart)
        {
            Debug.Log("Seesion: auto start = " + isAutoStart);
            this.isStartSession = isAutoStart;
            var val = isAutoStart ? 1 : 0;
            PlayerPrefs.SetInt(SESSION_AUTO_START, val);

            if (PlayerPrefs.GetInt(SESSION_AUTO_START, -10) != val)
                Debug.Log("err prefs");
            PlayerPrefs.Save();
        }
        public void SetDistanceNearAnchor(string number)
        {
            Debug.Log("Set near distance: " + number);
            int val;
            if (int.TryParse(number, out val))
            {
                NearAnchorDistance = val;
            }
        }
        public void SetMaxNearAnchor(string number)
        {
            Debug.Log("Set max Anchor: " + number);
            int val;
            if (int.TryParse(number, out val))
            {
                NearAnchorMaxCount = val;
            }
        }
        public void CreateAnchor()
        {
            if (!isCreatingAnchor)
            {
                isCreatingAnchor = true;
                Debug.Log("Azure Session Manager: create anchor at " + parentAnchor.transform.position);
                AzureAdapter.CreateAnchor(parentAnchor.transform.position, parentAnchor.transform.rotation, locatedAnchorPrefab);
            }
            else
            {
                Debug.Log("Azure Session Manager: already busy with creating an anchor");
            }
         
        }
        public void FindAnchor(string anchorID)
        {
            Debug.Log("Find anchor: " + anchorID);
            FindAnchor(new string[] { anchorID });
        }

        public void FindAnchor(ICollection<string> anchorIDs)
        {
            Debug.Log("AzureSessionManager: FindAnchor -> count = " + anchorIDs.Count);
            if (AzureAdapter == null)
                Debug.Log("AzureAdapter null");
            else
                AzureAdapter.FindAnchor(anchorIDs);
        }

        public void StopFindAnchor(string anchorID)
        {
            Debug.Log("Stop find anchor: " + anchorID);
            StopFindAnchor(new string[] { anchorID });
        }


        public void StopFindAnchor(ICollection<string> anchorIDs)
        {
            AzureAdapter.StopFindAnchor(anchorIDs);
        }
        public void DeleteAnchor(CloudSpatialAnchor anchor)
        {
            try
            {
                Debug.Log("ASM: delete anchor -> " + anchor.Identifier);
                AzureAdapter.DeleteAnchor(anchor);
            }
            catch (Exception e)
            {

                Debug.LogErrorFormat("Delete Anchor: id = {0}, exception -> {1}", anchor.Identifier, e);
            }

        }
        public void GetNearAnchor(AnchorWrapper anchor)
        {
            Debug.Log("Azure: GetNearAnchor of id = " + anchor.identifier);
            //https://github.com/Azure/azure-spatial-anchors-samples/blob/79dc2ee96d027f9eeeab56767b5013d533bced63/Unity/Assets/AzureSpatialAnchors.Examples/Scripts/DemoScriptBase.cs#L225

            AzureAdapter.GetNearAnchor(anchor, NearAnchorDistance, NearAnchorMaxCount);
        }

        public void GetAnchorNearDevice()
        {
            AzureAdapter.GetAnchorNearDevice(NearAnchorDistance, NearAnchorMaxCount);
        }
        public void StopAzureSession()
        {
            if (mSessionStarted)
                AzureAdapter.StopSession();
            mSessionStarted = false;
        }
        private void OnDestroy()
        {
            if (mSessionStarted)
                AzureAdapter.StopSession();
        }
    }
}