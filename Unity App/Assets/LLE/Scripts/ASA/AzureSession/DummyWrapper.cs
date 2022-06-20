
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading;
using UnityEngine;
using Random = System.Random;

namespace LLE.ASA
{
    class DummyWrapper : AbstractAzureWrapper
    {
        private Random random = new Random();
        public string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public override void CreateAnchor(Vector3 pos, Quaternion rot, GameObject prefab)
        {
            var marker = GameObject.Instantiate(prefab);
            marker.transform.position = pos;
            marker.transform.rotation = rot;

            AnchorWrapper anchor = new AnchorWrapper(RandomString(8), marker.transform);
            callbacks.OnAnchorCreated.Invoke(marker);
        }

        public override void FindAnchor(ICollection<string> anchorIDs)
        {
            Debug.Log("AzureDummy.FindAnchor() -> #anchor = " + anchorIDs.Count);
            foreach (var id in anchorIDs)
                simulateWatcher(id);
        }
        private void simulateWatcher(string anchorID)
        {
            Debug.Log("Simulated tracking: id= " + anchorID);
            Timer t = new Timer(o =>
            {
                AnchorWrapper anchor = new AnchorWrapper(anchorID, GetRndPosition(0, -0.5f, 2, 2, 2, 2), Quaternion.identity);
                UnityDispatcher.InvokeOnAppThread(() => callbacks.OnAnchorLocated.Invoke(anchor));
                Debug.Log("DummyASA: OnAnchorLocated successfully notified");
                Timer t = (Timer)o;
                t.Dispose();
            });

            t.Change(1000, 0);

        }
        public Vector3 GetRndPosition(float xIn, float yIn, float zIn, float wX, float wY, float wZ)
        {
            //Random rnd = new Random();
            float x = (float)(random.NextDouble() * wX + xIn - wX / 2);
            float y = (float)(random.NextDouble() * wY + yIn - wY / 2);
            float z = (float)(random.NextDouble() * wZ + zIn - wZ / 2);
            var pos = new Vector3(x, y, z);
            Debug.LogFormat("new Random pos = {0}", pos);
            return pos;
        }
        public override void GetNearAnchor(AnchorWrapper anchor, float distInMeter = float.MaxValue, int maxAnchor = int.MaxValue)
        {
            Debug.Log("AzureDummy.GetNearAnchor() -> " + anchor.identifier);
        }

        public override void StartSession()
        {
            Debug.Log("AzureDummy.StartSession()");
        }

        public override void StopSession()
        {
            Debug.Log("AzureDummy.StopSession()");
        }

        public override void StopFindAnchor(ICollection<string> anchorIDs)
        {
            Debug.Log("AzureDummy.StopFindAnchor() -> #anchor = " + anchorIDs.Count);
        }

        public override void GetAnchorNearDevice(float distInMeter = float.MaxValue, int maxAnchor = int.MaxValue)
        {
            Debug.Log("AzureDummy.GetNearAnchor() -> device");
        }

        public override void SetBypassCache(bool BypassCache)
        {
            Debug.Log("AzureDummy.SetByPassCache()");
        }

        public override void ResetSession()
        {
            Debug.Log("AzureDummy.Reset");
        }

        public override void DeleteAnchor(CloudSpatialAnchor cloudSpatialAnchor)
        {
            Debug.Log("AzureDummy.Delete");
        }
    }
}
