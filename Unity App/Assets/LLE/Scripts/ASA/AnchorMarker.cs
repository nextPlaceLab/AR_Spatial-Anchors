using LLE.Unity;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace LLE.ASA
{
    public class AnchorMarker
    {
        // rename to ARParent, ARAnchor (conflicts with unity) ?
        public GameObject gameObject { get; private set; }
        public AnchorWrapper anchor { get; private set; }
        public UnityEvent<AnchorMarker> OnClick { get; private set; }
        public AnchorMarker(AnchorWrapper anchor, GameObject gameObject)
        {
            this.anchor = anchor;
            this.gameObject = gameObject;

            OnClick = new UnityEvent<AnchorMarker>();

            var interactable = gameObject.GetComponent<Interactable>();
            if (interactable == null)
                interactable = gameObject.AddComponent<Interactable>();
            interactable.OnClick.AddListener(() => OnClick.Invoke(this));
        }
    }
}
