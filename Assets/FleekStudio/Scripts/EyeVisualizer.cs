/**
*
* Copyright (c) 2021 XZIMG Limited , All Rights Reserved
* No part of this software and related documentation may be used, copied,
* modified, distributed and transmitted, in any form or by any means,
* without the prior written permission of XZIMG Limited
*
* contact@xzimg.com, www.xzimg.com
*
*/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XZIMG;

[RequireComponent(typeof(XmgMagicFace))]
public class EyeVisualizer : MonoBehaviour
{
    #region Public properties
    public GameObject EyePrefab;
    #endregion

    #region Private properties
    private GameObject m_LeftEye;
    private GameObject m_RightEye;
    #endregion
    
    void Start()
    {
        if (EyePrefab == null)
            return;

        var faceTracking = FindObjectOfType<XmgMagicFaceTracking>();
        if (faceTracking == null || !faceTracking.TrackEyesPositions)
            Debug.LogWarning("No active XmgMagicFaceTracking detected. Add or enable it to display eyes.");
        else if (!faceTracking.TrackEyesPositions)
            Debug.LogWarning("TrackEyesPositions is no enable on XmgMagicFaceTracking. Enable it to display eyes.");
        else
        {
            m_LeftEye = Instantiate(EyePrefab, transform);
            m_LeftEye.name = $"{EyePrefab.name} (Left)";
            m_RightEye = Instantiate(EyePrefab, transform);
            m_RightEye.name = $"{EyePrefab.name} (Right)";
        }
    }

    private void OnEnable()
    {
        GetComponent<XmgMagicFace>().FaceDataUpdated += OnFaceDataUpdated;
    }

    private void OnDisable()
    {
        GetComponent<XmgMagicFace>().FaceDataUpdated -= OnFaceDataUpdated;
    }

    private void OnFaceDataUpdated(XmgMagicFaceData faceData)
    {
        if (m_LeftEye == null || m_RightEye == null)
            return;

        m_LeftEye.transform.localPosition =
            new Vector3(
                -faceData.m_faceData.m_eyesPosition3D.xLeft,
                -faceData.m_faceData.m_eyesPosition3D.yLeft,
                faceData.m_faceData.m_eyesPosition3D.zLeft);
        m_RightEye.transform.localPosition =
            new Vector3(
                -faceData.m_faceData.m_eyesPosition3D.xRight,
                -faceData.m_faceData.m_eyesPosition3D.yRight,
                faceData.m_faceData.m_eyesPosition3D.zRight);
    }
}
