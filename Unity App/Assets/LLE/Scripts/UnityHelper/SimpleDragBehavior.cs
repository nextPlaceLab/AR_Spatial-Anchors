using UnityEngine;

namespace LLE.Model
{
    public class SimpleDragBehavior : MonoBehaviour
    {
        private Vector3 initPosition;
        private Quaternion initRotation;
        private void Start()
        {
            initPosition = transform.position;
            initRotation = transform.rotation;

            var collider = gameObject.GetComponentsInChildren<Collider>();
            foreach (var col in collider)
            {
                col.gameObject.AddComponent<DragController>().modelParent = transform;
                //Debug.Log("Add dragbehavior: " + col.gameObject.name);
            }
        }
        public void ResetPosition()
        {
            transform.position = initPosition;
            transform.rotation = initRotation;
            Debug.Log("Reset position: " + name);
        }
    }
}