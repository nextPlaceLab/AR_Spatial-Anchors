using System;
using UnityEngine;
using UnityEngine.Events;

namespace LLE.ASA
{
    [Serializable]
    public class AzureEvents
    {
        public UnityEvent<GameObject> OnAnchorCreated = new UnityEvent<GameObject>();
        public UnityEvent<AnchorWrapper> OnAnchorLocated = new UnityEvent<AnchorWrapper>();
        public UnityEvent<string> OnAnchorDeleted = new UnityEvent<string>();
        public UnityEvent OnLocateComplete = new UnityEvent();

    }
}
