﻿/**
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
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using System.Collections.Generic;

namespace XZIMG
{
    /// <summary>
    /// XmgMagicFace2DVisualizer visualization for the 2D mesh on the face (given key face features)
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(XmgMagicFace))]
    public sealed class XmgMagicFace2DVisualizer : MonoBehaviour
    {
        #region Public properties
        #endregion
                    
        #region Private properties
        /// <summary>
        /// <see cref="XmgMagicFace"/> attached face component 
        /// </summary>
        private XmgMagicFace m_face;
        
        /// <summary>
        /// Game object's mesh renderer
        /// </summary>
        private MeshRenderer m_MeshRenderer = null;

        /// <summary>
        /// Game object's Mesh filter
        /// </summary>
        private MeshFilter m_MeshFilter = null;

        /// <summary>
        /// AR Camera manager
        /// </summary>
        private XmgCameraManager m_CameraManager => m_face.Face3DManager.FaceTracking.CameraManager;

        /// <summary>
        /// Face features count return by XmgMagicFace
        /// </summary>
        private int m_FaceFeaturesCount => 68;

        /// <summary>
        /// GameObject created at each vertex to display shape location
        /// </summary>
        private List<GameObject> m_meshFeatures;

        /// <summary>
        /// Use a refined mesh (additional points around mouth and eyes)
        /// </summary>
        [SerializeField]
        [Tooltip("Select if we are using a denser model")]
        private bool m_RefineMesh2D = true;
        
        /// <summary>
        /// Matariel cache
        /// </summary>
        private Material m_DefaultMaterial;

        #endregion
        private void Awake()
        {
            m_face = GetComponent<XmgMagicFace>();
            var faceTracking = FindObjectOfType<XmgMagicFaceTracking>();

            // -- Get or create mesh filter
            m_MeshFilter = GetComponent<MeshFilter>();
            if (m_MeshFilter == null)
                m_MeshFilter = gameObject.AddComponent<MeshFilter>();


            // -- Get or create mesh renderer
            m_MeshRenderer = GetComponent<MeshRenderer>();
            if (m_MeshRenderer == null)
                m_MeshRenderer = gameObject.AddComponent<MeshRenderer>();

            // -- Load mesh 
            string meshAsset = "XZIMG/Models/face-2d-dense";
            GameObject obj = Resources.Load(meshAsset) as GameObject;
            if (obj == null)
            {
                Debug.LogError("==> (E) problems when loading the 2d face model mesh: " + meshAsset);
                return;
            }
            Mesh mesh = null;
            mesh = Instantiate(obj.transform.Find("default").GetComponent<MeshFilter>().sharedMesh);

            // -- Reverse triangles if camera mirrored
            if (faceTracking.CameraManager.VideoParameters.MirrorVideo)
                mesh.triangles = mesh.triangles.Reverse().ToArray();
            GetComponent<MeshFilter>().mesh = mesh;

            transform.eulerAngles = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.localPosition = new Vector3(0, 0, 1);

            // enable renderers
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers) r.enabled = false;
        }

        private void Start()
        {
        }

        private void OnDestroy()
        {
        }

        private void OnEnable()
        {
            m_face.FaceDataUpdated += OnFaceDataUpdated;
        }

        private void OnDisable()
        {
            m_face.FaceDataUpdated -= OnFaceDataUpdated;
        }
        

        private void OnFaceDataUpdated(XmgMagicFaceData faceData)
        {
            if (m_MeshFilter == null || m_MeshRenderer == null)
                return;

            // -- Get 2D key face features and convert to centered coordinate system
            var faceTracking = FindObjectOfType<XmgMagicFaceTracking>();
            int nbFaceFeatures2D = 68;
            Vector2[] vts = new Vector2[nbFaceFeatures2D];
            int cx = m_CameraManager.VideoParameters.GetProcessingWidth() / 2;
            int cy = m_CameraManager.VideoParameters.GetProcessingHeight() / 2;
            bool mirror = m_CameraManager.VideoParameters.MirrorVideo;
            for (int i = 0; i < faceData.m_faceData.m_nbLandmarks; i++)
            {
                vts[i].x = (faceData.m_dataLandmarks2D[2 * i] - cx) / cx;
                if (mirror) vts[i].x = -vts[i].x;
                vts[i].y = -(faceData.m_dataLandmarks2D[2 * i + 1] - cy) / cy;
            }

            // -- Create 3D vertices
            float sx = (float)XmgCameraBackground.GetScaleX(m_CameraManager.VideoParameters);
            float sy = (float)XmgCameraBackground.GetScaleY(m_CameraManager.VideoParameters);
            sx *= m_CameraManager.VideoParameters.VideoPlaneScale;
            sy *= m_CameraManager.VideoParameters.VideoPlaneScale;
            Vector3[] vertices = new Vector3[vts.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3(vts[i].x, vts[i].y, 1.0f);
            }


            if (!m_RefineMesh2D)
            {
                // Properly order indices
                int[] triangles = new int[faceData.m_faceData.m_nbTriangles * 3];
                Array.Copy(
                    faceData.m_dataTriangles2D, faceData.m_dataTriangles2D.GetLowerBound(0),
                    triangles, triangles.GetLowerBound(0), faceData.m_faceData.m_nbTriangles * 3);
                
                m_MeshFilter.mesh.vertices = vertices;
            }
            else
            {
                // -- Refine vertices                
                float[] faceFeatures = new float[nbFaceFeatures2D * 2];
                for (int i = 0; i < nbFaceFeatures2D; i++)
                {
                    faceFeatures[2 * i] = vertices[i].x;
                    faceFeatures[2 * i + 1] = vertices[i].y;
                }
                float[] out_contour = new float[1000];
                int[] out_contour_size = new int[1];
                GCHandle hdl_face_features = GCHandle.Alloc(faceFeatures, GCHandleType.Pinned);
                GCHandle hdl_out_contour = GCHandle.Alloc(out_contour, GCHandleType.Pinned);
                GCHandle hdl_out_contour_size = GCHandle.Alloc(out_contour_size, GCHandleType.Pinned);

                // -- Compute refined coutour and triangulate 
                int[] out_triangulation = new int[1000];
                int[] out_triangulation_size = new int[1];
                GCHandle hdl_out_triangulation = GCHandle.Alloc(out_triangulation, GCHandleType.Pinned);
                GCHandle hdl_out_triangulation_size = GCHandle.Alloc(out_triangulation_size, GCHandleType.Pinned);
                bool fillEyes = false;
                bool fillMouth = false;
                XmgMagicFaceBridge.xzimgMagicFaceRefineContourTriangulate(
                    hdl_face_features.AddrOfPinnedObject(),
                    nbFaceFeatures2D,
                    hdl_out_contour.AddrOfPinnedObject(),
                    hdl_out_contour_size.AddrOfPinnedObject(),
                    hdl_out_triangulation.AddrOfPinnedObject(),
                    hdl_out_triangulation_size.AddrOfPinnedObject(),
                    fillEyes ? 1: 0,
                    fillMouth ? 1 : 0);

                // Release 
                hdl_face_features.Free();
                hdl_out_contour.Free();
                hdl_out_contour_size.Free();
                hdl_out_triangulation.Free();
                hdl_out_triangulation_size.Free();


                //Array.Resize<int>(ref out_triangulation, out_triangulation_size[0]*3);

                vertices = new Vector3[out_contour_size[0]];
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = new Vector3(out_contour[2 * i], out_contour[2 * i + 1], 1);
                }

                m_MeshFilter.mesh.vertices = vertices;
            }
            

            // shader parameters
            var orientation = XmgTools.GetVideoOrientation(m_CameraManager.VideoParameters.UseNativeCapture, m_CameraManager.VideoParameters.UseFrontal);

#if ((UNITY_IOS) && !UNITY_EDITOR && !UNITY_STANDALONE)
            if (orientation == XmgOrientationMode.LandscapeLeft)
                orientation = XmgOrientationMode.LandscapeRight;
            else if (orientation == XmgOrientationMode.LandscapeRight)
                orientation = XmgOrientationMode.LandscapeLeft;
#endif

            m_MeshRenderer.material.SetInt("_Rotation", (int)orientation);
            m_MeshRenderer.material.SetFloat("_ScaleX", sx);
            m_MeshRenderer.material.SetFloat("_ScaleY", sy);

            var renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers) r.enabled = true;
        }
    }
}
