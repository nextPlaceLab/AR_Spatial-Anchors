

using LLE.ASA;
using LLE.Rx;
using LLE.Unity;
using Microsoft.Azure.SpatialAnchors.Unity;
using System;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;


namespace LLE.ASA
{


    public class LocatedAnchorHandler
    {
        private static string KEY_SHOW_MARKER = "isShowMarker";
        public UnityEvent<AnchorMarker> OnAnchorCreatedEvent { get; private set; } = new UnityEvent<AnchorMarker>();
        public UnityEvent<AnchorMarker> OnAnchorLocatedEvent { get; private set; } = new UnityEvent<AnchorMarker>();
        public UnityEvent<AnchorMarker> OnAnchorSelectedEvent { get; private set; } = new UnityEvent<AnchorMarker>();
        public UnityEvent<string> OnAnchorRemovedEvent { get; private set; } = new UnityEvent<string>();

        public UnityEvent<string, bool> AnchorIDsUpdateEvent { get; private set; } = new UnityEvent<string, bool>();
        public AnchorMarker SelectedAnchor { get; private set; }
        public Dictionary<string, AnchorMarker> anchorGoById { get; private set; } = new Dictionary<string, AnchorMarker>();


        private HashSet<string> locatedAnchorIDs = new HashSet<string>();
        private GameObject anchorContainer;
        private GameObject anchorPrefab;
        private Color colorDefault;
        private Color colorSelected;
        private bool isShowAnchorMarker = true;

        public bool IsShowAnchorMarker
        {
            get
            {
                return isShowAnchorMarker;
            }
            private set
            {
                if (isShowAnchorMarker != value)
                    Dispatcher.ProcessOnUI(() =>
                    {
                        Debug.Log("LocatedAnchor: set isShowMarker = " + value);
                        PlayerPrefs.SetInt(KEY_SHOW_MARKER, value ? 1 : 0);
                        PlayerPrefs.Save();
                    });

                isShowAnchorMarker = value;
            }
        }



        public LocatedAnchorHandler(GameObject container, GameObject anchorMarkerPrefab, Color defaultAnchor, Color selectedAnchor)
        {

            Dispatcher.ProcessOnUI(() =>
            {
                isShowAnchorMarker = PlayerPrefs.GetInt(KEY_SHOW_MARKER, 1) == 1 ? true : false;
            });
            Debug.Log("LocatedAnchor: isShowMarker = " + isShowAnchorMarker);

            anchorContainer = container;
            anchorPrefab = anchorMarkerPrefab;
            colorDefault = defaultAnchor;
            colorSelected = selectedAnchor;
        }

        public void RemoveAllAnchor()
        {
            var located = anchorGoById.Keys.ToArray<String>();
            RemoveAnchor(located);
            SelectedAnchor = null;
        }
        public void RemoveAnchor(ICollection<string> anchorIDs)
        {
            foreach (var anchor in anchorIDs)
            {
                RemoveAnchor(anchor);
            }
        }

        public bool RemoveAnchor(string anchorId)
        {
            if (locatedAnchorIDs.Contains(anchorId))
            {
                Debug.Log("LocatedAnchor: Remove anchor -> " + anchorId);
                locatedAnchorIDs.Remove(anchorId);

                ResetSelectedAnchor();

                var marker = anchorGoById[anchorId];

                anchorGoById.Remove(anchorId);
                Dispatcher.ProcessOnUI(() =>
                {
                    UnityEngine.Object.Destroy(marker.gameObject);
                });

                AnchorIDsUpdateEvent.Invoke(anchorId, false);
                OnAnchorRemovedEvent.Invoke(anchorId);
                return true;
            }
            else
            {
                Debug.Log("LocatedAnchor: Remove anchor -> not found: " + anchorId);
            }
            return false;
        }

        public AnchorMarker GetAnchor(string anchorId)
        {
            if (anchorGoById.ContainsKey(anchorId))
                return anchorGoById[anchorId];
            else
                return null;
        }

        public void OnNewCloudAnchorCreated(AnchorWrapper anchor, GameObject anchoredGO)
        {
            Debug.Log("LocatedAnchor: Add create anchor");
            AnchorMarker ag;
            if (locatedAnchorIDs.Contains(anchor.identifier))
            {
                Debug.LogErrorFormat("Located Anchor: New anchor already listed ->" + anchor.identifier);
                ag = anchorGoById[anchor.identifier];
            }
            else
            {
                Debug.Log("LocatedAnchorManager: new anchor created -> " + anchor.identifier);
                locatedAnchorIDs.Add(anchor.identifier);
                ag = BundleAnchor(anchor, anchoredGO);
                AnchorIDsUpdateEvent.Invoke(anchor.identifier, true);
                OnAnchorCreatedEvent.Invoke(ag);
            }

            SetVisibility(anchoredGO, IsShowAnchorMarker);

            OnAnchorLocatedEvent.Invoke(ag);
            OnAnchorSelected(ag);
        }

        private AnchorMarker BundleAnchor(AnchorWrapper anchor, GameObject anchorGO)
        {
            Debug.Log("Located Anchor: Create new anchor marker");
            anchorGO.name = anchor.identifier;
            var infoTxt = anchorGO.GetComponentInChildren<TextMeshPro>();
            if (infoTxt != null)
                infoTxt.text = anchor.identifier;

            anchorGO.GetComponent<MeshRenderer>().material.color = colorDefault;

            var ag = new AnchorMarker(anchor, anchorGO);
            ag.gameObject.transform.SetParent(anchorContainer.transform, true);

            ag.OnClick.AddListener(OnAnchorSelected);
            anchorGoById.Add(anchor.identifier, ag);
            return ag;
        }
        public void SelectAnchor(AnchorMarker anchor)
        {
            OnAnchorSelected(anchor);
        }
        private void OnAnchorSelected(AnchorMarker anchor)
        {
            Debug.Log("LocatedAnchor: onAnchorSelected -> " + anchor.anchor.identifier);
            ResetSelectedAnchor();
            SetSelectedAnchor(anchor);
            OnAnchorSelectedEvent.Invoke(anchor);
        }
        public void ResetSelectedAnchor()
        {
            if (SelectedAnchor != null)
                SetColor(SelectedAnchor.gameObject, colorDefault);
            SelectedAnchor = null;
        }
        private void SetSelectedAnchor(AnchorMarker anchor)
        {
            SelectedAnchor = anchor;
            SetColor(anchor.gameObject, colorSelected);
        }

        private void SetColor(GameObject gameObject, Color color)
        {
            Dispatcher.ProcessOnUI(() =>
            {
                gameObject.GetComponent<MeshRenderer>().material.color = color;
            });
        }
        public void OnAnchorLocated(AnchorWrapper anchor)
        {

            if (!locatedAnchorIDs.Contains(anchor.identifier))
            {
                Debug.LogFormat("Located Anchor: new anchor {0} -> tid = {1}", anchor.identifier, Thread.CurrentThread.ManagedThreadId);

                locatedAnchorIDs.Add(anchor.identifier);

                Dispatcher.ProcessOnUI(() =>
                {
                    var go = GameObject.Instantiate(anchorPrefab);

                    // for debugging purposes only
                    go.transform.position = anchor.GetPose().position;
                    go.transform.rotation = anchor.GetPose().rotation;

                    var cna = go.GetComponent<CloudNativeAnchor>();
                    if (cna == null)
                        cna = go.AddComponent<CloudNativeAnchor>();
                    // Apply the cloud anchor, which also sets the pose. -> does not work
                    if (anchor.anchor != null)
                        cna.CloudToNative(anchor.anchor);

                    go.transform.position = anchor.GetPose().position;
                    go.transform.rotation = anchor.GetPose().rotation;

                    Debug.Log("Tracked Anchor Mng: create marker -> anchor: " + anchor.identifier + ", pos = " + go.transform.position);
                    var ag = BundleAnchor(anchor, go);
                    SetVisibility(go, isShowAnchorMarker);
                    OnAnchorLocatedEvent.Invoke(ag);
                });
            }
            else
            {
                Debug.Log("Located Anchor: Update " + anchor.identifier);
            }
        }

        public void ShowMarker(bool show)
        {
            Dispatcher.ProcessOnUI(() =>
            {
                Debug.Log("ShowMarker: " + show + ", count = " + anchorGoById.Count);
                IsShowAnchorMarker = show;
                if (anchorGoById != null)
                {
                    foreach (var kv in anchorGoById)
                    {
                        if (kv.Value != null)
                            SetVisibility(kv.Value.gameObject, IsShowAnchorMarker);
                        else
                            Debug.Log("GO is null! anchor = " + kv.Key == null ? "null key" : kv.Key);
                    }
                }
                else
                {
                    Debug.Log("anchorMarker null");
                }
            });
        }

        private void SetVisibility(GameObject gameObject, bool isShowAnchorMarker)
        {
            var tv = gameObject.GetComponent<ToggleVisibility>();
            if (tv != null)
                tv.SetVisibilty(isShowAnchorMarker);
        }
    }
}