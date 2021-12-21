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
using System.Collections.Generic;

namespace XZIMG
{
    /// <summary>
    /// Delegate instantiating a face gameobject for a specific face index.
    /// </summary>
    /// <param name="faceIndex">index of the detected face</param>
    /// <returns></returns>
    public delegate GameObject XmgMagicFaceSpwanHandlerDelegate(int faceIndex);
    
    [RequireComponent(typeof(XmgMagicFaceTracking))]
    public sealed class XmgMagicFaceManager : MonoBehaviour, IXmgMagicFaceManager
    {
        #region Public properties        
        /// <summary>
        /// Used when PrefabForFaceWithIndex is null.
        /// Getter/setter for the Face Prefab.
        /// </summary>
        public GameObject FacePrefab
        {
            get { return m_FacePrefab; }
            set { m_FacePrefab = value; }
        }

        public GameObject FacePrefabInstanciated
        {
            get { return m_FacePrefabInstanciated; }
            set { m_FacePrefabInstanciated = value; }
        }

        /// <summary>
        /// Used when FacePrefab is null.
        /// Delegate returning a gameobject with a <see cref="XmgMagicFace"/> component for a specific detected face index.
        /// </summary>
        public XmgMagicFaceSpwanHandlerDelegate FaceSpawnHandler;

        /// <summary>
        /// Raised for each new <see cref="XmgMagicFace"/> detected or desapeared in the environment.
        /// </summary>
        public event Action<List<XmgMagicFaceData>> FacesChanged;
        #endregion

        #region Internal properties
        /// <summary>
        /// <see cref="XmgMagicFaceTracking"/> component of this gameobject.
        /// </summary>
        internal XmgMagicFaceTracking FaceTracking => m_FaceTracking;
        #endregion

        #region Private properties        
        /// <summary>
        /// If not null, instantiates this prefab for each created face.
        /// </summary>
        [SerializeField]
        [Tooltip("If not null, instantiates this prefab for each created face.")]
        GameObject m_FacePrefab;


        GameObject m_FacePrefabInstanciated;
        /// <summary>
        /// Instantiated faces.
        /// </summary>
        private Dictionary<int, XmgMagicFace> m_faces = new Dictionary<int, XmgMagicFace>();
        
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
            foreach(var pair in m_faces)
            {
                var face = pair.Value;
                if(face!= null) Destroy(face.gameObject);
            }
            m_faces.Clear();
        }

        #region IXmgMagicFaceManager
        void IXmgMagicFaceManager.OnXmgInitialize()
        {
        }

        void IXmgMagicFaceManager.OnXmgUpdateMagicFaces(List<XmgMagicFaceData> faceDataList)
        {
            if (isActiveAndEnabled)
                UpdateFaceData(faceDataList);
        }
        #endregion

        #region Tracking
        private void UpdateFaceData(List<XmgMagicFaceData> faceDataList)
        {
            bool didFaceChanged = false;

            // Loop scene faces
            for (int o = 0; o < faceDataList.Count; o++)
            {
                if (faceDataList[o].m_faceData.m_faceDetected > 0)
                {
                    XmgMagicFace face;
                    if (!m_faces.ContainsKey(o))    // check if face is already in the dict. of faces
                    {
                        if (FacePrefab != null)
                        {
                            m_FacePrefabInstanciated = Instantiate(FacePrefab);
                            m_FacePrefabInstanciated.name = $"{FacePrefab.name} ({o})";
                        }
                        else if (FaceSpawnHandler != null)
                            m_FacePrefabInstanciated = FaceSpawnHandler(o);
                        else
                        {
                            Debug.LogError("==> (E) Either FacePrefab and FaceSpawnHandler are null.");
                            return;
                        }

                        face = m_FacePrefabInstanciated.GetComponent<XmgMagicFace>();

                        if (face == null)
                        {
                            Debug.LogError("==> (E) problems when instantiating face prefab. Face prefab has no XmgMagicFace component.");
                            return;
                        }
                        else
                            m_faces.Add(o, face);

                        face.SetFaceManager(this);
                        face.SetFaceIndex(o);

                        didFaceChanged = true;
                    }
                    else
                        face = m_faces[o];

                    face.UpdateMagicFaceData(faceDataList[o]);
                }
                else if (m_faces.ContainsKey(o))
                {
                    XmgMagicFace face = m_faces[o];
                    
                    if (face.DestroyOnRemoval)
                    {
                        // Destroy the gameobject and remove the face
                        Destroy(face.gameObject);
                        m_faces.Remove(o);
                    }
                    else
                    {
                        // Update the face and disable the renderers 
                        face.UpdateMagicFaceData(faceDataList[o]);
                        
                        var renderers = face.GetComponentsInChildren<Renderer>();
                        foreach (Renderer r in renderers)
                            r.enabled = false;
                    }
                    didFaceChanged = true;
                }
            }

            if (didFaceChanged)
                FacesChanged?.Invoke(faceDataList);
        }
        #endregion
    }
}
