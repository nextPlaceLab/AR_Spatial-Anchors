using LLE.ASA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.LLE.Scripts.ASA.Anchor.Behavior
{
    class AnchorController : MonoBehaviour
    {
        [SerializeField]
        private AnchorMaster AnchorMaster;

        private string knownAnchorSelection = "";
        private string watchedAnchorSelection = "";
        private string locatedAnchorSelection = "";

        private void Start()
        {

        }
        public void ToggleAnchorMarker()
        {
            AnchorMaster.locatedAnchor.ShowMarker(!AnchorMaster.locatedAnchor.IsShowAnchorMarker);
        }
        public void OnKnownAnchorSelected(string anchor)
        {
            knownAnchorSelection = anchor;
        }
        public void OnWatchedAnchorSelected(string anchor)
        {
            watchedAnchorSelection = anchor;
        }
        public void OnLocatedAnchorSelected(AnchorMarker anchor)
        {
            OnLocatedAnchorSelected(anchor.anchor.identifier);
        }
        public void OnLocatedAnchorSelected(string anchor)
        {
            locatedAnchorSelection = anchor;
        }


        public void WatchAllAnchors()
        {
            Debug.Log("UI: Watch all");
            AnchorMaster.watcher.ResetAllWatcher();
            AnchorMaster.watcher.AddWatcher(AnchorMaster.knownAnchor.knownAnchorIDs);
            
        }
        public void WatchNearDevice()
        {
            Debug.Log("UI: Watch near device");
            AnchorMaster.watcher.WatchNearDevice();
        }
        public void RemoveSelectedWatcher()
        {
            if (watchedAnchorSelection != "")
            {
                Debug.Log("UI: Remove Watcher -> " + watchedAnchorSelection);
                AnchorMaster.watcher.RemoveWatcher(watchedAnchorSelection);
                watchedAnchorSelection = "";
            }
            else
            {
                Debug.Log("UI: Remove Watcher -> No anchor selected");
            }
        }
        public void WatchNearAnchor()
        {
            if (locatedAnchorSelection != "")
            {
                string anchorId = locatedAnchorSelection;
                Debug.Log("UI: Watch near anchor -> " + anchorId);
                var ago = AnchorMaster.locatedAnchor.GetAnchor(anchorId);
                if (ago != null)
                    AnchorMaster.watcher.WatchNearAnchor(ago.anchor);
                else
                    Debug.Log("UI: Watch near anchor: located anchor not found -> " + anchorId);
            }
            else
            {
                Debug.Log("UI: Watch near anchor -> No located anchor selected");
            }
        }
        public void RemoveAllLocatedAnchor()
        {
            Debug.Log("Anchor Controller: remove all located anchor");
            foreach (var item in AnchorMaster.locatedAnchor.anchorGoById.Keys.ToArray())
            {
                AnchorMaster.RemoveLocatedAnchor(item);
            }
        }
        public void RemoveLocatedAnchor()
        {

            if (locatedAnchorSelection != "")
            {
                Debug.Log("UI: Remove anchor -> " + locatedAnchorSelection);
                AnchorMaster.RemoveLocatedAnchor(locatedAnchorSelection);
                locatedAnchorSelection = "";
            }
            else
            {
                Debug.Log("UI: Remove anchor -> No located anchor selected");
            }
        }
        public void DeleteAnchor()
        {
            if (AnchorMaster.locatedAnchor.SelectedAnchor != null)
            {
                var csa = AnchorMaster.locatedAnchor.SelectedAnchor.anchor.anchor;
                if (csa != null)
                {
                    Debug.Log("UI: delete anchor -> " + locatedAnchorSelection);
                    AzureSessionManager.currentAzureManager.DeleteAnchor(csa);
                }
                else
                {
                    Debug.Log("UI: delete anchor -> error: spatial anchor not found" + locatedAnchorSelection);
                }
            }
            else
            {
                Debug.Log("UI: Remove anchor -> No located anchor selected");
            }
        }
        
        public void SetNearParameter(int maxAnchor, int maxDist)
        {
            AzureSessionManager.currentAzureManager.NearAnchorMaxCount = maxAnchor;
            AzureSessionManager.currentAzureManager.NearAnchorDistance = maxDist;
        }
        public void NotifyAllLocatedAnchor()
        {
            foreach (var anchor in AnchorMaster.locatedAnchor.anchorGoById.Values)
                AnchorMaster.OnAnchorLocated.Invoke(anchor);
        }
    }
}
