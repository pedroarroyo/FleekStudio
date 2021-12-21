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
using System.Collections.Generic;

namespace XZIMG
{
    internal interface IXmgMagicFaceManager
    {
        /// <summary>
        /// Initialize Magic face manager
        /// </summary>
        void OnXmgInitialize();

        /// <summary>
        /// Update detected faces' data
        /// </summary>
        /// <param name="videoFrame">list of detected faces' data</param>
        void OnXmgUpdateMagicFaces(List<XmgMagicFaceData> faceDataList);
    }
}
