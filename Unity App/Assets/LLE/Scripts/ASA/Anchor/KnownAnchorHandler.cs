
using LLE.Network;
using LLE.Unity;
using Microsoft.Azure.SpatialAnchors.Unity;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace LLE.ASA
{
    class KnownAnchorHandler
    {
        public HashSet<string> knownAnchorIDs { get; private set; } = new HashSet<string>();
        public UnityEvent<string, bool> AnchorIDsUpdate { get; private set; } = new UnityEvent<string, bool>();
        private string fileName = "knownAnchor.mng";
        private bool isUseLocalAnchor;

        public KnownAnchorHandler(AnchorMaster anchorMaster, bool useLocalAnchor)
        {
            isUseLocalAnchor = useLocalAnchor;
            if (isUseLocalAnchor)
                LoadAnchorIDs();
        }

        public bool Add(ICollection<string> anchorIds)
        {
            bool isNew = false;
            foreach (var item in anchorIds)
            {
                if (Add(item))
                {
                    Debug.Log("KnownAncho: new anchor =" + item);
                    isNew = true;
                }
            }
            return isNew;
        }

        public bool Add(string anchorId)
        {
            Debug.Log("KnownAnchor: add id -> " + anchorId);
            if (anchorId == "")
                Debug.LogError("KnownAnchor: attempt to add empty anchorId");

            if (anchorId != "" && !knownAnchorIDs.Contains(anchorId))
            {
                knownAnchorIDs.Add(anchorId);
                AnchorIDsUpdate.Invoke(anchorId, true);
                if (isUseLocalAnchor)
                    SaveAnchorIDs();

                return true;
            }
            return false;
        }
        public void RemoveAll()
        {
            foreach (var item in knownAnchorIDs.ToArray())
            {
                Remove(item);
            }
        }
        public void Remove(string anchorId)
        {
            if (knownAnchorIDs.Remove(anchorId))
            {
                Debug.Log("KnownAnchor: remove " + anchorId);
                AnchorIDsUpdate.Invoke(anchorId, false);
                if (isUseLocalAnchor)
                    SaveAnchorIDs();
            }
        }


        private void SaveAnchorIDs()
        {
            try
            {
                string json = JsonConvert.SerializeObject(knownAnchorIDs);
                FileIO.Write(fileName, json);
                Debug.Log("Saved known anchors: count = " + knownAnchorIDs.Count);
            }
            catch (Exception e)
            {
                Debug.Log("Known Anchor: Save exception " + e);
            }
        }

        private bool LoadAnchorIDs()
        {
            try
            {
                string json = FileIO.Read(fileName);
                if (json == "")
                    Debug.Log("emtpy file => " + fileName);

                List<string> list = JsonConvert.DeserializeObject<List<string>>(json);
                if (list != null)
                {
                    Add(list);
                    Debug.Log("Loaded anchors: count = " + list.Count);
                    return list.Count > 0;
                }
            }
            catch (Exception e)
            {
                Debug.Log("KnownAnchor: Load exception " + e);
            }
            return false;
        }


    }
}