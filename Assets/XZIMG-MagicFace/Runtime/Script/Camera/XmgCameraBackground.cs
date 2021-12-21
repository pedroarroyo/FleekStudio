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

namespace XZIMG
{
    [RequireComponent(typeof(XmgCameraManager))]
    public sealed class XmgCameraBackground : MonoBehaviour
    {
        #region Public properties
        /// <summary>
        /// Current material used for background rendering
        /// </summary>
        public Material Material => (UseCustomMaterial && CustomMaterial != null) ? CustomMaterial : m_DefaultMaterial;

        /// <summary>
        /// Use custom material for rendering background
        /// </summary>
        public bool UseCustomMaterial { get => m_UseCustomMaterial; set => m_UseCustomMaterial = value; }

        /// <summary>
        /// Custom material for rendering background
        /// </summary>
        public Material CustomMaterial { get => m_CustomMaterial; set => m_CustomMaterial = value; }
        #endregion

        #region Private properties
        /// <summary>
        /// Default material used for background rendering
        /// </summary>
        private Material m_DefaultMaterial
        {
            get
            {
                Material material;
                if (m_SegmentationManager != null && 
                    m_SegmentationManager.SegmentationMode == XmgSegmentationMode.BodySegmentation)
                    material = m_DefaultMaterialBodySegmentation.Value;
#if (UNITY_IOS)
                else if (m_SegmentationManager && 
                    m_SegmentationManager.SegmentationMode == XmgSegmentationMode.BodySegmentationRobust)
                    material = m_DefaultMaterialBodySegmentation.Value;
#endif
                else if (m_SegmentationManager != null && m_SegmentationManager.SegmentationMode == XmgSegmentationMode.HairSegmentation)
                    material = m_DefaultMaterialHairSegmentation.Value;
                else // Shader for face tracking only
                    material = m_DefaultMaterialNoSegmentation.Value;

                return material;
            }
        }

        /// <summary>
        /// Default material when no segmentation are used
        /// </summary>
        private Lazy<Material> m_DefaultMaterialNoSegmentation = new Lazy<Material>(() => new Material(Shader.Find("XZIMG/VideoShader")));

        /// <summary>
        /// Default material when no segmentation are used
        /// </summary>
        private Lazy<Material> m_DefaultMaterialHairSegmentation = new Lazy<Material>(() => new Material(Shader.Find("XZIMG/VideoShaderHairDying")));

        /// <summary>
        /// Default material when no segmentation are used
        /// </summary>
        private Lazy<Material> m_DefaultMaterialBodySegmentation = new Lazy<Material>(() => new Material(Shader.Find("XZIMG/VideoShaderBodySegmentation")));

        /// <summary>
        /// Use custom material for rendering background
        /// </summary>
        [SerializeField]
        private bool m_UseCustomMaterial = false;

        /// <summary>
        /// Custom material for rendering background
        /// </summary>
        [SerializeField]
        private Material m_CustomMaterial = null;

        /// <summary>
        /// Renderer used for camera background rendering
        /// </summary>
        private Renderer m_CameraBackgroundRenderer = null;

        /// <summary>
        /// The camera manager from which frame information is pulled.
        /// </summary>
        private XmgCameraManager m_CameraManager = null;

        /// <summary>
        /// The occlusion manager, which may not exist, from which occlusion information is pulled.
        /// </summary>
        private XmgSegmentationManager m_SegmentationManager = null;
#endregion

        private void Awake()
        {
            // -- Get optional Segmentation Manager
            m_CameraManager = GetComponent<XmgCameraManager>();
            m_SegmentationManager = GetComponent<XmgSegmentationManager>();

            CreateVideoCapturePlane();
        }

        void OnEnable()
        {
            // Ensure that background rendering is disabled until the first camera frame is received.
            m_CameraManager.FrameReceived += OnCameraFrameReceived;
            if (m_SegmentationManager != null)
            {
                m_SegmentationManager.FrameReceived += OnOcclusionFrameReceived;
                if (m_SegmentationManager.SegmentationBackgroundTexture != null)
                {
                    Material.SetTexture("_MainTex3", m_SegmentationManager.SegmentationBackgroundTexture);
                    m_SegmentationManager.SegmentationBackgroundTextureUpdated = false;
                }                    
            }
            m_CameraBackgroundRenderer.enabled = true;
        }

        void OnDisable()
        {
            if (m_SegmentationManager != null)
            {
                m_SegmentationManager.FrameReceived -= OnOcclusionFrameReceived;
            }
            m_CameraManager.FrameReceived -= OnCameraFrameReceived;

            m_CameraBackgroundRenderer.enabled = false;
        }

        private void CreateVideoCapturePlane()
        {
            // -- Fetch or create Video Plane
            var childrenRederers = GetComponentsInChildren<Renderer>();

            m_CameraBackgroundRenderer = childrenRederers.FirstOrDefault(r => r.name == "XmgCameraBackgroundMesh");
            if (m_CameraBackgroundRenderer == null)
            {
                var rendererGO = new GameObject("XmgCameraBackgroundMesh");

                rendererGO.transform.SetParent(transform);
                rendererGO.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                rendererGO.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                rendererGO.transform.position = new Vector3(0.0f, 0.0f, 1.0f);

                // -- Create mesh (plane)
                Mesh mesh = new Mesh();
                mesh.vertices = new Vector3[]
                {
                    new Vector3(-1, 1, 0),
                    new Vector3(1, 1, 0),
                    new Vector3(1, -1, 0),
                    new Vector3(-1, -1, 0)
                };
                mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
                mesh.uv = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0) };
                rendererGO.AddComponent<MeshFilter>().mesh = mesh;

                m_CameraBackgroundRenderer = rendererGO.GetComponent<Renderer>();

                if (!m_CameraBackgroundRenderer)
                    m_CameraBackgroundRenderer = rendererGO.AddComponent<MeshRenderer>();
            }
        }

        static public float GetScaleX(XmgVideoCaptureParameters videoParameters)
        {
            int CaptureWidth = videoParameters.GetVideoCaptureWidth();
            int CaptureHeight = videoParameters.GetVideoCaptureHeight();

            float arVideo = (float)CaptureWidth / (float)CaptureHeight;
            float arScreen = (float)Screen.width / (float)Screen.height;

#if ((UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR && !UNITY_STANDALONE)
		if (Screen.orientation == ScreenOrientation.Portrait ||
            Screen.orientation == ScreenOrientation.PortraitUpsideDown)
		    arScreen =  (float)Screen.height / (float)Screen.width;
#endif
            if (Math.Abs(arVideo - arScreen) > 0.001f && videoParameters.VideoPlaneFittingMode == XmgVideoPlaneFittingMode.FitScreenVertically)
                return arVideo / arScreen;
            else
                return 1.0f;
        }

        static public float GetScaleY(XmgVideoCaptureParameters videoParameters)
        {
            int CaptureWidth = videoParameters.GetVideoCaptureWidth();
            int CaptureHeight = videoParameters.GetVideoCaptureHeight();

            float arVideo = (float)CaptureWidth / (float)CaptureHeight;
            float arScreen = (float)Screen.width / (float)Screen.height;

#if ((UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR && !UNITY_STANDALONE)
		if (Screen.orientation == ScreenOrientation.Portrait ||
            Screen.orientation == ScreenOrientation.PortraitUpsideDown)
		    arScreen =  (float)Screen.height / (float)Screen.width;
#endif
            if (Math.Abs(arVideo - arScreen) > 0.001f && videoParameters.VideoPlaneFittingMode == XmgVideoPlaneFittingMode.FitScreenHorizontally)
                return arScreen / arVideo;
            else
                return 1.0f;
        }

        internal void OnCameraFrameReceived(Texture2D cameraFrame, XmgVideoCaptureParameters videoParameters)
        {
            if (m_CameraBackgroundRenderer.material != Material)
                m_CameraBackgroundRenderer.material = Material;

         var orientation = XmgTools.GetVideoOrientation(videoParameters.UseNativeCapture, videoParameters.UseFrontal);

            #if ((UNITY_IOS) && !UNITY_EDITOR && !UNITY_STANDALONE)
            if (orientation == XmgOrientationMode.LandscapeLeft)
                orientation = XmgOrientationMode.LandscapeRight;
            else if (orientation == XmgOrientationMode.LandscapeRight)
                orientation = XmgOrientationMode.LandscapeLeft;
            #endif

            // shader parameters
            Material.SetInt("_Rotation",
                (int)orientation);
            Material.SetFloat("_ScaleX",
                (float)GetScaleX(videoParameters) * videoParameters.VideoPlaneScale);
            Material.SetFloat("_ScaleY",
                (float)GetScaleY(videoParameters) * videoParameters.VideoPlaneScale);
            Material.SetInt("_Mirror",
                (int)(videoParameters.MirrorVideo == true ? 1 : 0));
            Material.SetInt("_VerticalMirror",
                (int)((videoParameters.GetVerticalMirror() == true) ? 1 : 0));
#if (!UNITY_EDITOR && UNITY_IOS)
            // Native images on iOS are BGRA
            Material.SetInt("_InvertTextureChannels", 1);
#endif

            Material.SetTexture("_MainTex1", cameraFrame);
        }
        
        private void OnOcclusionFrameReceived(Texture2D segmentationTexture)
        {
            Material.SetTexture("_MainTex2", segmentationTexture);
            Material.SetInt("_ActivateSegmentation", 1);
            if (m_SegmentationManager.SegmentationBackgroundTextureUpdated)
            {
                Material.SetTexture("_MainTex3", m_SegmentationManager.SegmentationBackgroundTexture);
                m_SegmentationManager.SegmentationBackgroundTextureUpdated = false;
            }
        }
    }
}