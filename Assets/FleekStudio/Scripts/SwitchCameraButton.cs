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

using UnityEngine;
using UnityEngine.UI;
using XZIMG;

[RequireComponent(typeof(Button))]
public class SwitchCameraButton : MonoBehaviour
{
    #region Private properties
    private XmgCameraManager m_CameraManager = null;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
#if (!UNITY_EDITOR && UNITY_ANDROID) || (!UNITY_EDITOR && UNITY_IOS)
        enabled = true;
        m_CameraManager = FindObjectOfType<XmgCameraManager>();
        
        var button = GetComponent<Button>();
        button.onClick .AddListener(OnButtonPressed);
#else
        enabled = false;
#endif
    }

    private void OnDestroy()
    {
#if (!UNITY_EDITOR && UNITY_ANDROID) || (!UNITY_EDITOR && UNITY_IOS)
        var button = GetComponent<Button>();
        button.onClick.RemoveListener(OnButtonPressed);
#endif
    }

    private void OnButtonPressed()
    {
        if (m_CameraManager != null)
        {
            m_CameraManager.SwitchCamera();
        }
    }
}
