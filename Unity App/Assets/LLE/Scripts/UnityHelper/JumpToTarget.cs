using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LLE.Unity
{
    class JumpToTarget : MonoBehaviour
    {
        [SerializeField]
        bool LookAtTarget = false;

        [SerializeField]
        private Transform target;
        private void Start()
        {
            if (target == null)
                Debug.LogError("Target not set"); 
        }
        [SerializeField]
        private Vector3 offset;
        public void Move()
        {
            this.transform.position = target.position + target.TransformDirection(offset);
            if (LookAtTarget)
                this.transform.rotation = Quaternion.LookRotation(this.transform.position - target.position);
        }
    }
}
