using LLE.Model;
using LLE.Unity;
using NetworkCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace LLE.Network
{
    public class ModelPoseObserver : MonoBehaviour
    {
        public ARModel model;

        public Pose currentPose;
        public Pose syncPose;
        private Pose mExternalPose;
        private bool isApplyExternal = false;

        private Vector3 currentScale;
        private Vector3 syncScale;
        private Vector3 mExternalScale;
        

        private object _lock = new object();
        private bool isManipulationInProgress = false;

        private void Start()
        {

            syncPose = new Pose(transform.localPosition, transform.localRotation);
            syncScale = transform.localScale;

            if (model == null)
            {
                Debug.Log("ModelPoseObserver: model == null");
                return;
            }
        }
        private void Update()
        {
            if (model == null)
            {
                Debug.Log("ModelPoseObserver: model == null");
                return;
            }
            lock (_lock)
            {
                if (isApplyExternal && !isManipulationInProgress)
                {
                    transform.localPosition = mExternalPose.position;
                    transform.localRotation = mExternalPose.rotation;

                    currentPose = mExternalPose;
                    syncPose = mExternalPose;

                    transform.localScale = mExternalScale;

                    currentScale = mExternalScale;
                    syncScale = mExternalScale;

                    isApplyExternal = false;
                }
            }

            currentPose = new Pose(transform.localPosition, transform.localRotation);
            currentScale = transform.localScale;

            if (syncPose != currentPose || currentScale != syncScale)
            {
                Debug.Log("arm: update pose");
                isManipulationInProgress = true;
                syncPose = currentPose;
                syncScale = currentScale;
                
                model.Update(syncPose, syncScale);
            }
            else if (isManipulationInProgress)
            {
                isManipulationInProgress = false;
                model.UpdateFinished();
            }

        }

        public void SetExternalPose(Pose pose, Vector3 scale)
        {
            lock (_lock)
            {
                mExternalPose = pose;
                mExternalScale = scale;
                isApplyExternal = true;
            }
        }
    }
}
