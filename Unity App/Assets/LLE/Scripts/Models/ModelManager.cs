
using Assets.LLE.Scripts.Models;
using LLE.ASA;
using LLE.Network;
using LLE.Rx;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using NetworkCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace LLE.Model
{
    public class ModelManager : MonoBehaviour
    {

        [SerializeField]
        private PrefabProvider arPrefabProvider;

        [SerializeField]
        private int modelCount;

        // Note: ensure that all event listener get notified both editor and runtime
        // otherwise unity will check weather it is runtime or not which will raise an error if not called from ui thread
        public UnityEvent<ModelData> OnUserInteraction = new UnityEvent<ModelData>();
        public UnityEvent<ModelData> OnUserInteractionFinished = new UnityEvent<ModelData>();
        public UnityEvent<ModelData> OnNewModelDataCreated = new UnityEvent<ModelData>();
        public UnityEvent<ModelData> OnModelRemoved = new UnityEvent<ModelData>();
        public UnityEvent<ModelData> OnModelDeleted = new UnityEvent<ModelData>();

        public UnityEvent<ARModel> OnModelInstantiated = new UnityEvent<ARModel>();
        public UnityEvent<ARModel> OnModelSelected = new UnityEvent<ARModel>();
        public UnityEvent<ARModel, bool> OnModelTouch = new UnityEvent<ARModel, bool>();

        public ARModel SelectedModel { get; private set; }

        private Dictionary<ARId, ARModel> modelById = new Dictionary<ARId, ARModel>(new ARIdComparer());
        //private Dictionary<ARId, ModelData> modelDataByIds = new Dictionary<ARId, ModelData>(new ARIdComparer());
        private Dictionary<string, HashSet<ModelData>> modelDataByAnchor = new Dictionary<string, HashSet<ModelData>>();
        private Dictionary<string, AnchorMarker> anchorById = new Dictionary<string, AnchorMarker>();

        private HashSet<Tuple<string, ARId>> triggerdAnchorModels = new HashSet<Tuple<string, ARId>>();
        private Dictionary<string, HashSet<ARModel>> spawnedModels = new Dictionary<string, HashSet<ARModel>>();

        private void Start()
        {
            if (arPrefabProvider == null)
                Debug.LogError("ModelManager: ar prefab provider == null");
        }
        public void DeleteSelectedModel()
        {
            Debug.Log("ModelManager: delete selected model");
            if (SelectedModel != null)
            {
                DeleteModelModelData(SelectedModel.modelData);
            }
        }
        public void DeleteModelModelData(ModelData[] modelData)
        {
            foreach (var item in modelData)
            {
                DeleteModelModelData(item);
            }
        }
        public void DeleteModelModelData(ModelData modelData)
        {
            if (RemoveModelData(modelData))
            {
                if (modelById.ContainsKey(modelData.Id))
                    RemoveModel(modelById[modelData.Id]);

                OnModelDeleted.Invoke(modelData);
            }
        }

        public void AddCreatedModelData(ModelData model)
        {
            Debug.Log("######   Add User Model  #######");
            AddModelData(model);
            OnNewModelDataCreated.Invoke(model);
        }
        private bool isExisting(ModelData item)
        {
            return modelDataByAnchor.ContainsKey(item.Id.anchorId) &&
                modelDataByAnchor[item.Id.anchorId].Contains(item);
        }
        public void AddOrUpdateModelData(ModelData[] models)
        {
            Debug.Log("addOrUpdate modeldata");
            foreach (var item in models)
            {
                if (isExisting(item))
                {
                    if (modelById.ContainsKey(item.Id))
                    {
                        Debug.Log("ModelManager: Update modeldata: " + item.Id);
                        modelById[item.Id].UpdateFromRemote(item);
                    }
                    else
                    {
                        SpawnModels();
                        // add would override existing data -> update may not contain all of the data
                        //modelDataByAnchor[item.Id.anchorId].Add(item);
                        Debug.LogError("ModelManager: Model not found: " + item.Id);
                    }
                }
                else
                {
                    AddModelData(item);
                }
            }
        }

        public void OnAnchorLocated(AnchorMarker anchorGo)
        {
            if (!anchorById.ContainsKey(anchorGo.anchor.identifier))
                anchorById.Add(anchorGo.anchor.identifier, anchorGo);
            else
                anchorById[anchorGo.anchor.identifier] = anchorGo;

            SpawnModels();
        }
        public void OnAnchorRemoved(AnchorMarker anchorMarker)
        {
            OnAnchorRemoved(anchorMarker.anchor.identifier);
        }
        public void OnAnchorRemoved(string anchorId)
        {
            Debug.Log("ModelManager: OnAnchorRemove-> " + anchorId);
            if (modelDataByAnchor.ContainsKey(anchorId))
                RemoveModel(spawnedModels[anchorId]);

            anchorById.Remove(anchorId);

        }


        private void AddModelData(ModelData model)
        {
            if (!modelDataByAnchor.ContainsKey(model.Id.anchorId))
                modelDataByAnchor.Add(model.Id.anchorId, new HashSet<ModelData>(new ModelDataComparer()));
            modelDataByAnchor[model.Id.anchorId].Add(model);

            SpawnModels();
        }

        private bool RemoveModelData(ModelData model)
        {
            if (modelDataByAnchor.ContainsKey(model.Id.anchorId))
                return modelDataByAnchor[model.Id.anchorId].Remove(model);
            return false;
        }


        private void RemoveModel(ARModel model)
        {
            RemoveModel(new ARModel[] { model });
        }
        private void RemoveModel(ICollection<ARModel> modelsToDelete)
        {
            ARModel[] models = modelsToDelete.ToArray();
            var toDestroy = new HashSet<GameObject>();
            foreach (var item in models)
            {
                UnregisterModel(item);
                toDestroy.Add(item.gameObject);

            }
            Dispatcher.Process(() =>
            {
                foreach (var md in models)
                {
                    Debug.Log("Remove modeldata: " + md);
                    OnModelRemoved.Invoke(md.modelData);
                }

            });

            Dispatcher.ProcessOnUI(() =>
            {
                foreach (var item in toDestroy)
                    Destroy(item);
            });

        }

        private void NotifiyUserModelUpdate(ModelData data)
        {
            //Debug.Log("ModelManager: Notify user data");
            OnUserInteraction.Invoke(data);
        }
        private void NotifiyUserInteractionFinished(ModelData data)
        {
            Debug.Log("ModelManager: Notify user interaction finished");
            OnUserInteractionFinished.Invoke(data);
        }

        private void RegisterModel(ARModel model)
        {
            modelCount++;
            model.OnUserModelInteraction.AddListener(NotifiyUserModelUpdate);
            model.OnUserInteractionFinished.AddListener(NotifiyUserInteractionFinished);
            model.interactableHandler.OnTouchChange.AddListener((isTouch) => OnModelTouchChanged(model, isTouch));

            if (modelById.ContainsKey(model.modelData.Id))
                modelById[model.modelData.Id] = model;
            else
                modelById.Add(model.modelData.Id, model);

            if (!spawnedModels.ContainsKey(model.modelData.Id.anchorId))
                spawnedModels.Add(model.modelData.Id.anchorId, new HashSet<ARModel>());

            spawnedModels[model.modelData.Id.anchorId].Add(model);
        }
        private void UnregisterModel(ARModel model)
        {
            if (modelById.Remove(model.modelData.Id))
            {
                Debug.Log("ModelManager: remove model ->" + model.modelData.Id);
                modelCount--;
                model.interactableHandler.OnTouchChange.RemoveListener((s) => OnModelTouchChanged(model, s));
                model.OnUserModelInteraction.RemoveListener(NotifiyUserModelUpdate);
                model.OnUserInteractionFinished.RemoveListener(NotifiyUserInteractionFinished);
                spawnedModels[model.modelData.Id.anchorId].Remove(model);
            }
            else
            {
                Debug.LogError("Model not found -> " + model.modelData.Id);
            }
        }

        private void OnModelTouchChanged(ARModel model, bool isTouchBegin)
        {
            Debug.Log("OnModelTouch: " + model.modelData.Id + ", isTouchBegin = " + isTouchBegin);
            if (isTouchBegin)
            {
                SelectedModel = model;
                OnModelSelected.Invoke(model);
            }

            OnModelTouch.Invoke(model, isTouchBegin);
        }

        private void InstaniateArModel(AnchorMarker anchorGO, ModelData modelData)
        {
            GameObject prefab = arPrefabProvider.GetPrefab(modelData);
            Dispatcher.ProcessOnUI(() =>
            {
                var go = InstantiatePrefab(anchorGO.gameObject.transform, UnityConverter.Convert(modelData.Pose), UnityConverter.Convert(modelData.Scale), prefab);
                var arm = CreateARModel(go, modelData);
                if (modelData.Type == ModelData.ModelType.TXT)
                {
                    var cb = arm.gameObject.GetComponent<CommentBehavior>();
                    if (cb != null)
                    {
                        cb.model = arm;
                        //cb.OnBtnSetText(modelData.Message);
                        //cb.SetComment(modelData.Message);
                        var comment = modelData.Message != "" ? modelData.Message : "ohne Worte";
                        cb.SetComment(comment);
                    }
                }

                RegisterModel(arm);
                OnModelInstantiated.Invoke(arm);
            });
        }

        private ARModel CreateARModel(GameObject go, ModelData model)
        {
            go.name = model.Id + "_" + model.Id.anchorId;

            go.AddComponent<AutoBoxCollider>();
            go.AddComponent<ObjectManipulator>();
            go.AddComponent<NearInteractionGrabbable>();

            var interactable = go.GetComponent<Interactable>();
            if (interactable == null)
                interactable = go.AddComponent<Interactable>();

            var poseObserver = go.AddComponent<ModelPoseObserver>();

            var handler = new InteractableHandler();
            interactable.AddHandler(handler);

            var arModel = new ARModel(go, model, poseObserver, interactable, handler);

            poseObserver.model = arModel;

            return arModel;
        }

        private GameObject InstantiatePrefab(Transform anchor, Pose pose, Vector3 scale, GameObject prefab)
        {
            var go = Instantiate(prefab, pose.position, pose.rotation);
            go.transform.localScale = scale;
            go.transform.SetParent(anchor, false);

            return go;
        }

        private void SpawnModels()
        {
            Debug.Log("ARModelManager: check spawn Model, tid = " + Thread.CurrentThread.ManagedThreadId);
            var selection = new List<Action>();
            foreach (var anchorId in anchorById.Keys)
            {
                if (modelDataByAnchor.ContainsKey(anchorId))
                {
                    foreach (var model in modelDataByAnchor[anchorId])
                    {
                        var anchorModel = new Tuple<string, ARId>(anchorId, model.Id);
                        if (!triggerdAnchorModels.Contains(anchorModel))
                        {
                            InstaniateArModel(anchorById[anchorId], model);
                            triggerdAnchorModels.Add(anchorModel);
                        }
                    }
                }
                else
                {
                    Debug.Log("check anchorId = " + anchorId + " -> NO Models found");
                }
            }
        }
    }
}
