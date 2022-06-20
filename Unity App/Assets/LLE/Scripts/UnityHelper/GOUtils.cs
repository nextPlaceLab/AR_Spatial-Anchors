using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LLE.Unity
{
    class GOUtils
    {
        public static void EnableMeshRenderer(GameObject go, bool isEnabled, bool applyToChildren = true)
        {
            Debug.LogFormat("GoUtils: EnableMeshRenderer of {0}, enable = {1}, children = {2}", go.name, isEnabled, applyToChildren);
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
                mr.enabled = isEnabled;

            if (applyToChildren)
                foreach (var cmr in go.GetComponentsInChildren<MeshRenderer>())
                {
                    cmr.enabled = isEnabled;
                }
        }
    }
}
