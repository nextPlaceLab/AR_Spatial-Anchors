using LLE.Model;
using LLE.Rx;
using LLE.Unity;
using NetworkCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace LLE.Network
{
    public class RemoteModelDataManager : MonoBehaviour
    {
        
        [SerializeField]
        private int syncInterval = 1000;

        public UnityEvent<ModelData[]> OnNewModelData { get; private set; } = new UnityEvent<ModelData[]>();
        public UnityEvent<ModelData[]> OnDeleteModelData { get; private set; } = new UnityEvent<ModelData[]>();

        private Dictionary<ARId, ModelData> modelUpdates = new Dictionary<ARId, ModelData>();

        private object _lock = new object();

        private void Start()
        {
            var timerSyncModels = new Timer((s) => SendModelUpdates(), null, 1000, syncInterval);
        }

        public void SaveNewModel(ModelData model)
        {
            Debug.Log("RemoteModel: Save new model: " + model);
            LLEMain.currentInstance.Client.QueueRequest(new SetARModelRequest(new ModelData[] { model }, SetARModelRequest.CREATE), (r) => Debug.Log("Save model: success -> " + r.Success));
        }
        public void DeleteModel(ModelData model)
        {
            Debug.Log("RemoteModel: Delete model: " + model);
            LLEMain.currentInstance.Client.QueueRequest(new SetARModelRequest(new ModelData[] { model }, SetARModelRequest.DELETE), (r) => Debug.Log("Delete model: success -> " + r.Success));
        }
        public void GetModelData(string[] anchorIDs)
        {
            Dispatcher.Process(() =>
            {
                foreach (var anchorId in anchorIDs)
                {
                    Debug.Log("GetModelData for anchor: " + anchorId);
                    LLEMain.currentInstance.Client.QueueRequest(new GetARModelRequest(anchorId), OnARModelResponse);
                }
            });
        }

        private void OnARModelResponse(Response response)
        {
            if (response.Success && response.Request is GetARModelRequest)
            {
                GetARModelRequest gmr = (GetARModelRequest)response.Request;
                ModelData[] models = gmr.Result;
                if (models.Length == 0)
                {
                    Debug.LogFormat("OnARModelResponse:  anchor = {0}, task = {1} -> #models = 0", gmr.AnchorId, gmr.Task);
                    return;
                }

                if (response is DeleteResponse)
                {
                    Debug.LogFormat("OnARModelResponse:  DELETE MODEL anchor = {0}, #models = {1}, task = {2}, tid = {3}", gmr.AnchorId, models.Length, gmr.Task, Thread.CurrentThread.ManagedThreadId);
                    OnDeleteModelData.Invoke(models);
                }
                else
                {
                    Debug.LogFormat("OnARModelResponse: ADD MODEL anchor = {0}, #models = {1}, task = {2}, tid = {3}", gmr.AnchorId, models.Length, gmr.Task, Thread.CurrentThread.ManagedThreadId);
                    OnNewModelData.Invoke(models);
                }
            }
            else
            {
                Debug.LogErrorFormat("OnAnchorResponse: error response '{0}'", response);
            }
        }

        public void FinishModelDataUpdate(ModelData data)
        {
            LLEMain.currentInstance.Client.QueueRequest(new SetARModelRequest(new ModelData[] { data }, SetARModelRequest.UPDATE_FINISHED), (r) => Debug.Log("Send update finished: success -> " + r.Success));
        }
        public void UpdateModelData(ModelData data)
        {
            // TODO optional: store every update of an model and send the history
            // so that the other clients can process the changes frame by frame
            // with some networt delay
            //Debug.Log("ApplyUserModelDataChange -> tid = " + Thread.CurrentThread.ManagedThreadId);
            lock (_lock)
            {
                if (!modelUpdates.ContainsKey(data.Id))
                    modelUpdates.Add(data.Id, data);
                else
                    modelUpdates[data.Id] = Merge(modelUpdates[data.Id], data);
            }
        }

        private ModelData Merge(ModelData current, ModelData other)
        {
            if (current.Id.Equals(other.Id))
            {
                current.Pose = other.Pose ?? current.Pose;
                current.Data = other.Data ?? current.Data;
                current.Message = other.Message ?? current.Message;
                current.Context = other.Context ?? current.Context;
            }
            return current;
        }

        private void SendModelUpdates()
        {
            if (LLEMain.currentInstance.Client.IsConnected())
            {
                ModelData[] updates = null;
                lock (_lock)
                {
                    updates = modelUpdates.Values.ToArray();
                    modelUpdates.Clear();
                }
                if (updates != null && updates.Length > 0)
                {
                    Debug.Log("send model updates -> count = " + ((updates == null) ? 0 : updates.Length));
                    LLEMain.currentInstance.Client.QueueRequest(new SetARModelRequest(updates, SetARModelRequest.UPDATE), (r) => Debug.Log("Send updates: success -> " + r.Success));
                }
            }
            else
            {
                //Debug.Log("send model updates -> not connected");
            }
        }




    }
}
