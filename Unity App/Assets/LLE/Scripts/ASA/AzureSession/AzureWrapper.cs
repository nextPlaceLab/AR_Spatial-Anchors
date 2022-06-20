using LLE.Rx;
using LLE.Unity;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using static LLE.ASA.AzureSessionManager;

namespace LLE.ASA
{
    class AzureWrapper : AbstractAzureWrapper
    {
        [SerializeField]
        private SessionLogLevel ASALogLevel = SessionLogLevel.None;

        private SpatialAnchorManager cloudManager;
        private AnchorLocateCriteria anchorLocateCriteria;
        private CloudSpatialAnchorWatcher currentWatcher;

        private bool isInit;
        public WatchMode CurrentWatchMode { get; private set; }

        private void Start()
        {
            // Get a reference to the SpatialAnchorManager component (must be on the same gameobject)
            cloudManager = GetComponent<SpatialAnchorManager>();
            cloudManager.LogLevel = ASALogLevel;

            cloudManager.SessionUpdated += OnSessionUpdated;
            cloudManager.AnchorLocated += OnAchorLocated;
            cloudManager.LocateAnchorsCompleted += OnLocateCompleted;

            anchorLocateCriteria = new AnchorLocateCriteria();
            CurrentWatchMode = WatchMode.NA;
            isInit = true;

            Debug.Log("ASA: finished Start()");
        }

        /// <summary>
        /// Bypassing the cache will force new queries to be sent for objects, allowing
        /// for refined poses over time.
        /// </summary>
        /// <param name="BypassCache"></param>
        public override void SetBypassCache(bool BypassCache)
        { // not sure if useful, no usage example found
            anchorLocateCriteria.BypassCache = BypassCache;
        }


        private void OnSessionUpdated(object sender, SessionUpdatedEventArgs args)
        {
            //Debug.Log("ASA: OnSessionUpdated: " + args.ToString() + ", " + args.Status);
        }

        private void OnLocateCompleted(object sender, LocateAnchorsCompletedEventArgs args)
        {
            callbacks.OnLocateComplete.Invoke();
            Debug.Log("ASA: OnLocateCompleted: " + args.ToString() + ", watcher = " + args.Watcher.ToString());
        }


        #region Wrapper
        public async override void StartSession()
        {
            Debug.Log("AzureWrapper3: Starting Azure session... please wait...");
            // Creates a new session if one does not exist
            if (cloudManager.Session == null)
                await cloudManager.CreateSessionAsync();

            // Starts the session if not already started
            await cloudManager.StartSessionAsync();
            Debug.Log("Azure session started successfully");

            PlatformLocationProvider locationProvider = new PlatformLocationProvider();
            locationProvider.Sensors.GeoLocationEnabled = true;
            locationProvider.Sensors.WifiEnabled = true;
            locationProvider.Sensors.BluetoothEnabled = true;

            cloudManager.Session.LocationProvider = locationProvider;
            Debug.Log("Set Location Provider successfully");
        }

        public async override void ResetSession()
        {
            if (cloudManager.Session != null)
            {
                Debug.Log("Start Session reset");
                await cloudManager.ResetSessionAsync();
                Debug.Log("Session reset done");

            }
            else
            {
                Debug.Log("Session null");
            }
        }
        public async override void DeleteAnchor(CloudSpatialAnchor anchor)
        {
            try
            {
                Debug.Log("Azure: delete anchor: " + anchor.Identifier);
                if (cloudManager != null && cloudManager.Session != null)
                    await cloudManager.DeleteAnchorAsync(anchor);
            }
            catch (Exception e)
            {
                Debug.Log("Delete Anchor Exception: " + e.Message);
            }
            callbacks.OnAnchorDeleted.Invoke(anchor.Identifier);
        }

        public async override void CreateAnchor(Vector3 pos, Quaternion rot, GameObject prefab)
        {
            Debug.Log("Azure: create anchor...");

            var go = Instantiate(prefab, pos, rot);
            // attaching the cna resets the position of the gameobject to the origin
            // -> disable until anchor creation done
            GOUtils.EnableMeshRenderer(go, false);
            var cna = go.AddComponent<CloudNativeAnchor>();
            var csa = await SaveNativeToCloud(cna);

            //NOTE/TODO if saveNativeToCloud fails, csa = null
            // but the go will still be created, but the attempt to
            // get its anchor id will return id = ""

            // place object back to its correct position and enable it
            go.transform.position = pos;
            go.transform.rotation = rot;
            GOUtils.EnableMeshRenderer(go, true);
            //tmpAzure.AttachToGameObject(csa, theObject);
            Debug.LogFormat("Azure: notify anchor created -> csa.id = {0}, cna.id = {1} ", csa.Identifier, cna.CloudAnchor.Identifier);

            callbacks.OnAnchorCreated.Invoke(go);

            Debug.Log("Anchor created & saved to cloud -> csa: " + csa.Identifier);
        }

        private async Task<CloudSpatialAnchor> SaveNativeToCloud(CloudNativeAnchor cna)
        {
            // 99% from AzureBase.cs
            // line 437 ff
            //https://github.com/Azure/azure-spatial-anchors-samples/blob/79dc2ee96d027f9eeeab56767b5013d533bced63/Unity/Assets/AzureSpatialAnchors.Examples/Scripts/DemoScriptBase.cs#L287

            // If the cloud portion of the anchor hasn't been created yet, create it
            if (cna != null && cna.CloudAnchor == null)
            {
                await cna.NativeToCloud();
            }
            else
            {
                Debug.LogError("Azure: cna null!");
            }

            // Get the cloud portion of the anchor
            CloudSpatialAnchor cloudAnchor = cna.CloudAnchor;

            // In this sample app we delete the cloud anchor explicitly, but here we show how to set an anchor to expire automatically
            //cloudAnchor.Expiration = DateTimeOffset.Now.AddDays(7);

            while (!cloudManager.IsReadyForCreate)
            {
                await Task.Delay(330);
                float createProgress = cloudManager.SessionStatus.RecommendedForCreateProgress;
                Debug.Log($"Move your device to capture more environment data: {createProgress:0%}");
            }

            bool success = false;

            Debug.Log("Saving...");

            try
            {
                // Actually save
                await cloudManager.CreateAnchorAsync(cloudAnchor);

                // Success?
                success = cloudAnchor != null;

                if (success)
                {
                    Debug.Log("Saving...success -> " + cloudAnchor.Identifier);

                    //// Await override, which may perform additional tasks
                    //// such as storing the key in the AnchorExchanger
                    ////await OnSaveCloudAnchorSuccessfulAsync();

                    ////why ui thread???
                    ////UnityDispatcher.InvokeOnAppThread(() => callbacks.OnAnchorCreated.Invoke(anchoredGO));
                    //Dispatcher.ProcessOnUI(() =>
                    //{
                    //    Debug.Log("AzureWrapper3: Notifiy on anchor create: " + cloudAnchor.Identifier + ", anchor.pos = " + cloudAnchor.GetPose().position + ", go.pos = " + anchoredGO.transform.position);
                    //    anchoredGO.transform.position = cloudAnchor.GetPose().position;
                    //    anchoredGO.transform.rotation = cloudAnchor.GetPose().rotation;

                    //    callbacks.OnAnchorCreated.Invoke(anchoredGO);
                    //});

                }
                else
                {
                    Debug.Log("Failed to save, but no exception was thrown.");
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Exception: NO ANCHOR CREATED" + ex.Message);

            }
            return cloudAnchor;
        }
        public override void FindAnchor(ICollection<string> anchorIDs)
        {
            Debug.Log("AzureWrapper3: findAnchor() -> count =  " + anchorIDs.Count);

            // Start watching for Anchors
            if ((cloudManager != null) && (cloudManager.Session != null))
            {
                if (currentWatcher != null)
                    currentWatcher.Stop();

                CurrentWatchMode = WatchMode.IDENTIFIER;

                anchorLocateCriteria.NearAnchor = null;
                anchorLocateCriteria.NearDevice = null;

                anchorLocateCriteria.Strategy = LocateStrategy.AnyStrategy;
                anchorLocateCriteria.Identifiers = anchorIDs.ToArray();
                anchorLocateCriteria.BypassCache = true;

                currentWatcher = cloudManager.Session.CreateWatcher(anchorLocateCriteria);

                string info = "Watch for anchorIDs:\n";
                foreach (string id in anchorLocateCriteria.Identifiers)
                    info += id + "\n";
                Debug.Log(info);
            }
            else
            {
                Debug.Log("Attempt to create watcher failed, no session exists");
                currentWatcher = null;
            }
        }


        private ICollection<string> ensureMaxAnchorLimit(ICollection<string> stringList)
        {
            if (stringList.Count <= 35)
            {
                return stringList;
            }
            else
            {
                return new List<string>(stringList).GetRange(0, 34);
            }
        }

        //https://github.com/Azure/azure-spatial-anchors-samples/blob/79dc2ee96d027f9eeeab56767b5013d533bced63/Unity/Assets/AzureSpatialAnchors.Examples/Scripts/DemoScriptBase.cs#L225
        public override void GetNearAnchor(AnchorWrapper anchor, float distInMeter = float.MaxValue, int maxAnchor = int.MaxValue)
        {
            if ((cloudManager != null) && (cloudManager.Session != null))
            {
                if (currentWatcher != null)
                    currentWatcher.Stop();

                CurrentWatchMode = WatchMode.NEAR_ANCHOR;

                NearAnchorCriteria nac = new NearAnchorCriteria();
                nac.SourceAnchor = anchor.anchor;

                nac.DistanceInMeters = distInMeter;
                nac.MaxResultCount = maxAnchor;

                anchorLocateCriteria.NearDevice = null;
                anchorLocateCriteria.Identifiers = new string[0];
                // any is also possible, but will default to relationship anyway
                anchorLocateCriteria.Strategy = LocateStrategy.Relationship;
                anchorLocateCriteria.NearAnchor = nac;

                anchorLocateCriteria.BypassCache = true;

                currentWatcher = cloudManager.Session.CreateWatcher(anchorLocateCriteria);

                Debug.Log("Watch for near anchor to ID:" + anchor.identifier);
            }
            else
            {
                Debug.Log("Attempt to create watcher failed, no session exists");
                currentWatcher = null;
            }

        }
        public override void GetAnchorNearDevice(float distInMeter = 100, int maxAnchor = 30)
        {


            Debug.Log("GetNearDeviceAnchor");
            if (currentWatcher != null)
                currentWatcher.Stop();

            CurrentWatchMode = WatchMode.NEAR_DEVICE;

            bool watchNearAnchor = true;
            if (watchNearAnchor)
            {
                // Configure the near-device criteria
                NearDeviceCriteria nearDeviceCriteria = new NearDeviceCriteria();
                nearDeviceCriteria.DistanceInMeters = distInMeter;
                nearDeviceCriteria.MaxResultCount = maxAnchor;

                // Set the session's locate criteria
                AnchorLocateCriteria anchorLocateCriteria = new AnchorLocateCriteria();
                anchorLocateCriteria.NearDevice = nearDeviceCriteria;
                currentWatcher = cloudManager.Session.CreateWatcher(anchorLocateCriteria);
            }
            else
            {
                // does not work
                EnumerateAllNearbyAnchors(distInMeter, maxAnchor);
          
            }

        }
        public override void StopFindAnchor(ICollection<string> anchorIDs)
        {
            // Start watching for Anchors
            if ((cloudManager != null) && (cloudManager.Session != null))
            {
                if (currentWatcher != null)
                    currentWatcher.Stop();

                var curAnchorIDs = anchorLocateCriteria.Identifiers;
                HashSet<string> set = new HashSet<string>(curAnchorIDs);
                bool hasChanged = false;
                foreach (var id in anchorIDs)
                {
                    if (set.Remove(id))
                        hasChanged = true;
                }
                if (hasChanged)
                {
                    anchorLocateCriteria.Identifiers = set.ToArray<string>();
                    currentWatcher = cloudManager.Session.CreateWatcher(anchorLocateCriteria);
                    string info = "Watch for anchorIDs:\n";
                    foreach (string id in anchorLocateCriteria.Identifiers)
                        info += id + "\n";
                    Debug.Log(info);
                }
                else
                {
                    // happens if the watcher removes the id on anchor located
                    Debug.Log("AzureWrapper: StopFindAnchor -> no changes");
                }
            }
            else
            {
                Debug.Log("Attempt to create watcher failed, no session exists");
                currentWatcher = null;
            }
        }

        public async override void StopSession()
        {
            Debug.Log("Stopping Azure session... please wait...");
            // Stops any existing session
            cloudManager.StopSession();
            // Resets the current session if there is one, and waits for any active queries to be stopped
            await cloudManager.ResetSessionAsync();
            Debug.Log("Azure session stopped successfully");
        }
        #endregion

        #region private
        private void OnAchorLocated(object sender, AnchorLocatedEventArgs args)
        {
            if (args.Status == LocateAnchorStatus.Located)
            {
                Debug.Log("Invoke OnAnchorFound: LOCATED -> " + args.Anchor.Identifier);
                callbacks.OnAnchorLocated.Invoke(new AnchorWrapper(args, CurrentWatchMode));
            }
            else if (args.Status == LocateAnchorStatus.AlreadyTracked)
            {
                Debug.Log("Invoke OnAnchorFound: ALREADY TRACKED -> " + args.Anchor.Identifier);
                callbacks.OnAnchorLocated.Invoke(new AnchorWrapper(args, CurrentWatchMode));
            }
            else
            {
                Debug.Log($"ANCHOR DISCARD -> '{args.Identifier}'\n" +
                    $"Attempt to locate Anchor failed , locate anchor status was not 'Located' but '{args.Status}'");
            }
           
        }

        #endregion
        #region AzureDemo githung
        // does not work
        private async void EnumerateAllNearbyAnchors(float distInMeter, int maxAnchor)
        {
            try
            {
                await EnumerateAllNearbyAnchorsAsync(distInMeter, maxAnchor);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in {nameof(EnumerateAllNearbyAnchors)}: === {ex.GetType().Name} === {ex.ToString()} === {ex.Source} === {ex.Message} {ex.StackTrace}");
            }
        }
        private async Task EnumerateAllNearbyAnchorsAsync(float distInMeter, int maxAnchor)
        {
            Debug.Log("Enumerating near-device spatial anchors in the cloud");

            NearDeviceCriteria criteria = new NearDeviceCriteria();
            criteria.DistanceInMeters = distInMeter;
            criteria.MaxResultCount = maxAnchor;

            var cloudAnchorSession = cloudManager.Session;
            Debug.LogFormat("Try find near anchor ids");

            var spatialAnchorIds = await cloudAnchorSession.GetNearbyAnchorIdsAsync(criteria);

            Debug.LogFormat("Got ids for {0} anchors", spatialAnchorIds.Count);

            List<CloudSpatialAnchor> spatialAnchors = new List<CloudSpatialAnchor>();

            foreach (string anchorId in spatialAnchorIds)
            {
                var anchor = await cloudAnchorSession.GetAnchorPropertiesAsync(anchorId);
                Debug.LogFormat("Received information about spatial anchor {0}", anchor.Identifier);
                spatialAnchors.Add(anchor);
            }

            Debug.Log($"Found {spatialAnchors.Count} anchors nearby");
            //UnityDispatcher.InvokeOnAppThread(() => callbacks.OnNearAnchor.Invoke(spatialAnchorIds));
        }

        #endregion
    }
}
