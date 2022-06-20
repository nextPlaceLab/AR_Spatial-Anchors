using Assets.LLE.Scripts.Models;
using LLE.Network;
using Microsoft.MixedReality.Toolkit.UI;
using NetworkCommon;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace LLE.Model
{
    [Serializable]
    public class ARModel
    {
        public GameObject gameObject { get; private set; }
        public ModelData modelData { get; private set; }

        public UnityEvent<ModelData> OnUserModelInteraction = new UnityEvent<ModelData>();
        public UnityEvent<ModelData> OnUserInteractionFinished = new UnityEvent<ModelData>();
        public ModelPoseObserver poseObserver { get; private set; }
        public Interactable interactable { get; private set; }
        public UnityEvent<ModelData> OnRemoteUpdate = new UnityEvent<ModelData>();
        public InteractableHandler interactableHandler { get; private set; }
        private static ARIdComparer ARIdComparer = new ARIdComparer();
        public ARModel(GameObject go, ModelData modelData, ModelPoseObserver poseObserver, Interactable interactable, InteractableHandler handler)
        {
            this.gameObject = go;
            this.modelData = modelData;
            this.poseObserver = poseObserver;
            this.interactable = interactable;
            this.interactableHandler = handler;


            OnRemoteUpdate.AddListener((d) =>
            {
                poseObserver.SetExternalPose(UnityConverter.Convert(d.Pose), UnityConverter.Convert(d.Scale));
            });

            var cb = go.GetComponent<CommentBehavior>();
            if (cb != null)
                OnRemoteUpdate.AddListener((d) =>
                {
                    cb.SetComment(d.Message);
                });
        }

        public void UpdateFromRemote(ModelData data)
        {
         
            if(ARIdComparer.Equals(data.Id, modelData.Id))
            {
                // pose, scale, string are all not nullable -> how does the server handles the updates?
                Debug.LogFormat("ModelData: remote update: {0}, tid = {1}", modelData.Id, Thread.CurrentThread.ManagedThreadId);
                modelData.Pose = data.Pose ?? modelData.Pose;
                modelData.Scale = data.Scale?? modelData.Scale;
                modelData.Message = data.Message?? modelData.Message;
                modelData.Context = data.Context?? modelData.Context;
                if (data.Data.Length > 0)
                    modelData.Data = data.Data;

                OnRemoteUpdate.Invoke(modelData);
            }
            else
            {
                Debug.LogError("ModelData Update: IDs not matching");
            }

        }

        public void UpdateFinished()
        {
            Debug.LogFormat("Update finished: {0}", modelData);
            OnUserInteractionFinished.Invoke(modelData);
        }

        public void Update(Pose pose,Vector3 scale , bool notify = true)
        {
            Debug.Log("update model pose: " + modelData.Id);
            modelData.Pose = UnityConverter.Convert(pose);
            modelData.Scale = UnityConverter.Convert(scale);
            if (notify)
                OnUserModelInteraction.Invoke(modelData);
        }
        public void Update(string message, bool notify = true)
        {
            Debug.Log("update model message: " + modelData.Id);
            modelData.Message = message;
            if (notify)
            {
                OnUserModelInteraction.Invoke(modelData);
                UpdateFinished();
            }
        }
        public void Update(byte[] data, bool notify = true)
        {
            modelData.Data = data;
            if (notify)
            {
                OnUserModelInteraction.Invoke(modelData);
                UpdateFinished();
            }
        }

    }
}
