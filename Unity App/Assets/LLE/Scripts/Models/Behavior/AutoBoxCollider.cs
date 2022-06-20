using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoBoxCollider: MonoBehaviour
{

    void Awake()
    {
        var existingCollider = GetComponentsInChildren<Collider>();
        Debug.Log("#Collider: " + gameObject.name +": "+  existingCollider.Length );
        if (existingCollider.Length > 0)
            return;

        var meshFiler = gameObject.GetComponentsInChildren<MeshFilter>();
        foreach (var mf in meshFiler)
        {
            BoxCollider collider = mf.gameObject.AddComponent<BoxCollider>();      
            collider.center = mf.sharedMesh.bounds.center;
            collider.size= mf.sharedMesh.bounds.size;
        }
        
    }

}
