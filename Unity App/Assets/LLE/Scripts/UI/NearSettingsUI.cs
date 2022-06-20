using Assets.LLE.Scripts.ASA.Anchor.Behavior;
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
using UnityEngine.UI;

public class NearSettingsUI : MonoBehaviour
{

    [SerializeField]
    private Text textMaxAnchor;

    [SerializeField]
    private Text textMaxDist;

    [SerializeField]
    private AnchorController anchorMaster;

    [SerializeField]
    private JumpToTarget JumpToTarget;

    private void Start()
    {
        if (JumpToTarget == null)
            JumpToTarget = GetComponent<JumpToTarget>();
    }


    public void ShowDialog()
    {
        Debug.Log("Show NearSettings");
        if (JumpToTarget != null)
            JumpToTarget.Move();

        gameObject.SetActive(true);

    }


    public void OnBtnApply()
    {
        int maxAnchor;
        int.TryParse(textMaxAnchor.text, out maxAnchor);
        maxAnchor = maxAnchor < 1 ? 1 : maxAnchor;

        int maxDist;
        int.TryParse(textMaxDist.text, out maxDist);
        maxDist = maxDist < 1 ? 1 : maxDist;
        Debug.LogFormat("Apply Near Setting: #= {0}, dist = {1}", maxAnchor, maxDist);
        anchorMaster.SetNearParameter(maxAnchor, maxDist);
        OnDismiss();
    }

    public void OnBtnCancel()
    {
        Debug.Log("NearSettings: cancel");
        OnDismiss();
    }
    private void OnDismiss()
    {
        gameObject.SetActive(false);
    }



}
