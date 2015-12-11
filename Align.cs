using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine.UI;

public class Align : MonoBehaviour
{
    //█▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀█
    //█ Align
    [ContextMenuItem("Set to parent", "SetToParent")]
    public GameObject positionAlignTarget;
    [ContextMenuItem("Update offset", "CaptureOffset")]
    public Vector3 positionOffset;
    [ContextMenuItem("Set to camera", "SetToCamera")]
    public GameObject orientationAlignTarget;
    [ContextMenuItem("Update facing direction", "CaptureFacing")]
    public Vector3 facingVector = Vector3.forward;
    public enum OffsetHandling
    {
        worldOffset,
        localOffset,
        XZRelativeToDirectionToOrientationTarget,
        relativeToDirectionToOrientationTarget,
    }
    public OffsetHandling offsetHandling;
    [Header("overrides orientation")]
    public GameObject lookTarget;

    //
    Quaternion positionTargetRotationOriginal;
    Vector3 positionTargetOriginalPos;
    //Quaternion orientationTargetRotationOriginal;
    Vector3 orientationTargetOriginalPos;
    //

    void Start()
    {
        if (positionAlignTarget != null)
        {
            positionTargetRotationOriginal = positionAlignTarget.transform.rotation;
            positionTargetOriginalPos = positionAlignTarget.transform.position;
        }
        if (orientationAlignTarget != null)
        {
            //orientationTargetRotationOriginal = orientationAlignTarget.transform.rotation;
            orientationTargetOriginalPos = orientationAlignTarget.transform.position;
        }
    }

    [ContextMenu("run align")]
    //█▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀█
    void LateUpdate()
    {
        if (positionAlignTarget != null)
        {
            Vector3 offset = positionOffset;
            if (offsetHandling == OffsetHandling.localOffset)
            {
                offset = Quaternion.FromToRotation(positionTargetRotationOriginal.eulerAngles, positionAlignTarget.transform.rotation.eulerAngles) * positionOffset;
            }
            else if (offsetHandling == OffsetHandling.XZRelativeToDirectionToOrientationTarget)
            {
                Vector3 originalDisplace = orientationTargetOriginalPos - positionTargetOriginalPos;
                Vector3 displace = orientationAlignTarget.transform.position - positionAlignTarget.transform.position;
                originalDisplace.y = 0;
                displace.y = 0;
                offset = Quaternion.FromToRotation(originalDisplace, displace) * positionOffset;
            }
            else if (offsetHandling == OffsetHandling.relativeToDirectionToOrientationTarget)
            {
                Vector3 originalDisplace = orientationTargetOriginalPos - positionTargetOriginalPos;
                Vector3 displace = orientationAlignTarget.transform.position - positionAlignTarget.transform.position;
                offset = Quaternion.FromToRotation(originalDisplace, displace) * positionOffset;
            }

            transform.position = positionAlignTarget.transform.position + offset;
        }

        if (lookTarget != null)
        {
            transform.rotation = Quaternion.LookRotation(lookTarget.transform.position - transform.position);
        }
        else if (orientationAlignTarget != null)
        {
            transform.rotation = Quaternion.LookRotation(facingVector) * orientationAlignTarget.transform.rotation;
        }
    }
    //█▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄█
    public void SetToParent()
    {
        positionAlignTarget = transform.parent.gameObject;
        CaptureOffset();
    }

    public void CaptureOffset()
    {
        if (positionAlignTarget != null)
        {
            positionOffset = transform.position - positionAlignTarget.transform.position;
        }
    }

    public void SetToCamera()
    {
        orientationAlignTarget = Camera.main.gameObject;
    }

    public void CaptureFacing()
    {
        if (orientationAlignTarget != null)
        {
            facingVector = orientationAlignTarget.transform.rotation * transform.forward;
        }
    }
    //███████████████████████████████████████████████
}