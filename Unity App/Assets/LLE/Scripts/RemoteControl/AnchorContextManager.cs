using LLE.Model;
using LLE.Network;
using LLE.Rx;
using Microsoft.Azure.SpatialAnchors.Unity;
using NetworkCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace LLE.ASA
{
    public class AnchorContextManager : MonoBehaviour
    {
        [SerializeField]
        private string context = "";
        public UnityEvent<string[]> OnNewContextIDs = new UnityEvent<string[]>();
        public UnityEvent<string> OnContextSet { get; private set; } = new UnityEvent<string>();
        public UnityEvent<string[]> OnNewAnchorIDs { get; private set; } = new UnityEvent<string[]>();
        public UnityEvent<string[]> OnDeleteAnchorIDs { get; private set; } = new UnityEvent<string[]>();

        public string Context
        {
            get => context;
            private set
            {
                context = value;
                SaveContext(context);

                OnContextSet.Invoke(context);
            }
        }

        private void SaveContext(string context)
        {
            Dispatcher.ProcessOnUI(() =>
            {
                PlayerPrefs.SetString(ANCHOR_CONTEXT_KEY, Context);
                PlayerPrefs.Save();
            });
        }

        private void LoadContext()
        {
            Dispatcher.ProcessOnUI(() =>
            {
                Context = PlayerPrefs.GetString(ANCHOR_CONTEXT_KEY, "empty");
            });
        }

        private static string ANCHOR_CONTEXT_KEY;

        private void Start()
        {
            LoadContext();
        }

        public void RequestKnownContexts()
        {
            Debug.Log("RemoteAnchorManager: request known ContextIDs");
            LLEMain.currentInstance.Client.QueueRequest(new GetContextRequest(), (r) =>
            {
                if (r.Success && r.Request is GetContextRequest)
                {
                    GetContextRequest gcr = (GetContextRequest)r.Request;
                    Debug.Log("RemoteAnchorManager: onNewContextIDs -> #count = " + gcr.Result.Length);
                    OnNewContextIDs.Invoke(gcr.Result);
                }
                else
                {
                    Debug.LogError("Error request konwn contextIDs");
                }
            });
        }
        public void ReapplyContext()
        {
            Debug.Log("Re-apply context: " + context);
            RequestContextSwitch(context);
        }
        public void RequestContextSwitch(string context)
        {
            Debug.Log("Dispatch context on tid = " + Thread.CurrentThread.ManagedThreadId);
            Dispatcher.Process(() => SwitchContext(context));

        }

        private void SwitchContext(string context)
        {
            Debug.Log("Switch context '" + context + "' on tid = " + Thread.CurrentThread.ManagedThreadId);
            Context = context;
            // request anchor of context
            RequestAnchor(context);
        }
        private void RequestAnchor(string context)
        {
            LLEMain.currentInstance.Client.QueueRequest(new GetAnchorRequest(context, true), OnGetAnchorResponse);
        }

        private void OnGetAnchorResponse(Response response)
        {
            if (response.Success && response.Request is GetAnchorRequest)
            {
                GetAnchorRequest gar = (GetAnchorRequest)response.Request;
                string[] anchorIds = gar.Result;

                Debug.LogFormat("OnGetAnchorResponse: context = {0} -> #anchor = {1}, tid = {2}", gar.AnchorContext, anchorIds.Length, Thread.CurrentThread.ManagedThreadId);

                if (anchorIds.Length > 0)
                {
                    if (response is DeleteResponse)
                        OnDeleteAnchorIDs.Invoke(anchorIds);
                    else
                        OnNewAnchorIDs.Invoke(anchorIds);
                }
            }
            else
            {
                Debug.LogErrorFormat("OnGetAnchorResponse: error response '{0}'", response);
            }
        }
        public void SaveAnchor(AnchorMarker anchorGo)
        {
            Dispatcher.ProcessOnUI(() =>
            {
                try
                {
                    string id;
                    var cna = anchorGo.gameObject.GetComponent<CloudNativeAnchor>();
                    if (cna != null && cna.CloudAnchor != null)
                    {
                        id = cna.CloudAnchor.Identifier;
                    }
                    else
                    {
                        Debug.Log("Save -> CloudNativeAnchor not found --> Use Dummy");
                        AnchorWrapper anchor = new AnchorWrapper(anchorGo.gameObject.name, anchorGo.gameObject.transform);
                        id = anchor.identifier;
                    }
                    if (id == "")
                    {
                        Debug.LogError("AnchorContext: attempt to add empty anchorId");
                    }
                    else
                    {
                        Debug.LogFormat("Save anchor '{0}' at {1} with context '{2}', tid = {3}", id, anchorGo.anchor.GetPose().position, context, Thread.CurrentThread.ManagedThreadId);
                        SendSetAnchorRequest(new SetAnchorRequest(id, context, SetAnchorRequest.CREATE));
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("RemoteAnchor: save exception " + e);
                }
            });
        }

        private void SendSetAnchorRequest(SetAnchorRequest req)
        {
            LLEMain.currentInstance.Client.QueueRequest(req, OnSetAnchorResponse);
        }

        private void OnSetAnchorResponse(Response response)
        {
            if (response.Success && response.Request is SetAnchorRequest)
            {
                SetAnchorRequest sar = (SetAnchorRequest)response.Request;
                Debug.LogFormat("SetAnchorResponse: anchor = {0}, task = {1}, success = {2}", sar.AnchorId, sar.Task, sar.Result);
            }
            else
            {
                Debug.LogErrorFormat("OnSetAnchorResponse: error response '{0}'", response);
            }
        }

        public void DeleteAnchor(string anchorId)
        {
            try
            {
                Debug.Log("NetAnchorManager: Delete anchor Id:" + anchorId + " -> context: " + context + ", tid = " + Thread.CurrentThread.ManagedThreadId);
                SendSetAnchorRequest(new SetAnchorRequest(anchorId, context, SetAnchorRequest.DELETE));
            }
            catch (Exception e)
            {
                Debug.Log("RemoteAnchor: delete exception " + e);
            }

        }
        
    }
}