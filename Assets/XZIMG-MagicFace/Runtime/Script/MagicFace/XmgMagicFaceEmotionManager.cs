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
    public sealed class XmgMagicFaceEmotionManager : MonoBehaviour, IXmgMagicFaceManager
    {
        #region Public properties
        /// <summary>
        /// Raised for each new emotion detection.
        /// </summary>
        public event Action<List<XmgEmotions>> EmotionReceived;
        #endregion

        #region Internal properties
        /// <summary>
        /// <see cref="XmgMagicFaceTracking"/> component of this gameobject.
        /// </summary>
        internal XmgMagicFaceTracking FaceTracking => m_FaceTracking;
        #endregion

        #region Private properties
        /// <summary>
        /// Face emotion detection init parameters
        /// </summary>
        private XmgInitCNNParams m_initFaceEmotionsParams;
        
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
            InitDetectEmotion();
        }

        void IXmgMagicFaceManager.OnXmgUpdateMagicFaces(List<XmgMagicFaceData> faceDataList)
        {
            if (isActiveAndEnabled)
                UpdateFaceData(faceDataList);
        }
        #endregion
        
        #region Tracking
        private void InitDetectEmotion()
        {
#if (!UNITY_EDITOR && UNITY_IOS)
            TextAsset cnn_textAsset = Resources.Load("XZIMG/Models/ENET50-16-EMO") as TextAsset;
#else
            TextAsset cnn_textAsset = Resources.Load("XZIMG/Models/ENET50-32-EMO-CPU") as TextAsset;
#endif
            GCHandle bytesHandleCNNWeights = GCHandle.Alloc(cnn_textAsset.bytes, GCHandleType.Pinned);
            XmgMagicFaceBridge.PrepareInitFaceEmotionsParams(
                ref m_initFaceEmotionsParams,
                bytesHandleCNNWeights.AddrOfPinnedObject());
            int statusFaceEmos = XmgMagicFaceBridge.xzimgMagicFaceInitializeEmotion(ref m_initFaceEmotionsParams);
            if (statusFaceEmos <= 0)
                Debug.LogError("==> (E) --  Face emotions failed!");
            else
                Debug.Log("==> (I) --  Face emotions Initialized!");
            bytesHandleCNNWeights.Free();
        }

        private void UpdateFaceData(List<XmgMagicFaceData> faceDataList)
        {
            for (int o = 0; o < faceDataList.Count; o++)
                XmgMagicFaceBridge.xzimgMagicFaceProcessEmotion(o, 0.5f);

            EmotionReceived?.Invoke(faceDataList.Select(data => data.m_faceData.m_emotions).ToList());
        }
        #endregion
    }
}
