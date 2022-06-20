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

public class AssignModelUI : MonoBehaviour
{

    [SerializeField]
    private Vector3 offsetModel;

    [SerializeField]
    private Vector3 scaleModel =Vector3.one;

    [SerializeField]
    private ModelManager modelManager;

    [SerializeField]
    private AnchorMaster anchorMaster;

    [SerializeField]
    private JumpToTarget JumpToTarget;
    private AnchorMarker anchorGO;
    private string selectedModelId = "";
    private void Start()
    {
        if (JumpToTarget == null)
            JumpToTarget = GetComponent<JumpToTarget>();
    }

    public void SetModel(string value)
    {
        selectedModelId = value;
        Debug.Log("selected model = " + selectedModelId);
    }

    public void ShowDialog()
    {
        Debug.Log("AssignModel -> tid = " + Thread.CurrentThread.ManagedThreadId);
        if (anchorMaster.selectedAnchor != null)
        {
            if (JumpToTarget != null)
                JumpToTarget.Move();

            anchorGO = anchorMaster.selectedAnchor;
            gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("No Anchor selected");
        }
    }


    public void OnBtnAssignClicked()
    {
        Debug.Log("AssignModel: assign -> " + selectedModelId);
        var md = CreateModel(anchorGO.anchor.identifier, selectedModelId, new Pose(offsetModel, Quaternion.identity),scaleModel);
        modelManager.AddCreatedModelData(md);

        OnDismiss();
    }

    public void OnBtnCancelClicked()
    {
        Debug.Log("AssignModel: cancel");
        OnDismiss();
    }
    private void OnDismiss()
    {
        gameObject.SetActive(false);
    }


    private ModelData CreateModel(string anchorId, string model, Pose pose, Vector3 scale)
    {
        var md = new ModelData(model + "_" + anchorId + "_" + DateTime.Now.ToString(),
            anchorId,
            UnityConverter.Convert(pose), UnityConverter.Convert(scale),
            ModelData.ModelType.ART, model
            );

        return md;
    }


}
