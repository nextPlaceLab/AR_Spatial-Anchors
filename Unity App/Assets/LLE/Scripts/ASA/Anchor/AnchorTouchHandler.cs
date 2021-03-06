
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#if WINDOWS_UWP || UNITY_WSA
using UnityEngine.XR.WindowsMR;
#endif

#if UNITY_ANDROID || UNITY_IOS
using UnityEngine.XR.ARFoundation;
#endif

namespace LLE.ASA
{
    class AnchorTouchHandler : MonoBehaviour
    {


        public GameObject marker;
#if UNITY_ANDROID || UNITY_IOS
        ARRaycastManager arRaycastManager;
#endif
        /// <summary>
        /// Start is called on the frame when a script is enabled just before any
        /// of the Update methods are called the first time.
        /// </summary>
        public void Start()
        {


#if UNITY_ANDROID || UNITY_IOS
            arRaycastManager = FindObjectOfType<ARRaycastManager>();
            if (arRaycastManager == null)
            {
                Debug.Log("Missing ARRaycastManager in scene");
            }
#endif
#if WINDOWS_UWP || UNITY_WSA

            WindowsMRGestures mrGestures = FindObjectOfType<WindowsMRGestures>();
            if (mrGestures != null)
            {
                mrGestures.onTappedChanged += MrGesturesOnTappedChanged;
            }
            else
            {
                //throw new InvalidOperationException("WindowsMRGestures not found");
                Debug.LogError("WindowsMRGestures not found");
            }
#endif
        }
        /// <summary>
        /// Destroying the attached Behaviour will result in the game or Scene
        /// receiving OnDestroy.
        /// </summary>
        /// <remarks>
        /// OnDestroy will only be called on game objects that have previously been active.
        /// </remarks>
        public virtual void OnDestroy()
        {
#if WINDOWS_UWP || UNITY_WSA
            WindowsMRGestures mrGestures = FindObjectOfType<WindowsMRGestures>();
            if (mrGestures != null)
            {
                mrGestures.onTappedChanged -= MrGesturesOnTappedChanged;
            }
#endif

        }
#if WINDOWS_UWP || UNITY_WSA
        /// <summary>
        /// Called when a tap interaction occurs.
        /// </summary>
        /// <remarks>Currently only called for HoloLens.</remarks>
        private void MrGesturesOnTappedChanged(WindowsMRTappedGestureEvent obj)
        {
            Debug.Log("MrGesturesOnTappedChanged -> OnSelectInteraction");
            OnSelectInteraction();
        }
#endif
        /// <summary>
        /// Called when a select interaction occurs.
        /// </summary>
        /// <remarks>Currently only called for HoloLens.</remarks>
        protected virtual void OnSelectInteraction()
        {
            Debug.Log("OnSelectInteraction");
#if WINDOWS_UWP || UNITY_WSA
            RaycastHit hit;
            if (TryGazeHitTest(out hit))
            {
                OnSelectObjectInteraction(hit.point, hit);
            }
#endif
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        public virtual void Update()
        {
            TriggerInteractions();
        }

        private void TriggerInteractions()
        {

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    return;
                }

                OnTouchInteraction(touch);
            }
        }

        protected void OnTouchInteraction(Touch touch)
        {

            if (touch.phase == TouchPhase.Ended)
            {
                OnTouchInteractionEnded(touch);
            }
        }

        /// <summary>
        /// Called when a touch interaction has ended.
        /// </summary>
        /// <param name="touch">The touch.</param>
        protected void OnTouchInteractionEnded(Touch touch)
        {
            //Debug.Log("OnTouchInteractionEnded");
#if UNITY_ANDROID || UNITY_IOS
            List<ARRaycastHit> aRRaycastHits = new List<ARRaycastHit>();
            if (arRaycastManager.Raycast(touch.position, aRRaycastHits) && aRRaycastHits.Count > 0)
            {
                ARRaycastHit hit = aRRaycastHits[0];
                // org in azure code -> seem to set the object at orgion of the ray
                //OnSelectObjectInteraction(hit.pose.position, hit);
                OnSelectObjectInteraction(hit.sessionRelativePose.position, hit);
            }
#elif WINDOWS_UWP || UNITY_WSA
            RaycastHit hit;
            if (TryGazeHitTest(out hit))
            {
                OnSelectObjectInteraction(hit.point, hit);
            }
#endif
        }

        private bool TryGazeHitTest(out RaycastHit target)
        {
            Camera mainCamera = Camera.main;

            // Only detect collisions on the spatial mapping layer. Prevents cube placement issues
            // related to collisions with the UI that follows the user gaze.
            const float maxDetectionDistance = 15.0f;
            int layerMask = LayerMask.GetMask("Surfaces");
            return Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out target, maxDetectionDistance, layerMask);
        }
        /// <summary>
        /// Called when a touch object interaction occurs.
        /// </summary>
        /// <param name="hitPoint">The position.</param>
        /// <param name="target">The target.</param>
        protected virtual void OnSelectObjectInteraction(Vector3 hitPoint, object target)
        {
            Debug.Log("OnSelectObjectInteraction -> hit = " + hitPoint);

            marker.transform.position = hitPoint;

        }
    }
}
