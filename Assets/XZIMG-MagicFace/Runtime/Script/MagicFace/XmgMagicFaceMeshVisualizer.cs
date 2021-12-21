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

using System.Linq;
using UnityEngine;

namespace XZIMG
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(XmgMagicFace))]
    public sealed class XmgMagicFaceMeshVisualizer : MonoBehaviour
    {
        #region Private properties
        /// <summary>
        /// <see cref="XmgMagicFace"/> component containing the face data.
        /// </summary>
        private XmgMagicFace m_face;
        private XmgCameraManager m_CameraManager => m_face.Face3DManager.FaceTracking.CameraManager;
        private XmgFaceFeaturesMode m_FaceFeaturesMode => m_face.Face3DManager.FaceTracking.FaceFeaturesMode;

        private bool m_MeshPrepared = false;
        #endregion

        private void Awake()
        {
            //m_MeshPrepared = false;
            m_face = GetComponent<XmgMagicFace>();
        }

        private void Start()
        {
            // Has to move it in the Awake as it is called after OnFaceDAtaUpdated ??
            PrepareMesh();
            m_MeshPrepared = true;
        }

        private void OnEnable()
        {
            m_face.FaceDataUpdated += OnFaceDataUpdated;
        }

        private void OnDisable()
        {
            m_face.FaceDataUpdated -= OnFaceDataUpdated;
        }

        private void PrepareMesh()
        {
            transform.eulerAngles = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            Mesh mesh = null;
        
            // Mesh filter is not defined, we use default ones
            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();
            XmgObjImporter objImporter = new XmgObjImporter();
            mesh = objImporter.Import("XZIMG/Models/face-model-obj");
            GetComponent<MeshFilter>().mesh = mesh;

            // enable renderers
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers) r.enabled = false;
        }

        private void OnFaceDataUpdated(XmgMagicFaceData faceData)
        {
            if (!m_MeshPrepared) return;

            // -- Read and convert 3D vertices
            int nbFaceFeatures = faceData.m_faceData.m_nbLandmarks3D;
            Vector3[] vertices3D = new Vector3[nbFaceFeatures];
            for (int i = 0; i < nbFaceFeatures; i++)
            {
                vertices3D[i].x = faceData.m_dataLandmarks3D[3 * i];

                // mirror
                if (m_CameraManager.VideoParameters.MirrorVideo)
                    vertices3D[i].x = -vertices3D[i].x;
                // left handed
                vertices3D[i].y = -faceData.m_dataLandmarks3D[3 * i + 1];
                vertices3D[i].z = faceData.m_dataLandmarks3D[3 * i + 2];
            }

            // -- Update the mesh
            Mesh msh = GetComponent<MeshFilter>().mesh;
            msh.vertices = vertices3D;

            XmgTools.UpdateObjectPosition(
                faceData.m_faceData,
                gameObject,
                m_CameraManager.VideoParameters.VideoPlaneScale,
                m_CameraManager.VideoParameters.MirrorVideo,
                m_CameraManager.VideoParameters.UseFrontal);

            // enable renderers
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers) r.enabled = true;
        }
    }
}
