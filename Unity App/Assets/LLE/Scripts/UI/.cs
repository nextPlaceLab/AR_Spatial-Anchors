using LLE.ASA;
using LLE.Model;
using LLE.Network;
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
    private Transform userTransform;

    [SerializeField]
    private Vector3 offsetUI;


    [SerializeField]
    private Vector3 offsetComment;


    [SerializeField]
    private ModelManager modelManager;

    [SerializeField]
    private AnchorMasterManager anchorMaster;

    private AnchorGO anchorGO;
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

    // Start is called before the first frame update
    void Start()
    {
        if (userTransform == null)
            Debug.LogError("CreateComment: user transform == null");

        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ShowDialog()
    {
        Debug.Log("OnCreateNewComment -> tid = " + Thread.CurrentThread.ManagedThreadId);
        if (anchorMaster.selectedAnchor != null)
        {
            anchorGO = anchorMaster.selectedAnchor;

            gameObject.transform.position = userTransform.position + userTransform.TransformDirection(offsetUI);
            gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("No Anchor selected");
        }
        Debug.Log("OnCreateNewComment...done");
    }


    public void OnBtnCreateClicked()
    {
        Debug.Log("OnCreateComment: create -> " + comment.Trim());
        CreateComment(anchorGO.anchor.identifier, comment.Trim(), new Pose(offsetComment, Quaternion.identity));
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


    private bool CreateComment(string anchorId, string comment, Pose pose)
    {

        if (anchorMaster.locatedAnchor.anchorGoById.ContainsKey(anchorId))
        {
            var spose = UnityConverter.Convert(pose);
            var md = new ModelData("comment_" + anchorId, anchorId, spose, ModelData.ModelType.TXT, comment);
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
