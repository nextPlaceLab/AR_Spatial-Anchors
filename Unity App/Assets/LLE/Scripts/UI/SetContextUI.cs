using LLE.ASA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace LLE.Unity
{
    public class SetContextUI : MonoBehaviour
    {
        [SerializeField]
        private AnchorContextManager AnchorContextManager;

        [SerializeField]
        private UIStringList uiContextList;

        [SerializeField]
        private TMP_InputField uiInputField;

        [SerializeField]
        private JumpToTarget JumpToTarget;
        private void Start()
        {
            if (JumpToTarget == null)
                JumpToTarget = GetComponent<JumpToTarget>();

            if (AnchorContextManager == null)
                Debug.LogError("SetContextUI: anchor context manager not set");
           
            if (uiContextList == null)
                Debug.LogError("SetContextUI: ui context list not set");
            
            if (uiInputField == null)
                Debug.LogError("SetContextUI: context input field not set");
        }
        public void ShowDialog()
        {
            Debug.Log("Show context dialog: current = " + AnchorContextManager.Context);
            uiInputField.text = AnchorContextManager.Context;
            if (JumpToTarget != null)
                JumpToTarget.Move();
            
            gameObject.SetActive(true);
        }
        public void OnBtnSetContext()
        {
            SetContext(uiInputField.text.Trim());
            OnDismiss();
        }
        public void SetKnownContexts(ICollection<String> contexts)
        {
            Debug.Log("SetKnownContexts -> count = " + contexts.Count);
            uiContextList.Add(contexts);
        }

        public void OnContextListSelection(string selectedContext)
        {
            uiInputField.text = selectedContext;
        }
        private void SetContext(string context)
        {
            uiInputField.text = context;
            AnchorContextManager.RequestContextSwitch(context);
        }

        public void OnBtnCancel()
        {
            OnDismiss();
        }

        private void OnDismiss()
        {
            gameObject.SetActive(false);
        }
    }
}
