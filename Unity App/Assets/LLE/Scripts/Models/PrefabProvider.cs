using NetworkCommon;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace LLE.ASA
{
    public class PrefabProvider : MonoBehaviour
    {
        public List<GameObject> Prefabs;
        public GameObject errorPrefab;
        public Dictionary<string, GameObject> taggedPrefabs;
        public UnityEvent<ICollection<string>> GetModelTags = new UnityEvent<ICollection<string>>();

        public string CommentPreabName= "CommentPrefab";

        void Start()
        {

            if (errorPrefab == null)
                Debug.LogError("ARPrefabProvider: error prefab == null");
            taggedPrefabs = new Dictionary<string, GameObject>();
            Prefabs.ForEach(p => taggedPrefabs.Add(p.name, p));
            GetModelTags.Invoke(taggedPrefabs.Keys);
        }

        public GameObject GetPrefab(string name)
        {
            if (taggedPrefabs.ContainsKey(name))
                return taggedPrefabs[name];

            Debug.Log("ModelDB: No prefab found: " + name);
            return errorPrefab;
        }

        public GameObject GetPrefab(ModelData modelData)
        {
            if (modelData.Type == ModelData.ModelType.ART)
                return GetPrefab(modelData.Message);
            else if (modelData.Type == ModelData.ModelType.TXT)
                return GetPrefab(CommentPreabName);
            else
                return errorPrefab;
        }
    }
}
