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

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XZIMG
{
    public enum XmgVideoPlaneFittingMode
    {
        FitScreenHorizontally,
        FitScreenVertically,
    };

    public enum XmgVideoCaptureModes
    {
        Video640x480 = 1,
        Video720p = 2,
        Video1080p = 3
    };

    [System.Serializable]
    public class XmgVideoCaptureParameters
    {
        #region Public properties
        /// <summary>
        /// Use Native Capture or Unity WebCameraTexture class - Should be activated for mobiles
        /// </summary>
        public bool UseNativeCapture { get => m_UseNativeCapture; internal set => m_UseNativeCapture = value; }

        /// <summary>
        /// Video device index
        /// -1 for automatic research
        /// </summary>
        public int VideoCaptureIndex { get => m_VideoCaptureIndex; internal set => m_VideoCaptureIndex = value; }

        /// <summary>
        /// Video capture mode 
        /// 1 is VGA
        /// 2 is 720p
        /// 3 is 1080p
        /// </summary>
        public int VideoCaptureMode { get => (int)m_VideoCaptureMode; internal set => m_VideoCaptureMode = (XmgVideoCaptureModes)value; }

        /// <summary>
        /// Use frontal camera (for mobiles only)
        /// </summary>
        public bool UseFrontal { get => m_UseFrontal; internal set => m_UseFrontal = value; }

        /// <summary>
        /// Mirror the video
        /// </summary>
        public bool MirrorVideo { get => m_MirrorVideo; internal set => m_MirrorVideo = value; }

        /// <summary>
        /// Choose if the video plane should fit  horizontally or vertically the screen
        /// (only relevent in case screen aspect ratio is different from video capture aspect ratio)
        /// </summary>
        public XmgVideoPlaneFittingMode VideoPlaneFittingMode { get => m_VideoPlaneFittingMode; internal set => m_VideoPlaneFittingMode = value; }

        /// <summary>
        /// To scale up/down the rendering plane
        /// </summary>
        public float VideoPlaneScale { get => m_VideoPlaneScale; internal set => m_VideoPlaneScale = value; }

        /// <summary>
        /// Camera vertical FOV
        /// This value will change the main camera vertical FOV
        /// </summary>
        public float CameraVerticalFOV { get => m_CameraVerticalFOV; internal set => m_CameraVerticalFOV = value; }
        #endregion

        #region Private properties
        /// <summary>
        /// Use Native Capture or Unity WebCameraTexture class - Should be activated for mobiles
        /// </summary>
        [SerializeField]
        [Tooltip("Use Native Capture or Unity WebCameraTexture class - Should be activated for mobiles")]
        private bool m_UseNativeCapture = true;

        /// <summary>
        /// Video device index
        /// -1 for automatic research
        /// </summary>
        [SerializeField]
        [Tooltip("Video device index \n -1 for automatic research")]
        private int m_VideoCaptureIndex = -1;

        /// <summary>
        /// Video capture mode
        /// </summary>
        [SerializeField]
        [Tooltip("Video capture mode")]
        private XmgVideoCaptureModes m_VideoCaptureMode = XmgVideoCaptureModes.Video720p;

        /// <summary>
        /// Use frontal camera (for mobiles only)
        /// </summary>
        [SerializeField]
        [Tooltip("Use frontal camera (for mobiles only)")]
        private bool m_UseFrontal = false;

        /// <summary>
        /// Mirror the video
        /// </summary>
        [SerializeField]
        [Tooltip("Mirror the video")]
        private bool m_MirrorVideo = false;

        /// <summary>
        /// Choose if the video plane should fit  horizontally or vertically the screen
        /// (only relevent in case screen aspect ratio is different from video capture aspect ratio)
        /// </summary>
        [SerializeField]
        [Tooltip("Choose if the video plane should fit  horizontally or vertically the screen (only relevent in case screen aspect ratio is different from video capture aspect ratio)")]
        private XmgVideoPlaneFittingMode m_VideoPlaneFittingMode = XmgVideoPlaneFittingMode.FitScreenHorizontally;

        /// <summary>
        /// To scale up/down the rendering plane
        /// </summary>
        //[SerializeField]
        //[Tooltip("To scale up/down the rendering plane")]
        private float m_VideoPlaneScale = 1.0f;

        /// <summary>
        /// Camera vertical FOV
        /// This value will change the main camera vertical FOV
        /// </summary>
        [SerializeField]
        [Tooltip("Camera vertical FOV \nThis value will change the main camera vertical FOV")]
        private float m_CameraVerticalFOV = 50f;

        /// <summary>
        /// Image is flipped upside down (depending on pixel formats and devices)
        /// </summary>
        private bool m_IsVideoVerticallyFlipped = false;
        #endregion

        internal void CheckVideoCaptureParameters()
        {
#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL)
            if (UseNativeCapture)
                Debug.LogWarning("XmgVideoCaptureParameters (UseNativeCapture) - Video Capture cannot be set to native for PC/MAC platforms => forcing to FALSE");
            if (UseFrontal)
                Debug.LogWarning("XmgVideoCaptureParameters (UseFrontal) - Frontal mode option is not available for PC/MAC platforms - Use camera index edit box instead => forcing to FALSE");
            UseNativeCapture = false;
            UseFrontal = false;     // this has to be removed and normalized with the rest
#endif

#if (!UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS))
            // useNativeCapture = true;
            if (UseFrontal && !MirrorVideo)
            {
                MirrorVideo = true;
                Debug.LogWarning("XmgVideoCaptureParameters (MirrorVideo) - Mirror mode is forced on mobiles when using frontal camera => forcing to TRUE");       
            }
            if (!UseFrontal && MirrorVideo)
            {
                MirrorVideo = false;
                Debug.LogWarning("XmgVideoCaptureParameters (MirrorVideo) - Mirror mode is deactivate on mobiles when using back camera => forcing to FALSE");       
            }
#endif

#if (!UNITY_EDITOR && UNITY_ANDROID) || (!UNITY_EDITOR && UNITY_IOS)
            if (UseNativeCapture)
                m_IsVideoVerticallyFlipped = true;
#endif
            // -- Manage video capture size
            if (VideoCaptureMode == 0)
                VideoCaptureMode = 1;

#if (!UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS))
            if (VideoCaptureMode == 3)
                VideoCaptureMode = 2;       // Full HD would be too long to process
#endif

#if (UNITY_STANDALONE_OSX && (UNITY_EDITOR || UNITY_STANDALONE))
            // Video Capture on MACOS through Unity is tricky 
            // (you might want to change following value when using a non-standard Camera
            if (VideoCaptureMode < 2)
                VideoCaptureMode = 2;
#endif
        }

        internal bool GetVerticalMirror()
        {
            return m_IsVideoVerticallyFlipped;
        }

        internal static int GetVideoCaptureMode(int width, int height)
        {
            if (width == 320 && height == 240) return 0;
            if (width == 640 && height == 480) return 1;
            if (width == 1280 && height == 720) return 2;
            if (width == 1920 && height == 1080) return 4;
            return -1;
        }

        internal int GetVideoCaptureWidth()
        {
            if (VideoCaptureMode == 0) return 320;
            if (VideoCaptureMode == 2) return 1280;
            if (VideoCaptureMode == 3) return 1920;
            return 640;
        }
        internal int GetVideoCaptureHeight()
        {
            if (VideoCaptureMode == 0) return 240;
            if (VideoCaptureMode == 2) return 720;
            if (VideoCaptureMode == 3) return 1080;
            return 480;
        }
        internal int GetProcessingWidth()
        {
            if (VideoCaptureMode == 0) return 320;
            if (VideoCaptureMode == 2) return 640;
            if (VideoCaptureMode == 3) return 480;
            return 640;
        }
        internal int GetProcessingHeight()
        {
            if (VideoCaptureMode == 0) return 240;
            if (VideoCaptureMode == 2) return 360;
            if (VideoCaptureMode == 3) return 270;
            return 480;
        }

        internal float GetVideoAspectRatio()
        {
            return (float)GetVideoCaptureWidth() / (float)GetVideoCaptureHeight();
        }

        internal float GetScreenAspectRatio()
        {
            float screen_AR = (float)Screen.width / (float)Screen.height;
            return screen_AR;

        }
        internal double GetMainCameraFovV()
        {
            float video_AR = (float)GetVideoAspectRatio();
            float screen_AR = GetScreenAspectRatio();
            double trackingCamera_fovh_radian = XmgTools.ConvertToRadian((double)CameraVerticalFOV);
            double trackingCamera_fovv_radian;
            if (VideoPlaneFittingMode == XmgVideoPlaneFittingMode.FitScreenHorizontally)
                trackingCamera_fovv_radian = XmgTools.ConvertHorizontalFovToVerticalFov(trackingCamera_fovh_radian, (double)screen_AR);
            else
                trackingCamera_fovv_radian = XmgTools.ConvertHorizontalFovToVerticalFov(trackingCamera_fovh_radian, (double)video_AR);
            return XmgTools.ConvertToDegree(trackingCamera_fovv_radian);
        }

        // Usefull for portrait and reverse protraits modes
        internal double GetPortraitMainCameraFovV()
        {
            float video_AR = (float)GetVideoAspectRatio();
            float screen_AR = GetScreenAspectRatio();

            double trackingCamera_fovh_radian = XmgTools.ConvertToRadian((double)CameraVerticalFOV);
            double trackingCamera_fovv_radian;
            if (VideoPlaneFittingMode == XmgVideoPlaneFittingMode.FitScreenHorizontally)
                trackingCamera_fovv_radian = trackingCamera_fovh_radian;
            else
            {
                trackingCamera_fovv_radian = XmgTools.ConvertHorizontalFovToVerticalFov(trackingCamera_fovh_radian, (double)video_AR);
                trackingCamera_fovv_radian = XmgTools.ConvertVerticalFovToHorizontalFov(trackingCamera_fovv_radian, (double)screen_AR);
            }

            return XmgTools.ConvertToDegree(trackingCamera_fovv_radian);
        }


        internal double[] GetVideoPlaneScale(double videoPlaneDistance)
        {
            double[] ret = new double[2];

            float video_AR = (float)GetVideoAspectRatio();
            float screen_AR = GetScreenAspectRatio();
            double scale_u, scale_v;

            if (VideoPlaneFittingMode == XmgVideoPlaneFittingMode.FitScreenHorizontally)
            {
                double mainCamera_fovv_radian = XmgTools.ConvertToRadian((double)GetMainCameraFovV());
                double mainCamera_fovh_radian = XmgTools.ConvertVerticalFovToHorizontalFov(mainCamera_fovv_radian, (double)screen_AR);
                scale_u = (videoPlaneDistance * Math.Tan(mainCamera_fovh_radian / 2.0));
                scale_v = (videoPlaneDistance * Math.Tan(mainCamera_fovh_radian / 2.0) * 1.0 / video_AR);
            }
            else
            {
                double mainCamera_fovv_radian = XmgTools.ConvertToRadian((double)GetMainCameraFovV());
                scale_u = (videoPlaneDistance * Math.Tan(mainCamera_fovv_radian / 2.0) * video_AR);
                scale_v = (videoPlaneDistance * Math.Tan(mainCamera_fovv_radian / 2.0));
            }
            ret[0] = scale_u;
            ret[1] = scale_v;
            return ret;
        }
    }
}