using Assets.LLE.Scripts.ASA.Anchor.Behavior;
using LLE.ASA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace LLE.Unity
{
    class ClosestAnchorBehavior : MonoBehaviour
    {
        [SerializeField]
        private Transform refTransform;
        [SerializeField]
        private AnchorMaster anchorMaster;
        public UnityEvent<AnchorMarker> OnClosestAnchorChanged = new UnityEvent<AnchorMarker>();

        private AnchorMarker ClosestAnchor = null;

        private void Update()
        {
            var current = GetClosestAnchor(refTransform.position);
            if (current != null && !current.Equals(ClosestAnchor))
                UpdateClosestAnchor(current);
        }

        private AnchorMarker GetClosestAnchor(Vector3 pos)
        {
            var anchors = anchorMaster.locatedAnchor.anchorGoById.Values;
            var minDist = float.MaxValue;
            AnchorMarker closestAnchor = null;
            foreach (var item in anchors)
            {
                var dist = Vector3.Distance(pos, item.anchor.GetPose().position);
                //Debug.LogFormat("Anchor: {0}, pos = {1}, dist = {2}", item.anchor.identifier, item.anchor.GetPose().position, dist);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestAnchor = item;
                }
            }
            return closestAnchor;
        }
        private void UpdateClosestAnchor(AnchorMarker anchor)
        {
            if (anchor == null)
                return;

            Debug.Log("ClosestAnchor changed to: " + anchor.anchor.identifier);
            ClosestAnchor = anchor;
            OnClosestAnchorChanged.Invoke(ClosestAnchor);
        }
        public AnchorMarker GetClosestAnchor()
        {
            return ClosestAnchor;           
        }
    }
}
