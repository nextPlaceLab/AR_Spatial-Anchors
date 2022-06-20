using LLE.Rx;
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
    public class AnchorWrapper
    {
        public string identifier { get; private set; }
        public CloudSpatialAnchor anchor { get; private set; }
        public WatchMode watchMode { get; private set; }
        public AnchorLocatedEventArgs anchorEventArgs { get; private set; }

        private Pose pose;
        public AnchorWrapper(AnchorLocatedEventArgs anchorEventArgs, WatchMode watchMode)
        {
            this.anchor = anchorEventArgs.Anchor;
            this.watchMode = watchMode;
            this.anchorEventArgs = anchorEventArgs;
            identifier = anchor.Identifier;
            pose = anchor.GetPose();
        }

        public AnchorWrapper(CloudSpatialAnchor anchor)
        {
            this.anchor = anchor;
            this.watchMode = WatchMode.NA;
            this.anchorEventArgs = null;
            identifier = anchor.Identifier;
            pose = anchor.GetPose();
        }
        public AnchorWrapper(string id, Transform t) : this(id, t.position, t.rotation) { }
        public AnchorWrapper(string id, Vector3 position, Quaternion rotation)
        {
            Debug.LogFormat("New AnchorWrapper: {0}, {1}, {2}", id, position, rotation);
            identifier = id;
            pose = new Pose(position, rotation);
            this.watchMode = WatchMode.NA;

        }
        
        public Pose GetPose()
        {
            return pose;
        }

        
    }
}
