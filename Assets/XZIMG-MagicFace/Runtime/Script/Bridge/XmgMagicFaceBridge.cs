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
using System.Runtime.InteropServices;
using System.Text;

namespace XZIMG
{
    /// <summary>
    /// Face data for exchanging with the tracker library
    /// </summary>
    public class XmgMagicFaceData
    {
        /// <summary>
        ///  Data strcture for actual exchange.
        /// </summary>
        public XmgNonRigidFaceData m_faceData;

        public float[] m_dataLandmarks2D;
        protected GCHandle m_dataLandmarks2DHandle;
        public float[] m_dataLandmarks3D;
        protected GCHandle m_dataLandmarks3DHandle;
        public int[] m_dataTriangles2D;
        protected GCHandle m_dataTrianglesHandle;
        public float[] m_dataKeyLandmarks3D;
        protected GCHandle m_dataKeyLandmarks3DHandle;

        public XmgMagicFaceData()
        {
            m_dataLandmarks2D = new float[100 * 2];
            m_dataLandmarks2DHandle = GCHandle.Alloc(m_dataLandmarks2D, GCHandleType.Pinned);
            m_dataLandmarks3D = new float[800 * 3];
            m_dataLandmarks3DHandle = GCHandle.Alloc(m_dataLandmarks3D, GCHandleType.Pinned);
            m_dataTriangles2D = new int[500];
            m_dataTrianglesHandle = GCHandle.Alloc(m_dataTriangles2D, GCHandleType.Pinned);
            m_dataKeyLandmarks3D = new float[100 * 3];
            m_dataKeyLandmarks3DHandle = GCHandle.Alloc(m_dataKeyLandmarks3D, GCHandleType.Pinned);

            m_faceData.m_landmarks = m_dataLandmarks2DHandle.AddrOfPinnedObject();
            m_faceData.m_landmarks3D = m_dataLandmarks3DHandle.AddrOfPinnedObject();
            m_faceData.m_triangles = m_dataTrianglesHandle.AddrOfPinnedObject();
            m_faceData.m_keyLandmarks3D = m_dataKeyLandmarks3DHandle.AddrOfPinnedObject();
            m_faceData.m_faceDetected = 0;
            m_faceData.m_facePoseComputed = 0;
        }

        ~XmgMagicFaceData()
        {
            m_dataLandmarks2DHandle.Free();
            m_dataLandmarks3DHandle.Free();
            m_dataTrianglesHandle.Free();
            m_dataKeyLandmarks3DHandle.Free();
        }
    }


    public enum XmgFaceFeaturesMode
    {
        AllFaceFeatures = 0,
        AllFaceFeaturesRobust = 1,
    };


    [StructLayout(LayoutKind.Sequential)]
    public struct XmgImage
    {
        public int m_width;
        public int m_height;
        public IntPtr m_imageData;

        /// Image Width Step (set to 0 for automatic computation)   
        public int m_iWStep;

        /// pixel format XMG_BW=0, XMG_RGB=1, XMG_BGR=2, XMG_YUV=3, XMG_RGBA=4, XMG_BGRA=5, XMG_ARGB=6                
        public int m_colorType;       
        public int m_type;
        // True if image is horizontally flipped (and needed to be reverted before used in the tracking)
        public bool m_flippedHorizontaly; 
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XmgRigidFaceData
    {
        public int m_faceDetected;
        public Vector3 m_position;
        public Vector3 m_euler;
        public Quaternion m_quatRot;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XmgMatrix3x3
    {
        public float x11, x12, x13;
        public float x21, x22, x23;
        public float x31, x32, x33;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XmgEyesPosition2D
    {
        public float xLeft, yLeft, xRight, yRight;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XmgEyesPosition3D
    {
        public float xLeft, yLeft, zLeft, xRight, yRight, zRight;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XmgEmotions
    {
        public float neutral;
        public float e1;
        public float e2;
        public float e3;
        public float e4;
        public float e5;
        public float e6;
        public float e7;
        public float getEmotion(int idx)
        {
            if (idx == 0) return neutral;
            if (idx == 1) return e1;
            if (idx == 2) return e2;
            if (idx == 3) return e3;
            if (idx == 4) return e4;
            if (idx == 5) return e5;
            if (idx == 6) return e6;
            if (idx == 7) return e7;
            return -1;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XmgNonRigidFaceData
    {
        public int m_faceDetected;
        public int m_facePoseComputed;

        public Vector3 m_position;
        public Vector3 m_euler;
        public Quaternion m_quatRot;
        public XmgMatrix3x3 m_matRot;

        public int m_nbLandmarks3D;
        public int m_nbLandmarks;
        public IntPtr m_landmarks3D;
        public IntPtr m_landmarks;
        public int m_nbTriangles;
        public IntPtr m_triangles;
        public IntPtr m_keyLandmarks3D;

        public XmgEyesPosition2D m_eyesPosition2D;
        public XmgEyesPosition3D m_eyesPosition3D;
        public XmgEyesPosition3D m_eyesCenter3D;

        public XmgEmotions m_emotions;
    }

    // -------------------------------------------------------------------------------------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    public struct XmgVideoCaptureOptions
    {
        /// 0 is 320x240; 1, is 640x480; 2 is 720p (-1 if no internal capture);
        public int m_resolutionMode;

        /// 0 is frontal; 1 is back
        public int m_frontal;

        /// 0 auto-focus now; 1 auto-focus continually; 2 locked; 3 focus to infinity; 4 focus macro;
        public int m_focusMode;

        /// (no effect on Android) 
        public int m_exposureMode;

        /// 0 auto-white balance; 1 day-light white balance
        public int m_whileBalanceMode;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XmgInitParams
    {
        /// Tracking mode
        ///     -->   static const int XMG_NON_RIGID_FACE_REG2D_3D = 4;
        ///     -->   static const int XMG_FACE_TRACKING_3D = 5;
        ///     -->   static const int XMG_NON_RIGID_FACE_REG2D_3D_CNN = 7;
        public int m_trackingMode;   

        /// Size of the image to process
        public int m_processingWidth;                        
        public int m_processingHeight;

        /// Number of facial features to be detected
        public int m_nbFacialFeatures;        

        /// Maximum number of faces to be detected simultaneously               
        public int m_nbMaxFaceObjects;           

        /// fov (vertical) in degree (around 50.0)            
        public float m_fovVerticalDegree;          
        
        /// 1 if you want the eyes to be tracked
        public int m_trackEyesPositions;

        /// 1 if you want to detect emotions             
        public int m_detectEmotions;

        /// Face Classifier data      
        public System.IntPtr m_faceClassifier;    

        //public System.IntPtr m_faceDetector_fn;  
        public String m_faceDetector_fn;  
        //public System.IntPtr m_faceFeaturesDetector_fn;  
        public String m_faceFeaturesDetector_fn;  
        //public System.IntPtr m_mouthRefiner_fn;  
        public String m_mouthRefiner_fn;  

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XmgEmotionParams
    {
    }

    /// Deep learning models parameters
    [StructLayout(LayoutKind.Sequential)]
    public struct XmgInitCNNParams
    {
        /// Network input size
        public int m_inWidth;
        public int m_inHeight;
        
        /// Number of input channels
        public int m_in_channels;

        /// Compression mode (0 is no compression, 1 indicates the model is compressed)
        public int m_compressionMode;
        
        /// Type of network (0 is very shallow, 4 is quite deep)
        public int m_netType;
        
        /// Is the network quantized (activations + weights)
        public int m_quantized;

        /// Thread number (-1) no multithreading - (0) automatic (1 per core) - (N) user defined
        public int m_num_threads;

        public int m_norm;
        public float m_rcnnWeight;
        public int m_cnnWeightsSize;

        /// Preloaded weights
        public System.IntPtr m_cnnWeights;

        /// Filename to the weights
        public String m_segmentation_fn;
    }

    /// This class contains the interface with the plugin for different platforms
    internal class XmgMagicFaceBridge
    {
        static public void PrepareImage(
            ref XmgImage dstimage,
            int width, int height,
            int colortype,
            IntPtr ptrdata)
        {
            dstimage.m_width = width;
            dstimage.m_height = height;
            dstimage.m_colorType = colortype;
            dstimage.m_type = 0;
#if (!UNITY_EDITOR && UNITY_ANDROID) || (!UNITY_EDITOR && UNITY_IOS)
            dstimage.m_flippedHorizontaly = false;
#else
            dstimage.m_flippedHorizontaly = true;
#endif
            dstimage.m_iWStep = 0;
            dstimage.m_imageData = ptrdata;
        }

        static public void PrepareImage(
            ref XmgImage dstimage,
            int width, int height,
            IntPtr ptrdata)
        {
            dstimage.m_width = width;
            dstimage.m_height = height;
#if (!UNITY_EDITOR && UNITY_IOS)
            dstimage.m_colorType = 5; // XMG_BGRA
#else
            dstimage.m_colorType = 4; // XMG_RGBA
#endif
            dstimage.m_type = 0;
#if (!UNITY_EDITOR && UNITY_ANDROID) || (!UNITY_EDITOR && UNITY_IOS)
            dstimage.m_flippedHorizontaly = false;
#else
            dstimage.m_flippedHorizontaly = true;
#endif
            dstimage.m_iWStep = 0;
            dstimage.m_imageData = ptrdata;
        }

        static public void PrepareGrayImage(
            ref XmgImage dstimage,
            int width, int height,
            IntPtr ptrdata)
        {
            dstimage.m_width = width;
            dstimage.m_height = height;
            dstimage.m_colorType = 0;
            dstimage.m_type = 0;
#if (!UNITY_EDITOR && UNITY_ANDROID) || (!UNITY_EDITOR && UNITY_IOS)
            dstimage.m_flippedHorizontaly = false;
#else
            dstimage.m_flippedHorizontaly = true;
#endif
            dstimage.m_iWStep = 0;
            dstimage.m_imageData = ptrdata;
        }
        
        static public void PrepareNativeVideoCapture(
            ref XmgVideoCaptureOptions videoCaptureOptions,
            int resolutionMode,
            int frontal,
            int focusMode,
            int exposureMode,
            int whileBalanceMode)
        {
            videoCaptureOptions.m_resolutionMode = resolutionMode;
            videoCaptureOptions.m_frontal = frontal;
            videoCaptureOptions.m_focusMode = focusMode;
            videoCaptureOptions.m_exposureMode = exposureMode;
            videoCaptureOptions.m_whileBalanceMode = whileBalanceMode;
        }

        static public void PrepareNativeVideoCaptureDefault(
            ref XmgVideoCaptureOptions videoCaptureOptions,
            int resolutionMode,
            int frontal)
        {
            videoCaptureOptions.m_resolutionMode = resolutionMode;
            videoCaptureOptions.m_frontal = frontal;
            videoCaptureOptions.m_focusMode = 1;
            videoCaptureOptions.m_exposureMode = 1;
            videoCaptureOptions.m_whileBalanceMode = 1;
#if (UNITY_ANDROID)
        videoCaptureOptions.m_focusMode = 2;        // -1 is default
        videoCaptureOptions.m_exposureMode = -1;    // -1 is default
        videoCaptureOptions.m_whileBalanceMode = -1; // -1 is default
#endif
        }

        static public void PrepareMagicFaceInitParams(
            ref XmgInitParams initializationParams,
            bool detect3DFacialFeatures,
            int processingWidth,
            int processingHeight,
            int nbFacialFeatures,
            int nbMaxFaceObjects,
            float fovVerticalDegree,
            int trackEyesPositions,
            bool robustModel=false)
        {
            initializationParams.m_trackingMode = robustModel ? 7 : 4; 
            initializationParams.m_processingWidth = processingWidth;
            initializationParams.m_processingHeight = processingHeight;
            initializationParams.m_nbFacialFeatures = nbFacialFeatures;
            initializationParams.m_nbMaxFaceObjects = nbMaxFaceObjects;
            initializationParams.m_fovVerticalDegree = fovVerticalDegree;
            initializationParams.m_trackEyesPositions = trackEyesPositions;
            initializationParams.m_detectEmotions = 0;
            initializationParams.m_faceClassifier = System.IntPtr.Zero;
#if (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
            String str = Application.dataPath + "/XZIMG-MagicFace/Runtime/Resources/XZIMG/Models/facedetector.xzimg";
            initializationParams.m_faceDetector_fn =  str;

            String str_features = Application.dataPath + "/XZIMG-MagicFace/Runtime/Resources/XZIMG/Models/facefeaturesdetector.xzimg";
            initializationParams.m_faceFeaturesDetector_fn =  str_features;

            String str_mouth = Application.dataPath + "/XZIMG-MagicFace/Runtime/Resources/XZIMG/Models/mouthfeaturesdetector.xzimg";
            initializationParams.m_mouthRefiner_fn =  str_mouth;
#endif
        }

        static public void DefaultInit(
            ref XmgInitCNNParams initializationParams)
        {
            initializationParams.m_cnnWeights = System.IntPtr.Zero;
            initializationParams.m_inWidth = 128;
            initializationParams.m_inHeight = 128;
            initializationParams.m_in_channels = 3;
            initializationParams.m_compressionMode = 0;
            initializationParams.m_quantized = 1;
            initializationParams.m_netType = 7;
            initializationParams.m_num_threads = 0;
        }

        /// <summary>
        ///  Returns segmentation asset name depeding on the device, the camera mode and segmentation mode
        /// </summary>
        /// <returns></returns>
        static public String GetSegmentationAssetName(
            XmgSegmentationMode segmentationMode,
            XmgOrientationMode orientationMode)
        {
            String result = "";
#if (!UNITY_EDITOR && UNITY_IOS)
            result = "";
            if (segmentationMode == XmgSegmentationMode.BodySegmentationRobust)
                result = "XZIMG/Models/ENET150-192x192-16-BODY";
            // if (segmentationMode == XmgSegmentationMode.BodySegmentation)
            //      result = "XZIMG/Models/ENET100-192x192-16-BODY";
            if (segmentationMode == XmgSegmentationMode.HairSegmentation)
                result = "XZIMG/Models/ENET100-192x192-16-HAIRS-ONLY";
#elif (!UNITY_EDITOR && UNITY_ANDROID)            
            if (segmentationMode == XmgSegmentationMode.HairSegmentation)
            {
                // if (orientationMode == XmgOrientationMode.Portrait ||
                //     orientationMode == XmgOrientationMode.PortraitUpsideDown)
                    result = "XZIMG/Models/hair_192x192";
            }   
            if (segmentationMode == XmgSegmentationMode.BodySegmentation)
            {
                // if (orientationMode == XmgOrientationMode.Portrait ||
                //     orientationMode == XmgOrientationMode.PortraitUpsideDown)
                    result = "XZIMG/Models/body_192x192";
            }
#elif (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
            //TODO result = "XZIMG/Models/ENET150-192x192-16-BODY";
            return "";
#else
            if (segmentationMode == XmgSegmentationMode.HairSegmentation)
            {
                if (orientationMode == XmgOrientationMode.LandscapeLeft ||
                    orientationMode == XmgOrientationMode.LandscapeRight)
                    result = "XZIMG/Models/hair-128x176T";
                else
                    result = "XZIMG/Models/hair-176x128T";
            }
            else
            {
                if (orientationMode == XmgOrientationMode.LandscapeLeft ||
                    orientationMode == XmgOrientationMode.LandscapeRight)
                    //result = "XZIMG/Models/seg-176x256-opt";
                    result = "XZIMG/Models/seg-160x256-opt";
                else
                    result = "XZIMG/Models/seg-256x176-opt";
            }
#endif
            return result;
        }

        static public void PrepareInitSegmentationParams(
            XmgSegmentationMode mode,
            ref XmgInitCNNParams initializationParams,
            System.IntPtr weights,
            XmgOrientationMode orientationMode)
        {
            if (mode == XmgSegmentationMode.Disabled)
                return;
            initializationParams.m_cnnWeights = weights;

#if (!UNITY_EDITOR && UNITY_IOS)
            if (
                mode == XmgSegmentationMode.HairSegmentation || 
                mode == XmgSegmentationMode.BodySegmentationRobust)
            {
                initializationParams.m_inWidth = 192;
                initializationParams.m_inHeight = 192;
                initializationParams.m_compressionMode = 1;
                initializationParams.m_quantized = 0;

                if (mode == XmgSegmentationMode.HairSegmentation)
                    initializationParams.m_netType = 3;
                if (mode == XmgSegmentationMode.BodySegmentationRobust)
                    initializationParams.m_netType = 4;

                initializationParams.m_num_threads = -1;
            }
            else
            {
                initializationParams.m_netType = 8; // COREML
                //String str = Application.dataPath + "/bodysegmentation_rec.xzimg";
                //String str = Application.dataPath + "/XZIMG-MagicFace/Runtime/Resources/XZIMG/Models/bodysegmentation256x176.xzimg";
                //initializationParams.m_segmentation_fn = str;              
                initializationParams.m_inHeight = 256;
                initializationParams.m_inWidth = 160; //176
                initializationParams.m_in_channels = 3;
            }
            initializationParams.m_rcnnWeight = 0.85f;

#elif (!UNITY_EDITOR && UNITY_ANDROID)
            initializationParams.m_inWidth = 192;
            initializationParams.m_inHeight = 192;
            initializationParams.m_in_channels = 3;
            initializationParams.m_compressionMode = 0;
            initializationParams.m_quantized = 2;
            initializationParams.m_netType = 7;

            // Multi-threading option (Android)
            // 0 will set the thread number automatically according to the processor
            // Avoid changing as much as possible this value (0 sometimes don't work properly in Unity)
            // Best is to set to (num_cpu - 1) to let Unity rendering thread smooth
            initializationParams.m_num_threads = 4;
            initializationParams.m_rcnnWeight = 0.85f;

#elif !(UNITY_EDITOR_OSX  || UNITY_STANDALONE_OSX)
            initializationParams.m_inWidth = 128;
            initializationParams.m_inHeight = 176;
            initializationParams.m_compressionMode = 0;
            initializationParams.m_netType = 7;

            initializationParams.m_num_threads = 8;
            initializationParams.m_quantized = 0;
            initializationParams.m_in_channels = 4;
            initializationParams.m_rcnnWeight = 0.95f;
#endif
            initializationParams.m_norm = 0;

#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
            // -- Desktop (OSX / WIN) parameters
            if (mode == XmgSegmentationMode.BodySegmentation)
            {
                initializationParams.m_inHeight = 160;
                initializationParams.m_inWidth = 256;
                initializationParams.m_in_channels = 4;
                initializationParams.m_rcnnWeight = 1.0f;
            }
            else
            {
                initializationParams.m_inHeight = 128;
                initializationParams.m_inWidth = 176;
                initializationParams.m_in_channels = 4;
                initializationParams.m_rcnnWeight = 0.9f;
            }
            if (orientationMode == XmgOrientationMode.Portrait ||
                orientationMode == XmgOrientationMode.PortraitUpsideDown)
            {
                int tmp = initializationParams.m_inHeight;
                initializationParams.m_inHeight = initializationParams.m_inWidth;
                initializationParams.m_inWidth = tmp;
            }
#elif (UNITY_EDITOR_OSX  || UNITY_STANDALONE_OSX)
            // -- Desktop (COREML) parameters
            initializationParams.m_netType = 8;
            if (orientationMode == XmgOrientationMode.LandscapeLeft ||
                orientationMode == XmgOrientationMode.LandscapeRight)
            {
                String str = Application.dataPath + "/XZIMG-MagicFace/Runtime/Resources/XZIMG/Models/bodysegmentation160x256_rec.xzimg";
                //String str = Application.dataPath + "/XZIMG-MagicFace/Runtime/Resources/XZIMG/Models/bodysegmentation176x256.xzimg";
                initializationParams.m_segmentation_fn = str;
                initializationParams.m_inHeight = 160;
                initializationParams.m_inWidth = 256;
                initializationParams.m_in_channels = 4;           
                initializationParams.m_rcnnWeight = 1.0f;
            }
            else
            { 
                String str = Application.dataPath + "/XZIMG-MagicFace/Runtime/Resources/XZIMG/Models/bodysegmentation256x176.xzimg";
                initializationParams.m_segmentation_fn = str;              
                initializationParams.m_inHeight = 256;
                initializationParams.m_inWidth = 176;
                initializationParams.m_in_channels = 3;
            }
#endif
        }

        static public void PrepareInitFaceEmotionsParams(
            ref XmgInitCNNParams initializationParams,
            System.IntPtr weights)
        {
            initializationParams.m_cnnWeights = weights;
#if UNITY_IOS
            initializationParams.m_inWidth = 48;
            initializationParams.m_inHeight = 48;
            initializationParams.m_in_channels = 1;
            initializationParams.m_compressionMode = 1;
            initializationParams.m_quantized = 0;
            initializationParams.m_netType = 1;
            
            // Avoid changing this valueas much as possible 
            initializationParams.m_num_threads = 1;
#else
            initializationParams.m_inWidth = 48;
            initializationParams.m_inHeight = 48;
            initializationParams.m_in_channels = 1;
            initializationParams.m_compressionMode = 0;
            initializationParams.m_quantized = 0;
            initializationParams.m_netType = 0;

            // Avoid changing this valueas much as possible 
            initializationParams.m_num_threads = 1;
#endif
        }
        // -------------------------------------------------------------------------------------------------------------------
        // -------------------------------------------------------------------------------------------------------------------

#if ((UNITY_STANDALONE || UNITY_EDITOR || UNITY_ANDROID))
        [DllImport("xzimgMagicFace")]
        public static extern int xzimgMagicFaceInitialize([In][Out] ref XmgInitParams initializationParams);
        [DllImport("xzimgMagicFace")]
        public static extern void xzimgMagicFaceRelease();
        [DllImport("xzimgMagicFace")]
        public static extern void xzimgMagicFacePause(int pause);
        [DllImport("xzimgMagicFace")]
        public static extern void xzimgMagicFaceReset();
        [DllImport("xzimgMagicFace")]
        public static extern int xzimgMagicFaceDetectNonRigidFaces2D([In][Out] ref XmgImage imageIn, int orientation);
        [DllImport("xzimgMagicFace")]
        public static extern int xzimgMagicFaceTrackNonRigidFaces([In][Out] ref XmgImage imageIn, int orientation);
        [DllImport("xzimgMagicFace")]
        public static extern int xzimgMagicFaceGetFaceData(int idxObject, [In][Out] ref XmgNonRigidFaceData nonRigidData);
        [DllImport("xzimgMagicFace")]
        public static extern int xzimgMagicFace2DTriangulation(
            IntPtr vertices2D, int nbVertices, 
            IntPtr outTriangles, IntPtr nbTriangles, 
            int fillEyes, int fillMouth);
        [DllImport("xzimgMagicFace")]
        public static extern int xzimgMagicFaceRefineContour(
            IntPtr in_contour, int in_contour_size,
            IntPtr out_contour, IntPtr out_contour_size);

        [DllImport("xzimgMagicFace")]
        public static extern int xzimgMagicFaceRefineContourTriangulate(
            IntPtr in_contour, int in_contour_size,
            IntPtr out_contour, IntPtr out_contour_size,
            IntPtr outTriangles, IntPtr nbTriangles,
            int fillEyes, int fillMouth);

        [DllImport("xzimgMagicFace")]
        public static extern int xzimgMagicFaceRefineMouthContourTriangulate(
            IntPtr in_contour, int in_contour_size,
            IntPtr out_contour, IntPtr out_contour_size,
            IntPtr outTriangles, IntPtr nbTriangles,
            int fillMouth);

        [DllImport("xzimgMagicFace")]
        public static extern int xzimgMagicFaceInitializeEmotion([In][Out] ref XmgInitCNNParams initParams);
        [DllImport("xzimgMagicFace")]
        public static extern int xzimgMagicFaceProcessEmotion(int idxFace, float filter);

        [DllImport("xzimgMagicFace")]
        public static extern int xzimgMagicSegmentationInitialize([In][Out] ref XmgInitCNNParams initParams);
        [DllImport("xzimgMagicFace")]
        public static extern void xzimgMagicSegmentationRelease();
        [DllImport("xzimgMagicFace")]
        public static extern void xzimgMagicSegmentationPause(int pause);
        [DllImport("xzimgMagicFace")]
        public static extern int xzimgMagicSegmentationProcess([In][Out] ref XmgImage imageIn, int rotation);
        [DllImport("xzimgMagicFace")]
        public static extern void xzimgMagicSegmentationSetColor(int R, int G, int B, int A);
        [DllImport("xzimgMagicFace")]
        public static extern int xzimgMagicSegmentationGetSegmentationImage([In][Out] ref XmgImage imageIn, int rotation, int normMode);
        [DllImport("xzimgMagicFace")]
        public static extern int xzimgMagicSegmentationGetDebugImage([In][Out] ref XmgImage imageIn);

        [DllImport("xzimgMagicFace")]
        public static extern int xzimgMagicFaceGetNumActionUnits();
        [DllImport("xzimgMagicFace")]
        public static extern int xzimgMagicFaceGetActionUnits(int idxFace, System.IntPtr actionUnits);
        [DllImport("xzimgMagicFace")]
        public static extern int xzimgActivateMouthRefiner(int activate);

#elif UNITY_WEBGL
        [DllImport ("__Internal")] 
	    public static extern int xzimgMagicFaceInitialize([In][Out] ref XmgInitParams initializationParams);
        [DllImport ("__Internal")] 
        public static extern void xzimgMagicFaceRelease();
        [DllImport ("__Internal")] 
        public static extern void xzimgMagicFacePause(int pause);
        [DllImport ("__Internal")] 
        public static extern void xzimgMagicFaceReset();
        [DllImport ("__Internal")] 
        public static extern int xzimgMagicFaceTrackNonRigidFaces([In][Out] ref XmgImage imageIn, int orientation);
        [DllImport ("__Internal")] 
        public static extern int xzimgMagicFaceGetFaceData(int idxObject, [In][Out] ref xmgNonRigidFaceData nonRigidData);
        [DllImport("__Internal")]
        public static extern int xzimgMagicFace2DTriangulation(
            IntPtr vertices2D, int nbVertices, IntPtr outTriangles, IntPtr nbTriangles, int fillEyes, int fillMouth);
        [DllImport("__Internal")]
        public static extern int xzimgMagicFaceRefineContour(
            IntPtr in_contour, int in_contour_size,
            IntPtr out_contour, IntPtr out_contour_size);

        [DllImport("__Internal")]
        public static extern int xzimgMagicFaceRefineContourTriangulate(
            IntPtr in_contour, int in_contour_size,
            IntPtr out_contour, IntPtr out_contour_size, 
            IntPtr outTriangles, IntPtr nbTriangles,
            int fillEyes, int fillMouth);
    
        [DllImport("__Internal")]
        public static extern int xzimgMagicFaceRefineMouthContourTriangulate(
            IntPtr in_contour, int in_contour_size,
            IntPtr out_contour, IntPtr out_contour_size, 
            IntPtr outTriangles, IntPtr nbTriangles,
            int fillMouth);

        [DllImport("__Internal")]
        public static extern int xzimgMagicFaceInitializeEmotion([In][Out] ref XmgInitCNNParams initParams);
        [DllImport("__Internal")]
        public static extern int xzimgMagicFaceProcessEmotion(int idxFace, float filter);
        [DllImport("__Internal")]
        public static extern int xzimgMagicFaceGetNumActionUnits();
        [DllImport("__Internal")]
        public static extern int xzimgMagicFaceGetActionUnits(int idxFace, System.IntPtr actionUnits); //System.IntPtr

        // Segmentation
        [DllImport("__Internal")]
        public static extern int xzimgMagicSegmentationInitialize([In][Out] ref XmgInitCNNParams initParams);
        [DllImport("__Internal")]
        public static extern void xzimgMagicSegmentationRelease();
        [DllImport("__Internal")]
        public static extern void xzimgMagicSegmentationPause(int pause);
        [DllImport("__Internal")]
        public static extern int xzimgMagicSegmentationProcess([In][Out] ref XmgImage imageIn, int rotation);
        [DllImport("__Internal")]
        public static extern void xzimgMagicSegmentationSetColor(int R, int G, int B, int A);
        [DllImport("__Internal")]
        public static extern int xzimgMagicSegmentationGetSegmentationImage([In][Out] ref XmgImage imageIn, int rotation, int normMode);
        [DllImport("__Internal")]
        public static extern int xzimgMagicSegmentationGetDebugImage([In][Out] ref XmgImage imageIn);

#elif (UNITY_IOS)
	    [DllImport ("__Internal")]	
	    public static extern int xzimgMagicFaceInitialize([In][Out] ref XmgInitParams initializationParams);
	    [DllImport ("__Internal")]
	    public static extern void xzimgMagicFaceRelease();
        [DllImport ("__Internal")] 
        public static extern void xzimgMagicFacePause(int pause);
        [DllImport ("__Internal")] 
        public static extern void xzimgMagicFaceReset();
	    [DllImport ("__Internal")]
	    public static extern int xzimgMagicFaceTrackNonRigidFaces([In][Out] ref XmgImage imageIn, int orientation);
	    [DllImport ("__Internal")]
	    public static extern int xzimgMagicFaceGetFaceData(int idxObject, [In][Out] ref XmgNonRigidFaceData nonRigidData);
        [DllImport ("__Internal")]
        public static extern int xzimgMagicFace2DTriangulation(
            IntPtr vertices2D, int nbVertices, IntPtr outTriangles, IntPtr nbTriangles, int fillEyes, int fillMouth);    

        [DllImport ("__Internal")]
        public static extern int xzimgMagicFaceRefineContour(
            IntPtr in_contour, int in_contour_size,
            IntPtr out_contour, IntPtr out_contour_size);

        [DllImport("__Internal")]
        public static extern int xzimgMagicFaceRefineContourTriangulate(
            IntPtr in_contour, int in_contour_size,
            IntPtr out_contour, IntPtr out_contour_size, 
            IntPtr outTriangles, IntPtr nbTriangles,
            int fillEyes, int fillMouth);
        
        [DllImport("__Internal")]
        public static extern int xzimgMagicFaceRefineMouthContourTriangulate(
            IntPtr in_contour, int in_contour_size,
            IntPtr out_contour, IntPtr out_contour_size, 
            IntPtr outTriangles, IntPtr nbTriangles,
            int fillMouth);


    
        [DllImport("__Internal")]
        public static extern int xzimgMagicFaceInitializeEmotion([In][Out] ref XmgInitCNNParams initParams);
        [DllImport("__Internal")]
        public static extern int xzimgMagicFaceProcessEmotion(int idxFace, float filter);
        [DllImport("__Internal")]
        public static extern int xzimgMagicFaceGetNumActionUnits();
        [DllImport("__Internal")]
        public static extern int xzimgMagicFaceGetActionUnits(int idxFace, System.IntPtr actionUnits);
        [DllImport("__Internal")]
        public static extern int xzimgActivateMouthRefiner(int activate);

        // Segmentation
        [DllImport("__Internal")]
        public static extern int xzimgMagicSegmentationInitialize([In][Out] ref XmgInitCNNParams initParams);
        [DllImport("__Internal")]
        public static extern void xzimgMagicSegmentationRelease();
        [DllImport("__Internal")]
        public static extern void xzimgMagicSegmentationPause(int pause);
        [DllImport("__Internal")]
        public static extern int xzimgMagicSegmentationProcess([In][Out] ref XmgImage imageIn, int rotation);
        [DllImport("__Internal")]
        public static extern void xzimgMagicSegmentationSetColor(int R, int G, int B, int A);
        [DllImport("__Internal")]
        public static extern int xzimgMagicSegmentationGetSegmentationImage([In][Out] ref XmgImage imageIn, int rotation, int normMode);
        [DllImport("__Internal")]
        public static extern int xzimgMagicSegmentationGetDebugImage([In][Out] ref XmgImage imageIn);
#endif

        /// Video capture is a bit specific on Android because of messy Android API
#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaObject m_videoActivity = null;
        private static AndroidJavaObject m_activityContext = null;

        public static void xzimgCamera_create([In][Out] ref XmgVideoCaptureOptions videoCaptureParams)
        {
            if (m_activityContext == null)
            {
                AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                m_activityContext = jc.GetStatic<AndroidJavaObject>("currentActivity");
            }

            if (m_videoActivity == null)
            {
                AndroidJavaClass xzimg_video_plugin = new AndroidJavaClass("com.xzimg.videocapture.VideoCaptureAPI");
                if (xzimg_video_plugin != null)
                {
                    m_videoActivity = xzimg_video_plugin.CallStatic<AndroidJavaObject>("instance");
                }
            }
            if (m_videoActivity != null)
                m_videoActivity.Call("xzimgCamera_create", 
                    videoCaptureParams.m_resolutionMode, 
                    videoCaptureParams.m_frontal,
                    videoCaptureParams.m_focusMode,
                    videoCaptureParams.m_whileBalanceMode);
        }
    
        [DllImport("xzimgMagicFace")]
        public static extern int xzimgCamera_getCaptureWidth();
        [DllImport("xzimgMagicFace")]
        public static extern int xzimgCamera_getCaptureHeight();
        [DllImport("xzimgMagicFace")]
        public static extern int xzimgCamera_getImage(System.IntPtr rgba_frame);
        public static void xzimgCamera_delete()
        {
            if (m_videoActivity != null)
                m_videoActivity.Call("xzimgCamera_delete");
        }
    

        public static void xzimgCamera_focus()
        {
            if (m_videoActivity != null)
                m_videoActivity.Call("xzimgCamera_focus");
        }

#endif

        /// Video capture on IOS
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        public static extern int xzimgCamera_create([In][Out] ref XmgVideoCaptureOptions videoCaptureParams);
        [DllImport("__Internal")]
        public static extern int xzimgCamera_delete();
        [DllImport("__Internal")]
        public static extern int xzimgCamera_getCaptureWidth();
        [DllImport("__Internal")]
        public static extern int xzimgCamera_getCaptureHeight();
        [DllImport("__Internal")]
        public static extern int xzimgCamera_getImage(System.IntPtr rgba_frame);
#endif

    }
}
