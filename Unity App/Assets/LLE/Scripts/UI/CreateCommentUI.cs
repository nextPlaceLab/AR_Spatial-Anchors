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

public class CreateCommentUI : MonoBehaviour
{



    [SerializeField]
    private Vector3 offsetComment;

    [SerializeField]
    private Vector3 scaleComment = Vector3.one;

    [SerializeField]
    private ModelManager modelManager;

    [SerializeField]
    private AnchorMaster anchorMaster;
    [SerializeField]
    private JumpToTarget JumpToTarget;

    private AnchorMarker anchorGO;
    private string comment = "";

    public string Comment
    {

        get
        {
            return comment;
        }

        set
        {
            comment = value;
            Debug.Log("comment = " + comment);
        }
    }

    
    void Start()
    {
        if (JumpToTarget == null)
            JumpToTarget = GetComponent<JumpToTarget>();
    }

    
    void Update()
    {

    }
    public void ShowDialog()
    {
        Debug.Log("OnCreateNewComment -> tid = " + Thread.CurrentThread.ManagedThreadId);
        if (anchorMaster.selectedAnchor != null)
        {

            ShowDialog(anchorMaster.selectedAnchor);
        }
        else
        {
            Debug.Log("No anchor selected");
        }

    }
    public void ShowDialog(AnchorMarker anchor)
    {
        Debug.Log("OnCreateNewComment: " + anchor.anchor.identifier + " -> tid = " + Thread.CurrentThread.ManagedThreadId);
        if (JumpToTarget != null)
            JumpToTarget.Move();
        anchorGO = anchor;        
        gameObject.SetActive(true);

    }


    public void OnBtnCreateClicked()
    {
        Debug.Log("OnCreateComment: create -> " + comment.Trim());
        CreateComment(anchorGO.anchor.identifier, comment.Trim(), new Pose(offsetComment, Quaternion.identity), scaleComment);
        OnDismiss();
    }

    public void OnBtnCancelClicked()
    {
        Debug.Log("OnCreateComment: cancel");
        OnDismiss();
    }
    private void OnDismiss()
    {
        gameObject.SetActive(false);
    }


    private bool CreateComment(string anchorId, string comment, Pose pose, Vector3 scale)
    {
        if (comment == "")
        {
            Debug.Log("Create comment: comment is empty -> set dummy");
            comment = "ohne Worte";
        }

        if (anchorMaster.locatedAnchor.anchorGoById.ContainsKey(anchorId))
        {
            var md = new ModelData("comment_" + anchorId + "_" + DateTime.Now.ToString(),
                                    anchorId,
                                    UnityConverter.Convert(pose),
                                    UnityConverter.Convert(scale),
                                    ModelData.ModelType.TXT,
                                    comment);

            modelManager.AddCreatedModelData(md);
            return true;
        }
        else
        {
            Debug.Log("CreateComment: anchor not found -> " + anchorId);
            return false;
        }


    }
}
