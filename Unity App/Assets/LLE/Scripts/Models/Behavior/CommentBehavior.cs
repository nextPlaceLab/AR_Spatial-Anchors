using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NetworkCommon;

using System.Threading;

namespace LLE.Model
{
    public class CommentBehavior : MonoBehaviour
    {
        [SerializeField]
        private TextMeshPro uiText;

        public ARModel  model;

        private string comment;
        private bool isDataUpdate = false;
        private void Start()
        {
            if (uiText == null)
                Debug.LogError("CommentBehavior: uiText == null");

            if (model == null)
                Debug.LogError("CommentBehavior: model == null");

        }
        private void Update()
        {
            if (isDataUpdate)
            {
                isDataUpdate = false;
                uiText.text = comment;
            }
        }
        
        public void SetComment(string msg)
        {
            Debug.LogFormat("Set comment: {0}, tid = {1}", msg, Thread.CurrentThread.ManagedThreadId);
            comment = msg;
            isDataUpdate = true;
        }

    }
}
