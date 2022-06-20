using LLE.Model;
using LLE.Network;
using LLE.Rx;
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
    public class RemoteMaster : MonoBehaviour
    {
        [SerializeField]
        private bool isActive = false;

        [SerializeField]
        private bool SetContextAtStart = false;

        [SerializeField]
        private AnchorContextManager contextManager;

        [SerializeField]
        private RemoteModelDataManager remoteModelDataManager;

        [SerializeField]
        AnchorMaster anchorMaster;

        [SerializeField]
        ModelManager modelManager;

        public UnityEvent<string> ContextSetEvent = new UnityEvent<string>();

        private void Start()
        {
            if (isActive)
            {
                modelManager.OnNewModelDataCreated.AddListener(remoteModelDataManager.SaveNewModel);
                modelManager.OnUserInteraction.AddListener(remoteModelDataManager.UpdateModelData);
                modelManager.OnUserInteractionFinished.AddListener(remoteModelDataManager.FinishModelDataUpdate);

                contextManager.OnContextSet.AddListener(OnContextSet);
                contextManager.OnNewAnchorIDs.AddListener(OnNewAnchorIDs);
                contextManager.OnDeleteAnchorIDs.AddListener(OnDeleteAnchorIDs);


                remoteModelDataManager.OnNewModelData.AddListener(OnNewModelData);
                remoteModelDataManager.OnDeleteModelData.AddListener(OnDeleteModelData);


                anchorMaster.OnAnchorCreated.AddListener(contextManager.SaveAnchor);
                anchorMaster.OnAnchorDeleted.AddListener(contextManager.DeleteAnchor);

            }
        }

        public void OnStart()
        {
            Debug.Log("RemoteMaster: OnStart");

            contextManager.RequestKnownContexts();

            if (SetContextAtStart)
                contextManager.ReapplyContext();
            else
                Debug.LogWarning("RemoteMaster: OnStart set context disabled");
        }


        private void OnContextSet(string context)
        {
            ContextSetEvent.Invoke(context);
        }

        private void OnNewAnchorIDs(string[] anchorIDs)
        {

            // add
            if (anchorMaster.AddAnchorIDs(anchorIDs))
            {
                Debug.Log("OnNewAnchorIDs");

                // watch
                anchorMaster.WatchAll();

                // request model data 
                remoteModelDataManager.GetModelData(anchorIDs);
            }
            else
            {
                Debug.Log("OnNewAnchorIDs -> no update");
            }
        }

        private void OnDeleteAnchorIDs(string[] anchorIDs)
        {
            anchorMaster.DeleteAnchor(anchorIDs);
        }
        private void OnNewModelData(ModelData[] modelData)
        {
            modelManager.AddOrUpdateModelData(modelData);
        }
        private void OnDeleteModelData(ModelData[] modelData)
        {
            modelManager.DeleteModelModelData(modelData);
        }

    }
}