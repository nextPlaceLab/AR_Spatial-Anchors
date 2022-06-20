using LLE.Network;
using LLE.Rx;
using LLE.Unity;
using Microsoft.Azure.SpatialAnchors.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace LLE.ASA
{
    class AnchorMaster : MonoBehaviour
    {

        [SerializeField]
        private AzureSessionManager azureSession;

        [SerializeField]
        private GameObject anchorContainer;

        [SerializeField]
        private GameObject anchorPrefab;

        [SerializeField]
        private Color colorNormal;

        [SerializeField]
        private Color colorSelected;

        [SerializeField]
        private bool useLocalKnownAnchorStore;

        [SerializeField]
        private UIStringList uiKnownAnchorList;
        [SerializeField]
        private UIStringList uiWatcherList;
        [SerializeField]
        private UIStringList uiLocatedAnchorList;


        public KnownAnchorHandler knownAnchor { get; private set; }
        public WatchedAnchorHandler watcher { get; private set; }
        public LocatedAnchorHandler locatedAnchor { get; private set; }

        // Note: make sure that all event listener get notified both editor and runtime
        // otherwise unity will check if it is runtime which will raise an error if not called from ui thread
        public UnityEvent<AnchorMarker> OnAnchorCreated = new UnityEvent<AnchorMarker>();
        public UnityEvent<AnchorMarker> OnAnchorLocated = new UnityEvent<AnchorMarker>();
        public UnityEvent<AnchorMarker> OnAnchorSelected = new UnityEvent<AnchorMarker>();
        public UnityEvent<string> OnAnchorRemoved = new UnityEvent<string>();
        public UnityEvent<string> OnAnchorDeleted = new UnityEvent<string>();
        public AnchorMarker selectedAnchor { get; private set; }

        private void Awake()
        {
            Debug.Log("Start Anchor Master");
            if (azureSession == null)
                Debug.LogError("AzureSessionManager == null");
            if (anchorContainer == null)
                Debug.LogError("AnchorContainer == null");
            if (anchorPrefab == null)
                Debug.LogError("AnchorPrefab == null");

            if (anchorPrefab.GetComponent<ToggleVisibility>() == null)
                Debug.LogError("AnchorMaster: anchor prefab requires a configured ToggleVisibilty component!");

            knownAnchor = new KnownAnchorHandler(this, useLocalKnownAnchorStore);

            watcher = new WatchedAnchorHandler(azureSession);
            locatedAnchor = new LocatedAnchorHandler(anchorContainer, anchorPrefab, colorNormal, colorSelected);

            locatedAnchor.OnAnchorCreatedEvent.AddListener((a) => OnAnchorCreated.Invoke(a));
            locatedAnchor.OnAnchorLocatedEvent.AddListener((a) => OnAnchorLocated.Invoke(a));
            locatedAnchor.OnAnchorRemovedEvent.AddListener((a) => OnAnchorRemoved.Invoke(a));
            locatedAnchor.OnAnchorSelectedEvent.AddListener((a) =>
            {
                selectedAnchor = a;
                OnAnchorSelected.Invoke(a);
            });

            azureSession.OnSessionReady.AddListener(OnAzureReady);
            azureSession.OnAnchorCreated.AddListener(AddCreatedAnchor);
            azureSession.OnAnchorLocated.AddListener(AddLocatedAnchor);
            azureSession.OnAnchorDeleted.AddListener(DeleteAnchor);

            azureSession.OnLocateComplete.AddListener(WatchAll);

            if (uiKnownAnchorList != null)
            {
                knownAnchor.AnchorIDsUpdate.AddListener(uiKnownAnchorList.Change);
            }
            else
            {
                Debug.Log("KnownAnchor: GUI not set");
            }

            if (uiWatcherList != null)
            {
                watcher.AnchorIDsUpdate.AddListener(uiWatcherList.Change);
            }
            else
            {
                Debug.Log("WatchedAnchor: GUI not set");
            }

            if (uiLocatedAnchorList != null)
            {
                locatedAnchor.AnchorIDsUpdateEvent.AddListener(uiLocatedAnchorList.Change);
            }
            else
            {
                Debug.Log("LocatedAnchor: GUI not set");
            }
            Debug.Log("Start Anchor Master...done");
        }


        internal void RemoveLocatedAnchor(string anchorId)
        {
            locatedAnchor.RemoveAnchor(anchorId);
            Debug.Log("Anchor Master: remove anchor -> " + anchorId);
            OnAnchorRemoved.Invoke(anchorId);
            //Dispatcher.Process(() => OnAnchorRemoved.Invoke(anchorId));
        }

        private void OnAzureReady(bool arg0)
        {
            Debug.Log("On Azure ready");
        }
        public void SelectAnchor(AnchorMarker anchor)
        {
            locatedAnchor.SelectAnchor(anchor);
        }

        public bool AddAnchorIDs(ICollection<string> anchorIds)
        {
            return knownAnchor.Add(anchorIds);
        }

        public void AddCreatedAnchor(GameObject anchorGO)
        {
            Debug.Log("AnchorMaster: OnAddCreatedAnchor");

            AnchorWrapper anchor;
            var cna = anchorGO.GetComponent<CloudNativeAnchor>();
            if (cna != null && cna.CloudAnchor != null)
            {
                Debug.Log("OnAnchorCreated: new CloudNativeAnchor -> id = " + cna.CloudAnchor.Identifier);
                anchor = new AnchorWrapper(cna.CloudAnchor);
            }
            else
            {
                Debug.Log("OnAnchorCreated: CloudNativeAnchor not found -> CREATE DUMMY");
                anchor = new AnchorWrapper(anchorGO.name + "_" + DateTime.Now.ToString(), anchorGO.transform);
            }

            Debug.Log("OnAnchorCreated: add created anchor -> id = " + anchor.identifier);
            knownAnchor.Add(anchor.identifier);
            locatedAnchor.OnNewCloudAnchorCreated(anchor, anchorGO);

            Debug.Log("AnchorMaster: OnAddCreatedAnchor...done");
        }
        public void DeleteAnchor(ICollection<string> anchorIDs)
        {
            foreach (var item in anchorIDs)
            {
                DeleteAnchor(item);
            }
        }
        public void DeleteAnchor(string anchorId)
        {
            Debug.Log("AnchorMaster: delete anchor -> " + anchorId);
            knownAnchor.Remove(anchorId);
            locatedAnchor.RemoveAnchor(anchorId);
            watcher.RemoveWatcher(anchorId);
            Dispatcher.Process(() => OnAnchorDeleted.Invoke(anchorId));
        }
        public void AddLocatedAnchor(AnchorWrapper anchor)
        {
            watcher.RemoveFromList(anchor.identifier);
            locatedAnchor.OnAnchorLocated(anchor);
        }

        public void WatchAll()
        {
            Debug.Log("KnownAnchorManager: WatchAllKnown -> count = " + knownAnchor.knownAnchorIDs.Count);
            List<string> unlocated = GetUnlocatedAnchorIDs();
            if (unlocated.Count > 0)
            {
                Debug.Log("AnchorMaster: Watch all unlocated -> count = " + unlocated.Count);
                watcher.AddWatcher(unlocated);
            }
            else
            {
                Debug.Log("AnchorMaster: Watch all unlocated -> all anchor located!");
            }
        }

        private List<string> GetUnlocatedAnchorIDs()
        {
            var known = new HashSet<string>(knownAnchor.knownAnchorIDs);
            foreach (var item in locatedAnchor.anchorGoById.Keys)
                known.Remove(item);

            var unlocated = known.ToList();
            while (unlocated.Count > 35)
                unlocated.RemoveAt(unlocated.Count - 1);

            return unlocated;
        }

        public void ResetAllAnchor()
        {
            knownAnchor.RemoveAll();
            locatedAnchor.RemoveAllAnchor();
            watcher.ResetAllWatcher();
        }

    }
}
