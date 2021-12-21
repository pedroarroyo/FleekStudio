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

using System.Runtime.InteropServices;
using System.Linq;

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

#if (UNITY_ANDROID)
using UnityEngine.Android;
#endif

namespace XZIMG
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class XmgCameraManager: MonoBehaviour
    {
#region Public properties
        /// <summary>
        /// Default Orientation PC/Windows feature
        /// </summary>
        public XmgOrientationMode CaptureDeviceOrientation => m_CaptureDeviceOrientation;

        /// <summary>
        /// Video capture parameters
        /// </summary>
        public XmgVideoCaptureParameters VideoParameters => m_VideoParameters;
        
        /// <summary>
        /// An event which fires each time an camera frame is received.
        /// </summary>
        public event Action<Texture2D, XmgVideoCaptureParameters> FrameReceived;
#endregion

#region Private properties
        /// <summary>
        /// Default Orientation PC/Windows feature
        /// </summary>
        //NB Must be changed to capture orientation mode (portrait vs landscape)
        [SerializeField]
        [Tooltip("Default Orientation PC/Windows feature")]
        private XmgOrientationMode m_CaptureDeviceOrientation = XmgOrientationMode.LandscapeLeft;

        /// <summary>
        /// Video capture parameters
        /// </summary>
        [SerializeField]
        [Tooltip("Video capture parameters")]
        private XmgVideoCaptureParameters m_VideoParameters;

        private ScreenOrientation m_currentScreenOrientation;

        private XmgVideoCaptureOptions m_xmgVideoParams;
        private WebCamTexture m_webcamTexture = null;
        private Color32[] m_data;
        private GCHandle m_PixelsHandle;
        private Texture2D l_texture = null;
        private XmgImage m_image;

        private XmgSegmentationManager m_segmentationManager = null;
        private List<IXmgMagicBehaviour> m_magicBehaviours = new List<IXmgMagicBehaviour>();

#if (!UNITY_EDITOR && UNITY_ANDROID) || (!UNITY_EDITOR && UNITY_IOS)
        private bool m_camera_ready = false;
        private bool m_has_lost_focus = false;
#endif
        
        private float m_deltaTime = 0.0f;
#endregion

#region Life Cycle
        private void Awake()
        {
#if (!UNITY_EDITOR && UNITY_ANDROID)
            m_camera_ready = false;
            // -- Camera permission for Android
            GameObject dialog = null;
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
                dialog = new GameObject();
            }
#endif

            // -- Retrieve all XmgMagicBehaviour    
            var magicbehaviours = FindObjectsOfType<MonoBehaviour>().OfType<IXmgMagicBehaviour>();
            if (magicbehaviours != null)
            {
                foreach (var m in magicbehaviours)
                    m_magicBehaviours.Add(m);
            }

            // -- Get optional Segmentation Manager
            m_segmentationManager = GetComponent<XmgSegmentationManager>();
        }

        private void Start()
        {
            VideoParameters.CheckVideoCaptureParameters();
#if (!UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS))
            if (VideoParameters.UseNativeCapture)
                m_CaptureDeviceOrientation = XmgOrientationMode.LandscapeLeft;
#endif

            foreach (var behaviours in m_magicBehaviours)
                behaviours.OnXmgValidate();

            // -- Launch the tracking engine
            PrepareVideoCapturePlane();
            m_segmentationManager?.Prepare();
            PrepareCamera();
            m_currentScreenOrientation = Screen.orientation;

            // -- Initialize all magic behaviours
            foreach (var behaviours in m_magicBehaviours)
                behaviours.OnXmgInitialize();

            // -- Prepare Video capture and segmentation rendering
            PrepareRendering();

            // -- Prepare Segmentation Rendering
            m_segmentationManager?.PrepareRendering();
        }

        private void Update()
        {
            m_deltaTime += (Time.unscaledDeltaTime - m_deltaTime) * 0.1f;

            // -- Update Camera Scene if screen orientation change
            if (Screen.orientation != m_currentScreenOrientation)
            {
                PrepareCamera();
                m_currentScreenOrientation = Screen.orientation;
            }

            // -- Set Focus
#if (!UNITY_EDITOR && UNITY_ANDROID)
            // -- double tap to start camera focus event
            if (m_camera_ready)
                if (XmgTools.IsDoubleTap())
                    XmgMagicFaceBridge.xzimgCamera_focus();
#endif

            // -- Get Video frame
            if (!VideoParameters.UseNativeCapture)
            {
                if (!GetCameraFrameData())
                    return;
            }
#if (!UNITY_EDITOR && UNITY_ANDROID) || (!UNITY_EDITOR && UNITY_IOS)
            else
            {
                int res = XmgMagicFaceBridge.xzimgCamera_getImage(m_PixelsHandle.AddrOfPinnedObject());
            }
#endif

            // -- Render Magic Face behaviour
            foreach (var behaviours in m_magicBehaviours)
                behaviours.OnXmgRendering(m_image);

            // -- Render Magic Segmentation
            m_segmentationManager?.Render(m_image);

            ApplyCameraFrameTexture();
        }

        private void OnApplicationPaused(bool pauseStatus)
        {
            // Do something here
        }

        private void OnDestroy()
        {
#if (!UNITY_EDITOR && UNITY_ANDROID) || (!UNITY_EDITOR && UNITY_IOS)
            if (m_VideoParameters.UseNativeCapture)
                XmgMagicFaceBridge.xzimgCamera_delete();
#endif
            XmgMagicFaceBridge.xzimgMagicFaceRelease();

            ReleaseCameraFrameData();
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            // -- this is to avoid loosing the video capture when switching apps
#if (!UNITY_EDITOR && UNITY_ANDROID)
            if (m_camera_ready)
            {
                Debug.Log("==> Focus " + hasFocus);
                if (hasFocus == false)
                {
                    XmgMagicFaceBridge.xzimgCamera_delete();
                    m_has_lost_focus = true;
                }
                else if (m_has_lost_focus)
                {
                    XmgMagicFaceBridge.xzimgCamera_create(ref m_xmgVideoParams);
                    m_has_lost_focus = false;
                }
            }
#endif

            // track when losing/recovering focus
            bool handle_focus_loss = false;
            if (handle_focus_loss)
            {
#if (UNITY_STANDALONE || UNITY_EDITOR)
                if (m_webcamTexture != null && hasFocus == false)
                    m_webcamTexture.Stop();     // you can pause as well
                else if (m_webcamTexture != null && hasFocus == true)
                    m_webcamTexture.Play();
#endif
            }
        }
#endregion

#region Rendering
        private void PrepareRendering()
        {
            XmgMagicFaceBridge.PrepareImage(
                ref m_image,
                VideoParameters.GetVideoCaptureWidth(),
                VideoParameters.GetVideoCaptureHeight(),
                m_PixelsHandle.AddrOfPinnedObject());
        }
#endregion

#region Camera
        private void PrepareCamera()
        {
            float arVideo = VideoParameters.GetVideoCaptureWidth() / VideoParameters.GetVideoCaptureHeight();
            float arScreen = VideoParameters.GetScreenAspectRatio();
            float fovy_degree = (float)VideoParameters.CameraVerticalFOV;

            var camera = GetComponent<Camera>();
            // Compute correct focal length according to video capture crops and different available modes
            if (VideoParameters.VideoPlaneFittingMode == XmgVideoPlaneFittingMode.FitScreenHorizontally &&
                (XmgTools.GetRenderOrientation() == XmgOrientationMode.LandscapeLeft || XmgTools.GetRenderOrientation() == XmgOrientationMode.LandscapeRight))
            {
                float fovx = (float)XmgTools.ConvertFov(VideoParameters.CameraVerticalFOV, VideoParameters.GetVideoAspectRatio());
                camera.fieldOfView =
                    (float)XmgTools.ConvertFov(fovx, 1.0f / VideoParameters.GetScreenAspectRatio());
            }
            if (VideoParameters.VideoPlaneFittingMode == XmgVideoPlaneFittingMode.FitScreenVertically &&
                (XmgTools.GetRenderOrientation() == XmgOrientationMode.LandscapeLeft || XmgTools.GetRenderOrientation() == XmgOrientationMode.LandscapeRight))
            {
                //float scaleY = (float)XmgCameraBackground.GetScaleY(VideoParameters);
                camera.fieldOfView = VideoParameters.CameraVerticalFOV;// / scaleY;
            }

            if (VideoParameters.VideoPlaneFittingMode == XmgVideoPlaneFittingMode.FitScreenHorizontally &&
                (XmgTools.GetRenderOrientation() == XmgOrientationMode.Portrait || XmgTools.GetRenderOrientation() == XmgOrientationMode.PortraitUpsideDown))
            {
                camera.fieldOfView =
                    (float)XmgTools.ConvertFov(VideoParameters.CameraVerticalFOV, VideoParameters.GetVideoAspectRatio());
            }

            if (VideoParameters.VideoPlaneFittingMode == XmgVideoPlaneFittingMode.FitScreenVertically &&
                (XmgTools.GetRenderOrientation() == XmgOrientationMode.Portrait || XmgTools.GetRenderOrientation() == XmgOrientationMode.PortraitUpsideDown))
            {
                camera.fieldOfView = (float)XmgTools.ConvertFov(
                    fovy_degree,
                    arVideo,
                    arScreen);
            }

            camera.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            camera.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
        }

        private void PrepareVideoCapturePlane()
        {
            var captureWidth = VideoParameters.GetVideoCaptureWidth();
            var captureHeight = VideoParameters.GetVideoCaptureHeight();

            // -- Prepare video and video plane
            if (!VideoParameters.UseNativeCapture)
            {
                // -- Unity webcam capture
                m_webcamTexture = OpenVideoCapture(ref m_VideoParameters);
                if (m_webcamTexture == null)
                {
                    Debug.LogError("Error - No camera detected!");
                    return;
                }

                // -- Prepare Video texture 
                m_data = new Color32[captureWidth * captureHeight];
                m_PixelsHandle = GCHandle.Alloc(m_data, GCHandleType.Pinned);
                l_texture = new Texture2D(captureWidth, captureHeight, TextureFormat.RGBA32, false);
            }
            else
            {
                // -- Native camera capture using xzimgCamera
                XmgMagicFaceBridge.PrepareNativeVideoCaptureDefault(
                    ref m_xmgVideoParams,
                    VideoParameters.VideoCaptureMode,
                    VideoParameters.UseFrontal ? 1 : 0);

                // -- Prepare Video texture 
                m_data = new Color32[captureWidth * captureHeight];
                m_PixelsHandle = GCHandle.Alloc(m_data, GCHandleType.Pinned);
                l_texture = new Texture2D(captureWidth, captureHeight, TextureFormat.RGBA32, false);

#if (!UNITY_EDITOR && UNITY_ANDROID) || (!UNITY_EDITOR && UNITY_IOS)
                XmgMagicFaceBridge.xzimgCamera_create(ref m_xmgVideoParams);
                m_camera_ready = true;
#endif
            }
        }

        private WebCamTexture OpenVideoCapture(ref XmgVideoCaptureParameters videoParameters)
        {
            string deviceName;
            var captureWidth = videoParameters.GetVideoCaptureWidth();
            var captureHeight = videoParameters.GetVideoCaptureHeight();

            var camera = GetComponent<Camera>();
            // Reset
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.transform.position = new Vector3(0, 0, 0);
            camera.transform.eulerAngles = new Vector3(0, 0, 0);
            transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

            Debug.Log("webcam names:");
            for (int cameraIndex = 0; cameraIndex < WebCamTexture.devices.Length; cameraIndex++)
            {
                Debug.Log(WebCamTexture.devices[cameraIndex].name);
            }

            if (videoParameters.VideoCaptureIndex == -1)
            {
                for (int cameraIndex = 0; cameraIndex < WebCamTexture.devices.Length; cameraIndex++)
                {
                    // We want the back camera
                    if (!WebCamTexture.devices[cameraIndex].isFrontFacing && !videoParameters.UseFrontal)
                    {
                        deviceName = WebCamTexture.devices[cameraIndex].name;
                        m_webcamTexture = new WebCamTexture(deviceName, captureWidth, captureHeight, 30);
                        break;
                    }
                    else if (WebCamTexture.devices[cameraIndex].isFrontFacing && videoParameters.UseFrontal)
                    {
                        deviceName = WebCamTexture.devices[cameraIndex].name;
                        m_webcamTexture = new WebCamTexture(deviceName, captureWidth, captureHeight, 30);
                        break;
                    }
                }
            }
            else
            {
                deviceName = WebCamTexture.devices[videoParameters.VideoCaptureIndex].name;
                // deviceName = "device #0";
                m_webcamTexture = new WebCamTexture(deviceName, captureWidth, captureHeight, 30);
            }
            if (!m_webcamTexture)   // try with the first idx
            {
                if (!videoParameters.UseFrontal || WebCamTexture.devices.Length == 1)
                    deviceName = WebCamTexture.devices[0].name;
                else
                    deviceName = WebCamTexture.devices[1].name;
                m_webcamTexture = new WebCamTexture(deviceName, captureWidth, captureHeight, 30);
            }


            if (!m_webcamTexture)
                Debug.LogError("No camera detected!");
            else
            {
                if (m_webcamTexture.isPlaying)
                    m_webcamTexture.Stop();

                m_webcamTexture.Play();     // It's here where width and height is usually modified to correct image resolution


                if (m_webcamTexture.width != m_webcamTexture.requestedWidth &&
                    m_webcamTexture.requestedWidth > 100 && m_webcamTexture.width > 100)
                {
                    Debug.LogError("==> (W) An issue is detected with required video capture mode, changing to a more appropriate mode");
                    Debug.LogError("requested width x height: " + m_webcamTexture.requestedWidth + m_webcamTexture.requestedHeight);
                    Debug.LogError("effective width x height: " + m_webcamTexture.width + m_webcamTexture.height);
                    videoParameters.VideoCaptureMode = XmgVideoCaptureParameters.GetVideoCaptureMode(m_webcamTexture.width, m_webcamTexture.height);
                }
            }
            return m_webcamTexture;
        }

        public void SwitchCamera()
        {
#if (!UNITY_EDITOR && UNITY_ANDROID) || (!UNITY_EDITOR && UNITY_IOS)
            XmgMagicFaceBridge.xzimgMagicFacePause(1);

            int front = 1 - m_xmgVideoParams.m_frontal;
            XmgMagicFaceBridge.xzimgCamera_delete();
            m_xmgVideoParams.m_frontal = front;
            m_VideoParameters.MirrorVideo = front==0?false:true;
            m_VideoParameters.UseFrontal = front==0?false:true;    
            XmgMagicFaceBridge.xzimgCamera_create(ref m_xmgVideoParams);
            
            XmgMagicFaceBridge.xzimgMagicFacePause(0);
#endif
        }

        private bool GetCameraFrameData()
        {
            if (m_webcamTexture)
            {
                // don't change - sequenced to avoid crash
                if (m_webcamTexture.didUpdateThisFrame)
                {
                    m_webcamTexture.GetPixels32(m_data);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        
        private void SetCameraFrameData(byte[] frame)
        {
            if (frame != null)
            {
                l_texture.LoadRawTextureData(frame);
                l_texture.Apply();
                FrameReceived?.Invoke(l_texture, VideoParameters);
            }
        }
        
        private void ReleaseCameraFrameData()
        {
            m_PixelsHandle.Free();
            m_webcamTexture.Stop();
        }
        
        private void ApplyCameraFrameTexture()
        {
            // don't change - sequenced to avoid crash
            l_texture.SetPixels32(m_data);
            l_texture.Apply();
            FrameReceived?.Invoke(l_texture, VideoParameters);
        }
#endregion
    }
}
