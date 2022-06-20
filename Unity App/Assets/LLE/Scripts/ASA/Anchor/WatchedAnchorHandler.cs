

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LLE.ASA
{
    class WatchedAnchorHandler
    {
        public UnityEvent<string, bool> AnchorIDsUpdate { get; private set; } = new UnityEvent<string, bool>();
        public HashSet<string> watchedAnchorIDs { get; private set; } = new HashSet<string>();
        private AzureSessionManager azureSessionManager;

        public WatchedAnchorHandler(AzureSessionManager azureSession)
        {
            azureSessionManager = azureSession;
        }

        public void AddWatcher(ICollection<string> anchorIDs)
        {
            foreach (var id in anchorIDs)
            {
                watchedAnchorIDs.Add(id);
                AnchorIDsUpdate.Invoke(id, true);
            }
            Debug.Log("WatcherManager: add watcher -> count = " + (anchorIDs == null ? "null" : "" + anchorIDs.Count));
            azureSessionManager.FindAnchor(watchedAnchorIDs);
        }



        public void RemoveWatcher(string anchorID)
        {
            if (watchedAnchorIDs.Contains(anchorID))
            {
                Debug.Log("WatcherManager: Remove Watcher -> " + anchorID);
                RemoveFromList(anchorID);
                azureSessionManager.StopFindAnchor(anchorID);
            }
            else
            {
                Debug.Log("WatcherManager: Remove Watcher -> not found: " + anchorID);
            }
        }


        public void RemoveFromList(string anchorId)
        {
            if (watchedAnchorIDs.Contains(anchorId))
            {
                Debug.Log("WatcherManager: Remove from list -> " + anchorId);
                AnchorIDsUpdate.Invoke(anchorId, false);
                watchedAnchorIDs.Remove(anchorId);
            }
            else
            {
                Debug.Log("WatcherManager: Remove from list -> not found: " + anchorId);
            }
        }

        public void ResetAllWatcher()
        {
            string[] watchedAnchor = watchedAnchorIDs.ToArray();
            foreach (var anchorId in watchedAnchor)
            {
                RemoveWatcher(anchorId);
            }
        }

        public void WatchNearAnchor(AnchorWrapper anchor)
        {
            ResetAllWatcher();
            AzureSessionManager.currentAzureManager.GetNearAnchor(anchor);
        }

        public void WatchNearDevice()
        {
            ResetAllWatcher();
            AzureSessionManager.currentAzureManager.GetAnchorNearDevice();
        }
    }
}