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

public class BtnCreateComment : MonoBehaviour
{
    
    [SerializeField]
    private CreateCommentUI commentUI;
    
    private AnchorMarker anchor;
    
    public void SetAnchor(AnchorMarker anchor)
    {
        this.anchor = anchor;
    }
    
    
    public void OnCreateComment()
    {
        if (anchor != null)
            commentUI.ShowDialog(anchor);
        else
            Debug.Log("OnCreateComment: anchor null");

    }
}
