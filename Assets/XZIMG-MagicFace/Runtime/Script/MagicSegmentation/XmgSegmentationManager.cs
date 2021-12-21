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

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;

namespace XZIMG
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(XmgCameraManager))]
    public sealed class XmgSegmentationManager: MonoBehaviour
    {
        #region Public properties
        /// <summary>
        /// Select segmentation mode
        /// </summary>
        public XmgSegmentationMode SegmentationMode {
            get { return m_SegmentationMode; }
            set { m_SegmentationMode = value; }
        }

        /// <summary>
        /// Segmentation Parameters
        /// </summary>
        public XmgInitCNNParams SegmentationParameters => m_SegmentationParameters;

        public Texture2D SegmentationBackgroundTexture
        {
            get { return m_SegmentationBackgroundTexture; }
            set {
                m_SegmentationBackgroundTexture = value;
                m_SegmentationBackgroundTextureUpdated = true;
            }
        }

        public bool SegmentationBackgroundTextureUpdated
        {
            get => m_SegmentationBackgroundTextureUpdated;
            set => m_SegmentationBackgroundTextureUpdated = value;
        }

        /// <summary>
        /// An event which fires each time an occlusion frame is received.
        /// </summary>
        public event Action<Texture2D> FrameReceived;
        #endregion

        #region Private properties
        /// <summary>
        /// Select segmentation mode
        /// </summary>
        [SerializeField]
        [Tooltip("Select segmentation mode")]
        private XmgSegmentationMode m_SegmentationMode = XmgSegmentationMode.Disabled;

        /// <summary>
        /// Segmentation Parameters
        /// </summary>
        [SerializeField]
        [Tooltip("Segmentation Parameters")]
        private XmgInitCNNParams m_SegmentationParameters;

        [SerializeField]
        [Tooltip("Select segmentation background texture")]
        private Texture2D m_SegmentationBackgroundTexture = null;
        private Texture2D m_prevSegmentationBackgroundTexture = null;
        private bool m_SegmentationBackgroundTextureUpdated = false;

        private XmgCameraManager m_cameraManager = null;
        private byte[] m_segmentationBytes;
        private GCHandle m_segBytesHandle;
        private XmgImage m_segmentationImage;
        private Texture2D m_segTexture_gray = null;
        #endregion

        public void ImChanged()
        {
            Debug.Log("I have changed to");
        }

        #region Life cycle
        private void Awake()
        {
            // -- Get Camera Manager
            m_cameraManager = GetComponent<XmgCameraManager>();
#if (UNITY_WEBGL)
            if (m_SegmentationMode != XmgSegmentationMode.Disabled)
            {
                m_SegmentationMode = XmgSegmentationMode.Disabled;
                Debug.LogWarning("==> (W) Segmentation engines are not working on WEBGL at current time");
            }
#endif
        }

        private void OnDisable()
        {
            if (!IsSegmentationActive()) return;

            // -- Reset all pixels color to transparent
            byte alpha = SegmentationMode == XmgSegmentationMode.HairSegmentation ? byte.MinValue : byte.MaxValue;
            Color32 resetColor = new Color32(0, 0, 0, alpha);
            Color32[] resetColorArray = m_segTexture_gray.GetPixels32();

            for (int i = 0; i < resetColorArray.Length; i++)
            {
                resetColorArray[i] = resetColor;
            }

            m_segTexture_gray.SetPixels32(resetColorArray);
            m_segTexture_gray.Apply();

            FrameReceived?.Invoke(m_segTexture_gray);
        }

        private void OnDestroy()
        {
            XmgMagicFaceBridge.xzimgMagicSegmentationRelease();
            if (m_segBytesHandle.IsAllocated)
            {
                m_segBytesHandle.Free();
            }
        }
        #endregion

        #region Segmentation
        internal void Prepare()
        {
            if (!IsSegmentationActive())
                return;

            // -- Initialize the segmentation module
            GCHandle bytesHandleCNNWeights;
            String strAsset = XmgMagicFaceBridge.GetSegmentationAssetName(
                m_SegmentationMode,
                m_cameraManager.CaptureDeviceOrientation);

            if (strAsset != "") {
                TextAsset cnn_textAsset = Resources.Load(strAsset) as TextAsset;
                if (cnn_textAsset == null)
                    Debug.LogError("==> (E) resource (cnn_textAsset) is not found");
                bytesHandleCNNWeights = GCHandle.Alloc(cnn_textAsset.bytes, GCHandleType.Pinned);
                m_SegmentationParameters.m_cnnWeightsSize = cnn_textAsset.bytes.Length;
            }

            XmgMagicFaceBridge.PrepareInitSegmentationParams(
                SegmentationMode,
                ref m_SegmentationParameters,
                strAsset != "" ? bytesHandleCNNWeights.AddrOfPinnedObject() : System.IntPtr.Zero,
                m_cameraManager.CaptureDeviceOrientation);
            int statusSeg = XmgMagicFaceBridge.xzimgMagicSegmentationInitialize(ref m_SegmentationParameters);
            if (statusSeg <= 0)
                Debug.LogError("==> (E) -- Segmentation engine initialization failed with error:" + statusSeg);
            else
                Debug.Log("==> (I) -- Segmentation engine Initialized!");

            if (strAsset != "")
                bytesHandleCNNWeights.Free();
        }

        internal void PrepareRendering()
        {
            if (!IsSegmentationActive()) return;

            // -- Get segmentation image output size which AR should match the rendering of the video capture
            int segOutWidth = SegmentationParameters.m_inWidth;
            int segOutHeight = SegmentationParameters.m_inHeight;
            int idx_rotation = (int)XmgTools.GetDeviceCurrentOrientation(
                (int)m_cameraManager.CaptureDeviceOrientation, 
                m_cameraManager.VideoParameters.UseFrontal);

            if (idx_rotation == 1 || idx_rotation == 3)
            {
                segOutWidth = SegmentationParameters.m_inHeight;
                segOutHeight = SegmentationParameters.m_inWidth;
            }
            segOutWidth*=2;
            segOutHeight*=2;

            // Debug.Log("rot: " + idx_rotation);
            // Debug.Log("width: " + SegmentationParameters.m_inWidth);
            // Debug.Log("height: " + SegmentationParameters.m_inHeight);
            // Debug.Log("out width: " + segOutWidth);
            // Debug.Log("out height: " + segOutHeight);

            m_segmentationBytes = new byte[segOutWidth * segOutHeight];
            m_segBytesHandle = GCHandle.Alloc(m_segmentationBytes, GCHandleType.Pinned);
            XmgMagicFaceBridge.PrepareGrayImage(
                ref m_segmentationImage,
                segOutWidth, segOutHeight,
                m_segBytesHandle.AddrOfPinnedObject());

            // -- Prepare the textured plan to display results
#if (UNITY_ANDROID && UNITY_IOS)
            m_segTexture_gray = new Texture2D(
                segOutHeight, segOutWidth,
                TextureFormat.Alpha8,
                false, true);
#else
            m_segTexture_gray = new Texture2D(
                segOutWidth, segOutHeight,
                TextureFormat.Alpha8,
                false, true);
#endif
            m_segTexture_gray.wrapMode = TextureWrapMode.Clamp;
            m_segTexture_gray.filterMode = FilterMode.Bilinear;
        }

        internal void Render(XmgImage videoFrame)
        {
            if (m_prevSegmentationBackgroundTexture != m_SegmentationBackgroundTexture)
            {
                m_prevSegmentationBackgroundTexture = m_SegmentationBackgroundTexture;
                m_SegmentationBackgroundTextureUpdated = true;
            }

            // -- Segmentation engine
            if (IsSegmentationActive())
            {
                int idx_rotation = (int)XmgTools.GetDeviceCurrentOrientation(
                    (int)m_cameraManager.CaptureDeviceOrientation,
                    m_cameraManager.VideoParameters.UseFrontal);

                // -- Process segmentation
                /*int res_process_seg = */
                XmgMagicFaceBridge.xzimgMagicSegmentationProcess(ref videoFrame, idx_rotation);
                
                // -- Get Segmentation data
                int res_get_segimage =
                    XmgMagicFaceBridge.xzimgMagicSegmentationGetSegmentationImage(ref m_segmentationImage, idx_rotation, 0);

                m_segTexture_gray.LoadRawTextureData(m_segmentationBytes);
                m_segTexture_gray.Apply();

                FrameReceived?.Invoke(m_segTexture_gray);
            }
        }

        /// <summary>
        /// Is segmentation mode active ( != XmgSegmentationMode.Disabled )
        /// </summary>
        /// <returns>Is segmentation mode active</returns>
        public bool IsSegmentationActive()
        {
            return (SegmentationMode != XmgSegmentationMode.Disabled && isActiveAndEnabled);
        }
        #endregion

    }
}
