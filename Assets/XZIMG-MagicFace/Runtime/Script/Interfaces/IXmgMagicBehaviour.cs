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

namespace XZIMG
{
    internal interface IXmgMagicBehaviour
    {

        /// <summary>
        /// Validate tracking parameters
        /// </summary>
        /// <param name="videoParameters">Video capture parameters</param>
        void OnXmgValidate();

        /// <summary>
        /// Initialize tracking
        /// </summary>
        void OnXmgInitialize();

        /// <summary>
        /// Render AR
        /// </summary>
        /// <param name="videoFrame">video capture frame</param>
        void OnXmgRendering(XmgImage videoFrame);
    }
}
