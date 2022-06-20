

using Microsoft.Azure.SpatialAnchors;

using System.Collections.Generic;

using UnityEngine;


namespace LLE.ASA
{

    // Notes
    // https://docs.microsoft.com/en-us/azure/spatial-anchors/how-tos/create-locate-anchors-unity
    // You can't update the location of an anchor once it has been created on the service -
    // you must create a new anchor and delete the old one to track a new position.
    public abstract class AbstractAzureWrapper : MonoBehaviour
    {

        public AzureEvents callbacks = new AzureEvents();
        public abstract void CreateAnchor(Vector3 pos, Quaternion rot,GameObject prefab);
        public abstract void DeleteAnchor(CloudSpatialAnchor cloudSpatialAnchor);

        public abstract void FindAnchor(ICollection<string> anchorIDs);
        public abstract void GetNearAnchor(AnchorWrapper anchor, float distInMeter = float.MaxValue, int maxAnchor = int.MaxValue);
        public abstract void GetAnchorNearDevice(float distInMeter = float.MaxValue, int maxAnchor = int.MaxValue);
        public abstract void StartSession();
        public abstract void ResetSession();
        public abstract void StopSession();

        public abstract void StopFindAnchor(ICollection<string> anchorIDs);
        public abstract void SetBypassCache(bool BypassCache);
        private void Update()
        {

        }
    }
}
