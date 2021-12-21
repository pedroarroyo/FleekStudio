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
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEditor;

namespace XZIMG
{
    [DisallowMultipleComponent]
    public sealed class XmgMagicFaceTracking : MonoBehaviour, IXmgMagicBehaviour
    {
        #region Public properties
        /// <summary>
        /// The Camera to associate with the AR device..
        /// </summary>
        public Camera Camera => m_Camera;
        
        /// <summary>
        /// Display Action Units as Sliders
        /// </summary>
        public bool TrackEyesPositions => m_trackEyesPositions;

        /// <summary>
        /// Display Action Units as Sliders
        /// </summary>
        public XmgFaceFeaturesMode FaceFeaturesMode => m_faceFeaturesMode;

        /// <summary>
        /// Number of face detected simultaneously
        /// </summary>
        public int MaximumFaceCount => m_maximumFaceCount;
        
        public void UpdateFaceManagerList()
        {
            m_faceManagers = GetComponents<MonoBehaviour>()
            .Where(c => c is IXmgMagicFaceManager)
            .Select(c => c as IXmgMagicFaceManager)
            .ToArray();
            if (m_faceManagers != null)
            {
                foreach (var manager in m_faceManagers)
                {
                    manager.OnXmgInitialize();
                }
            }
            else
                Debug.LogWarning("XmgMagicFaceTracking: No IXmgMagicFaceManager component detected.");
        }
        #endregion

        #region Internal properties
        /// <summary>
        /// AR camera manager
        /// </summary>
        internal XmgCameraManager CameraManager => Camera.GetComponent<XmgCameraManager>();
        #endregion

        #region Private properties
        /// <summary>
        /// The Camera to associate with the AR device..
        /// </summary>
        [SerializeField]
        [Tooltip("The Camera to associate with the AR device.")]
        private Camera m_Camera = null;
        
        /// <summary>
        /// Number of face detected simultaneously
        /// </summary>
        [SerializeField]
        [Tooltip("Number of face detected simultaneously")]
        private int m_maximumFaceCount = 1;

        /// <summary>
        /// Compute eyes positions (won't work with robust face tracking)
        /// </summary>
        [SerializeField]
        [Tooltip("Compute eyes positions (available only if face features mode == All face features)")]
        private bool m_trackEyesPositions = false;

        /// <summary>
        /// Select which features are being detected (All, internal)
        /// </summary>
        [SerializeField]
        [Tooltip("Select which face features are being detected (All, Internal)")]
        private XmgFaceFeaturesMode m_faceFeaturesMode = XmgFaceFeaturesMode.AllFaceFeatures;

#if (UNITY_IOS || UNITY_EDITOR_OSX)
        /// <summary>
        /// Select which features are being detected (All, internal)
        /// </summary>
        [SerializeField]
        [Tooltip("iOS (iphone 8+/iPads) feature to refine further the mouth postion")]
        private bool m_refineMouthContours = false;
#endif

        /// <summary>
        /// Detected faces data.
        /// </summary>
        private List<XmgMagicFaceData> m_faceDataList = new List<XmgMagicFaceData>();
        
        /// <summary>
        /// Face tracking init parameters
        /// </summary>
        private XmgInitParams m_initializationParams;

        /// <summary>
        /// List of all face managers attached to this gameobject.
        /// </summary>
        private IXmgMagicFaceManager[] m_faceManagers;
        
        #endregion

        #region IXmgMagicBehaviour
        void IXmgMagicBehaviour.OnXmgValidate()
        {
        }

        void IXmgMagicBehaviour.OnXmgInitialize()
        {            
            InitFaceTracking();

            m_faceManagers = GetComponents<MonoBehaviour>()
                .Where( c => c is IXmgMagicFaceManager)
                .Select( c => c as IXmgMagicFaceManager)
                .ToArray();
            if (m_faceManagers != null)
            {
                foreach (var manager in m_faceManagers)
                {
                    manager.OnXmgInitialize();
                }
            }
            else
                Debug.LogWarning("XmgMagicFaceTracking: No IXmgMagicFaceManager component detected.");
        }

        void IXmgMagicBehaviour.OnXmgRendering(XmgImage videoFrame)
        {
            UpdateFaceTracking(videoFrame);
        }
        #endregion

        #region Tracking
        private void InitFaceTracking()
        {
            // -- Check if segmentation is activated
            var segmentation = FindObjectOfType<XmgSegmentationManager>();
            if (segmentation.SegmentationMode != XmgSegmentationMode.Disabled)
                Debug.LogWarning("==> Simultaneous face tracking and segmentation engines should be activated on modern devices only!");
        
            if (m_trackEyesPositions == true && m_faceFeaturesMode == XmgFaceFeaturesMode.AllFaceFeaturesRobust)
                Debug.LogWarning("==> Calculation of eyes positions won't process wiht XmgFaceFeaturesMode.AllFaceFeaturesRobust!");


            // -- Init face data tracking
            TextAsset textAsset = Resources.Load("XZIMG/Models/models-68-BS") as TextAsset;

            // if (m_faceFeaturesMode == XmgFaceFeaturesMode.InternalFaceFeatures)
            //    textAsset = Resources.Load("XZIMG/Models/models-51-BS") as TextAsset;
#if (!UNITY_IOS)
            if (m_faceFeaturesMode == XmgFaceFeaturesMode.AllFaceFeaturesRobust)
                textAsset = Resources.Load("XZIMG/Models/models-68-robust-BS") as TextAsset;
            else 
                textAsset = Resources.Load("XZIMG/Models/models-68-BS") as TextAsset;
#endif
            if (textAsset == null)
            {
                Debug.LogError("==> (E) -- Can't find classifier asset!");
                return;
            }
            GCHandle bytesHandleRegressor = GCHandle.Alloc(textAsset.bytes, GCHandleType.Pinned);

            bool detect3DFacialFeatures = true;
            int nbFaceFeatures = 68;
            bool robustModel = false;
#if (UNITY_IOS) || (UNITY_EDITOR_OSX)
            if (m_faceFeaturesMode == XmgFaceFeaturesMode.AllFaceFeaturesRobust)
                robustModel = true;
#endif
            XmgMagicFaceBridge.PrepareMagicFaceInitParams(
                ref m_initializationParams,
                detect3DFacialFeatures,
                CameraManager.VideoParameters.GetProcessingWidth(),
                CameraManager.VideoParameters.GetProcessingHeight(),
                nbFaceFeatures,
                MaximumFaceCount,
                CameraManager.VideoParameters.CameraVerticalFOV,
                m_trackEyesPositions ? 1 : 0,
                robustModel);
            m_initializationParams.m_faceClassifier = bytesHandleRegressor.AddrOfPinnedObject();

            int status = XmgMagicFaceBridge.xzimgMagicFaceInitialize(ref m_initializationParams);
#if (UNITY_IOS) || (UNITY_EDITOR_OSX)
            if (m_refineMouthContours)
                XmgMagicFaceBridge.xzimgActivateMouthRefiner(1);
#endif
            if (status <= 0) Debug.LogError("==> (E) -- Initialization failed!");
            bytesHandleRegressor.Free();

            // -- Init face data list 
            m_faceDataList.Clear();
            for (int i = 0; i < MaximumFaceCount; i++)
                m_faceDataList.Add(new XmgMagicFaceData());
        }
        
        private void UpdateFaceTracking(XmgImage videoFrame)
        {
            // -- Calling the tracking
            XmgMagicFaceBridge.xzimgMagicFaceTrackNonRigidFaces(
                ref videoFrame,
                (int)XmgTools.GetDeviceCurrentOrientation(
                    (int)CameraManager.CaptureDeviceOrientation, CameraManager.VideoParameters.UseFrontal));

            // -- Get Face tracking data
            for (int i = 0; i < m_faceDataList.Count; i++)
                XmgMagicFaceBridge.xzimgMagicFaceGetFaceData(i, ref m_faceDataList[i].m_faceData);

            // -- Fire update event
            if (m_faceManagers != null)
            {
                foreach (var manager in m_faceManagers)
                {
                    manager.OnXmgUpdateMagicFaces(m_faceDataList);
                }
            }
        }
#endregion
    }
}