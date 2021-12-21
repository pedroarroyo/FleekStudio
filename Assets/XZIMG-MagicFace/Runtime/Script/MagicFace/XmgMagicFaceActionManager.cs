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
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace XZIMG
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(XmgMagicFaceTracking))]
    public sealed class XmgMagicFaceActionManager : MonoBehaviour, IXmgMagicFaceManager
    {
        #region Public properties
        /// <summary>
        /// Raised for each new emotion detection.
        /// </summary>
        public event Action<float[][]> ActionsReceived;
        #endregion

        #region Internal properties
        /// <summary>
        /// <see cref="XmgMagicFaceTracking"/> component of this gameobject.
        /// </summary>
        internal XmgMagicFaceTracking FaceTracking => m_FaceTracking;
        #endregion

        #region Private properties
        /// <summary>
        /// <see cref="XmgMagicFaceTracking"/> component of this gameobject.
        /// </summary>
        private XmgMagicFaceTracking m_FaceTracking;
        #endregion

        private void Awake()
        {
            m_FaceTracking = GetComponent<XmgMagicFaceTracking>();
        }

        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            
        }

        #region IXmgMagicFaceManager
        void IXmgMagicFaceManager.OnXmgInitialize()
        {
        }

        void IXmgMagicFaceManager.OnXmgUpdateMagicFaces(List<XmgMagicFaceData> faceDataList)
        {
            if (isActiveAndEnabled)
                UpdateFaceData(faceDataList);
        }
        #endregion
        
        #region Tracking
        private void UpdateFaceData(List<XmgMagicFaceData> faceDataList)
        {
            float[][] actionUnits = new float[faceDataList.Count][];

            int i = 0;
            foreach (var faceData in faceDataList)
            {
                int num_AU = XmgMagicFaceBridge.xzimgMagicFaceGetNumActionUnits();
                if (num_AU > 0)
                {
                    float[] v_AU = new float[num_AU];
                    GCHandle hdl_v_AU = GCHandle.Alloc(v_AU, GCHandleType.Pinned);
                    XmgMagicFaceBridge.xzimgMagicFaceGetActionUnits(i, hdl_v_AU.AddrOfPinnedObject());
                    actionUnits[i] = v_AU;
                    hdl_v_AU.Free();
                }

                i++;
            }
            
            ActionsReceived?.Invoke(actionUnits);
        }
        #endregion
    }
}
