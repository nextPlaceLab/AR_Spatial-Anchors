using UnityEngine;
using System.Collections;

//[RequireComponent(typeof(Collider))]

public class DragController : MonoBehaviour
{
    private Vector3 screenPoint;
    private Vector3 offset;
    public Transform modelParent;

    void OnMouseDown()
    {
        Transform t = modelParent;
        screenPoint = Camera.main.WorldToScreenPoint(t.position);
        offset = t.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

    }

    void OnMouseDrag()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        modelParent.position = curPosition;
    }
}