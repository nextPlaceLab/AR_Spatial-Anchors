using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using NetworkCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.LLE.Scripts.Models
{
    public class InteractableHandler : IInteractableHandler

    {
        
        public UnityEvent<bool> OnTouchChange = new UnityEvent<bool>();
        
        private bool isTouch = false;
        public InteractableHandler()
        {
        
        }
        public void OnClick(InteractableStates state, Interactable source, IMixedRealityPointer pointer = null)
        {
            //State[] states = state.GetStates();
            //Debug.Log("Click " + toString(states));
        }

        public void OnStateChange(InteractableStates state, Interactable source)
        {
            var isPressed = state.GetState(InteractableStates.InteractableStateEnum.Pressed);
            if (isPressed.Value == 1 && !isTouch)
            {
                SetTouchState(true);
            }
            else if (isPressed.Value == 0 && isTouch)
            {
                SetTouchState(false);
            }
        }

        private void SetTouchState(bool state)
        {
            isTouch = state;
            OnTouchChange.Invoke(isTouch);
            Debug.Log("OnTouch: " + (isTouch ? "begin" : "end"));
        }

        public void OnVoiceCommand(InteractableStates state, Interactable source, string command, int index = 0, int length = 1)
        {
            //State[] states = state.GetStates();
            //Debug.Log("On Voice" + toString(states));
        }
        private string toString(State[] states)
        {
            string msg = "";
            foreach (var item in states)
            {
                msg += item.Name + "= " + item.Value + ", ";
            }
            return msg;
        }
    }
}
