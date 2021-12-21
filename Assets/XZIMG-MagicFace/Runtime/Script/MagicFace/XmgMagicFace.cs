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
    [DisallowMultipleComponent]
    public sealed class XmgMagicFace: MonoBehaviour
    {
        #region Public properies
        /// <summary>
        /// If true, this component's <c>GameObject</c> will be removed immediately when the face is no longer tracked.
        /// </summary>
        /// <remarks>
        /// Setting this to false will keep the <c>GameObject</c> around. You may want to do this, for example,
        /// if you have custom removal logic, such as a fade out.
        /// </remarks>
        public bool DestroyOnRemoval
        {
            get { return m_DestroyOnRemoval; }
            set { m_DestroyOnRemoval = value; }
        }

        /// <summary>
        /// Tracked face's index number
        /// </summary>
        public int FaceIndex => m_faceIndex;

        /// <summary>
        /// Face tracking data
        /// </summary>
        public XmgMagicFaceData FaceData => m_faceData;

        /// <summary>
        /// Raised for each new <see cref="XmgMagicFace"/> detected in the environment.
        /// </summary>
        public event Action<XmgMagicFaceData> FaceDataUpdated;

        /// <summary>
        /// <see cref="XmgMagicFaceManager"/> component updating the face data.
        /// </summary>
        public XmgMagicFaceManager Face3DManager => m_face3DManager;
        #endregion

        #region Private properties
        /// <summary>
        /// If true, this component's GameObject will be removed immediately when this face is no longer tracked.
        /// </summary>
        [SerializeField]
        [Tooltip("If true, this component's GameObject will be removed immediately when this face is no longer tracked.")]
        private bool m_DestroyOnRemoval = true;

        /// <summary>
        /// Tracked face's index number
        /// </summary>
        private int m_faceIndex = -1;

        /// <summary>
        /// Face tracking data
        /// </summary>
        private XmgMagicFaceData m_faceData;

        /// <summary>
        /// <see cref="XmgMagicFace3DManager"/> component updating the face data.
        /// </summary>
        private XmgMagicFaceManager m_face3DManager;
        #endregion
        
        internal void SetFaceManager(XmgMagicFaceManager faceManager)
        {
            m_face3DManager = faceManager;
        }

        internal void SetFaceIndex(int faceIndex)
        {
            m_faceIndex = faceIndex;
        }

        internal void UpdateMagicFaceData(XmgMagicFaceData faceData)
        {
            m_faceData = faceData;
            FaceDataUpdated?.Invoke(m_faceData);
        }
    }
}
