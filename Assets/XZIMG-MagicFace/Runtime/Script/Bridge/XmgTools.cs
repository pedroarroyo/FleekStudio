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
using System.IO;

namespace XZIMG
{
    public enum XmgOrientationMode
    {
        LandscapeLeft = 0,
        Portrait = 1,
        LandscapeRight = 2,
        PortraitUpsideDown = 3,
    };

    public enum XmgSegmentationMode
    {
        Disabled = 0,
        HairSegmentation = 1,
        BodySegmentation = 2,
#if (UNITY_IOS)
        BodySegmentationRobust = 3, // On ios only
#endif
    };

    /**
     * Common tool functions
     */
    public class XmgTools
    {
        static public XmgOrientationMode GetRenderOrientation(bool isFrontalCamera = true)
        {
#if (UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL)
            return XmgOrientationMode.LandscapeLeft;
#elif (UNITY_ANDROID)
		if (Screen.orientation == ScreenOrientation.LandscapeRight) return XmgOrientationMode.LandscapeLeft;
		else if (Screen.orientation == ScreenOrientation.Portrait) return XmgOrientationMode.PortraitUpsideDown;
		else if (Screen.orientation == ScreenOrientation.LandscapeLeft) return XmgOrientationMode.LandscapeRight;
		else return XmgOrientationMode.Portrait;        
#elif (UNITY_IOS)
		if (isFrontalCamera)
		{
			if (Screen.orientation == ScreenOrientation.LandscapeRight) return XmgOrientationMode.LandscapeRight;
			else if (Screen.orientation == ScreenOrientation.Portrait) return XmgOrientationMode.Portrait;
			else if (Screen.orientation == ScreenOrientation.LandscapeLeft) return XmgOrientationMode.LandscapeLeft;
			else return XmgOrientationMode.PortraitUpsideDown;
		}
		else
		{
		if (Screen.orientation == ScreenOrientation.LandscapeRight) return XmgOrientationMode.LandscapeLeft;
		else if (Screen.orientation == ScreenOrientation.Portrait) return XmgOrientationMode.PortraitUpsideDown;
		else if (Screen.orientation == ScreenOrientation.LandscapeLeft) return XmgOrientationMode.LandscapeRight;
		else return XmgOrientationMode.Portrait;
		}
#endif
        }

        /// <summary>
        ///  Get an orientation mode accoring to render orientation as set in the Player Settings
        ///  Note: on some devices/OS versions (eg. Google Pixels), Portrait PortraitUpsideDown will fall back into Portrait
        /// </summary>
        static public XmgOrientationMode GetVideoOrientation(bool useNativeCamera, bool isFrontalCamera = true)
        {
            //return XmgOrientationMode.Portrait;
#if (UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL)
            return XmgOrientationMode.LandscapeLeft;
#elif (UNITY_ANDROID)
		if (Screen.orientation == ScreenOrientation.LandscapeRight) return XmgOrientationMode.LandscapeRight;
		else if (Screen.orientation == ScreenOrientation.Portrait) return XmgOrientationMode.Portrait;
		else if (Screen.orientation == ScreenOrientation.LandscapeLeft) return XmgOrientationMode.LandscapeLeft;
		else return XmgOrientationMode.PortraitUpsideDown;
        
#elif (UNITY_IOS)
		if (isFrontalCamera)
		{
			if (Screen.orientation == ScreenOrientation.LandscapeRight) return XmgOrientationMode.LandscapeRight;
			else if (Screen.orientation == ScreenOrientation.Portrait) return XmgOrientationMode.PortraitUpsideDown;
			else if (Screen.orientation == ScreenOrientation.LandscapeLeft) return XmgOrientationMode.LandscapeLeft;
			else return XmgOrientationMode.Portrait;
		}
		else
		{
			if (Screen.orientation == ScreenOrientation.LandscapeRight) return XmgOrientationMode.LandscapeRight;
			else if (Screen.orientation == ScreenOrientation.Portrait) return XmgOrientationMode.Portrait;
			else if (Screen.orientation == ScreenOrientation.LandscapeLeft) return XmgOrientationMode.LandscapeLeft;
			else return XmgOrientationMode.PortraitUpsideDown;
		}
#endif
        }

        // -------------------------------------------------------------------------------------------------------------------

        static public XmgOrientationMode GetDeviceCurrentOrientation(int captureDeviceOrientation, bool isFrontalCamera = false)
        {
            XmgOrientationMode orientation = XmgOrientationMode.LandscapeLeft;// Default portrait
#if (UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL)
            orientation = (XmgOrientationMode)captureDeviceOrientation;
#elif (UNITY_ANDROID)
        orientation = XmgOrientationMode.Portrait; // Default
        DeviceOrientation deviceOrientation = Input.deviceOrientation;
        if (deviceOrientation == DeviceOrientation.LandscapeRight) orientation = XmgOrientationMode.LandscapeRight;
        if (deviceOrientation == DeviceOrientation.LandscapeLeft) orientation = XmgOrientationMode.LandscapeLeft;
        if (deviceOrientation == DeviceOrientation.PortraitUpsideDown) orientation = XmgOrientationMode.PortraitUpsideDown;
		if (!isFrontalCamera && deviceOrientation == DeviceOrientation.Portrait) orientation = XmgOrientationMode.PortraitUpsideDown;
		if (!isFrontalCamera && deviceOrientation == DeviceOrientation.PortraitUpsideDown) orientation = XmgOrientationMode.Portrait;
#elif (UNITY_IOS)
		orientation = XmgOrientationMode.PortraitUpsideDown; // Default
		DeviceOrientation deviceOrientation = Input.deviceOrientation;
		if (deviceOrientation == DeviceOrientation.LandscapeRight) orientation = XmgOrientationMode.LandscapeLeft;
		if (deviceOrientation == DeviceOrientation.LandscapeLeft) orientation = XmgOrientationMode.LandscapeRight;
		if (deviceOrientation == DeviceOrientation.PortraitUpsideDown) orientation = XmgOrientationMode.Portrait;
		if (!isFrontalCamera && deviceOrientation == DeviceOrientation.LandscapeRight) orientation = XmgOrientationMode.LandscapeRight;
		if (!isFrontalCamera && deviceOrientation == DeviceOrientation.LandscapeLeft) orientation = XmgOrientationMode.LandscapeLeft;

#endif
            return orientation;
        }

        // -------------------------------------------------------------------------------------------------------------------

        static public float ConvertToRadian(float degreeAngle)
        {
            return (degreeAngle * ((float)Math.PI / 180.0f));
        }
        static public double ConvertToRadian(double degreeAngle)
        {
            return (degreeAngle * (Math.PI / 180.0f));
        }
        static public float ConvertToDegree(float degreeAngle)
        {
            return (degreeAngle * (180.0f / (float)Math.PI));
        }
        static public double ConvertToDegree(double degreeAngle)
        {
            return (degreeAngle * (180.0f / Math.PI));
        }
        static public double ConvertHorizontalFovToVerticalFov(double radianAngle, double aspectRatio)
        {
            return (Math.Atan(1.0 / aspectRatio * Math.Tan(radianAngle / 2.0)) * 2.0);
        }

        static public double ConvertVerticalFovToHorizontalFov(double radianAngle, double aspectRatio)
        {
            return (Math.Atan(aspectRatio * Math.Tan(radianAngle / 2.0)) * 2.0);
        }

        static public double ConvertFov(double degreeAngle, double aspectRatio)
        {
            return ConvertToDegree(Math.Atan(aspectRatio * Math.Tan(ConvertToRadian(degreeAngle) / 2.0)) * 2.0);
        }

        static public float ConvertFov(float fox_video_degree, float video_ar, float screen_ar)
        {
            double aspectRatio = (double)video_ar / screen_ar;
            return (float)ConvertToDegree(Math.Atan(aspectRatio * Math.Tan(ConvertToRadian((double)fox_video_degree) / 2.0)) * 2.0);

        }

        // -------------------------------------------------------------------------------------------------------------------

        static public void UpdateObjectPosition(XmgNonRigidFaceData nonRigidData, GameObject renderObject, float planeScale, bool mirror, bool frontal)
        {
            Quaternion quatRot = Quaternion.Euler(0, 0, 0);
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        int rotation = (int)XmgTools.GetRenderOrientation(frontal);
		if (rotation == 1) 
            quatRot = Quaternion.Euler(0, 0, 90);
        else if (rotation == 2)
            quatRot = Quaternion.Euler(0, 0, 0);
        else if (rotation == 3)
			quatRot = Quaternion.Euler(0, 0, -90);
		else 
			quatRot = Quaternion.Euler(0, 0, 180);
#endif
            //#if (UNITY_IOS) && !UNITY_EDITOR
            //        int rotation = (int)XmgTools.GetRenderOrientation(frontal);
            //        if (rotation == 1) 
            //            quatRot = Quaternion.Euler(0, 0, -90);
            //        else if (rotation == 2)
            //            quatRot = Quaternion.Euler(0, 0, 180);
            //        else if (rotation == 3)
            //            quatRot = Quaternion.Euler(0, 0, 90);
            //        else 
            //            quatRot = Quaternion.Euler(0, 0, 0);
            //#endif

            Vector3 position = nonRigidData.m_position;
            position.y *= -1;   // left hand -> right hand coordinate system
            Quaternion quat = Quaternion.Euler(nonRigidData.m_euler);
            if (mirror)
            {
                quat.y = -quat.y;
                quat.z = -quat.z;
                position.x = -position.x;
            }

            renderObject.transform.localPosition = quatRot * position;
            renderObject.transform.localRotation = quatRot * quat;
            renderObject.transform.localScale = new Vector3(planeScale, planeScale, planeScale);
        }


        static public Texture2D LoadPNG(string filePath)
        {

            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2, TextureFormat.BGRA32, false);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            }
            return tex;
        }

        static public void RenderWireframe(int[] triangles, Vector3[] vertices)
        {
            //meshRenderer.sharedMaterial.color = backgroundColor;
            //lineMaterial.SetPass(0);

            // GL.PushMatrix();
            // GL.MultMatrix(transform.localToWorldMatrix);
            GL.Begin(GL.LINES);
            Color lineColor = new Color(1, 0, 0, 1);
            GL.Color(lineColor);

            for (int i = 0; i < triangles.Length / 3; i++)
            {
                int idx1 = triangles[i * 3];
                int idx2 = triangles[i * 3];
                int idx3 = triangles[i * 3];
                GL.Vertex(vertices[idx1]);
                GL.Vertex(vertices[idx2]);

                GL.Vertex(vertices[idx2]);
                GL.Vertex(vertices[idx3]);

                GL.Vertex(vertices[idx3]);
                GL.Vertex(vertices[idx1]);
            }

            GL.End();
            //GL.PopMatrix();
        }


        public static bool IsDoubleTap()
        {
            bool result = false;
            float MaxTimeWait = 1;
            float VariancePosition = 10;

            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                float DeltaTime = Input.GetTouch(0).deltaTime;
                float DeltaPositionLenght = Input.GetTouch(0).deltaPosition.magnitude;

                if (DeltaTime > 0 && DeltaTime < MaxTimeWait && DeltaPositionLenght < VariancePosition)
                    result = true;
            }
            return result;
        }
    }
}