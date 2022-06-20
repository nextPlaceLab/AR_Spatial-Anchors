using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LLE.Unity
{
    public class ToggleVisibility : MonoBehaviour
    {
        [SerializeField]
        private List<MeshRenderer> renderer;
        [SerializeField]
        private bool isVisible;
        private void Start()
        {
            if (renderer != null && renderer.Count > 0)
                isVisible = renderer[0].enabled;
            else
                isVisible = gameObject.activeSelf;
        }
        
        public void Toggle()
        {
            isVisible = !isVisible;
            SetVisibilty(isVisible);
        }

        public void SetVisibilty(bool isVisible)
        {
            this.isVisible = isVisible;
            if (renderer != null && renderer.Count > 0)
                ToggleRenderer(isVisible);
            else
                gameObject.SetActive(isVisible);
        }

        private void ToggleRenderer(bool isEnabled)
        {
            foreach (var item in renderer)
            {
                item.enabled = isEnabled;
            }
        }

    }
}
