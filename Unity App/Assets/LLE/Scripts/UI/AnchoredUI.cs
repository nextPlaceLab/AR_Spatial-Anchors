using LLE.ASA;
using LLE.Model;
using LLE.Network;
using LLE.Unity;
using NetworkCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class AnchoredUI : MonoBehaviour
{
    [SerializeField]
    private Vector3 offset;

    [SerializeField]
    private Transform userTransform;

    [SerializeField]
    private AnchorMaster AnchorMaster;

    public UnityEvent<AnchorMarker> OnAnchorSet = new UnityEvent<AnchorMarker>();

    private Transform anchorTransform;
    private bool isVisible = true;

    // Start is called before the first frame update
    void Start()
    {
        if (userTransform == null)
            Debug.LogError("BTNCreateComment: user transform not set");

        if (AnchorMaster != null)
            AnchorMaster.OnAnchorSelected.AddListener(SetAnchor);
    }


    private void Update()
    {
        if ((anchorTransform != null) != isVisible)
        {
            isVisible = (anchorTransform != null);
            Debug.Log("AnchoredUI: update position -> isVisible = " + isVisible);
            GOUtils.EnableMeshRenderer(gameObject, isVisible);
        }
    }
    public void SetAnchor(AnchorMarker anchor)
    {
        Debug.Log("AnchoredUI: set Anchor");
        if (anchor != null)
            ApplyAnchor(anchor);
        else
            anchorTransform = null;
    }
    private void ApplyAnchor(AnchorMarker anchor)
    {
        anchorTransform = anchor.gameObject.transform;
        SetPos();
        OnAnchorSet.Invoke(anchor);
    }

    private void SetPos()
    {
        if (anchorTransform == null || userTransform == null)
            return;

        this.transform.position = anchorTransform.position;
        this.transform.rotation = Quaternion.LookRotation(this.transform.position - userTransform.position);
        this.transform.position += this.transform.TransformDirection(offset);
        GOUtils.EnableMeshRenderer(gameObject, true);
    }


}
