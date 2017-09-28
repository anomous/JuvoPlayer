// Copyright (c) 2017 Samsung Electronics Co., Ltd All Rights Reserved
// PROPRIETARY/CONFIDENTIAL 
// This software is the confidential and proprietary
// information of SAMSUNG ELECTRONICS ("Confidential Information"). You shall
// not disclose such Confidential Information and shall use it only in
// accordance with the terms of the license agreement you entered into with
// SAMSUNG ELECTRONICS. SAMSUNG make no representations or warranties about the
// suitability of the software, either express or implied, including but not
// limited to the implied warranties of merchantability, fitness for a
// particular purpose, or non-infringement. SAMSUNG shall not be liable for any
// damages suffered by licensee as a result of using, modifying or distributing
// this software or its derivatives.

using JuvoPlayer.Common;
using System;

namespace JuvoPlayer.RTSP
{
    public class RTSPDataProviderFactory : IDataProviderFactory
    {

        public RTSPDataProviderFactory()
        {
        }

        public IDataProvider Create(ClipDefinition clip)
        {
            if (clip == null)
            {
                throw new ArgumentNullException("clip cannot be null");
            }

            if (!SupportsClip(clip))
            {
                throw new ArgumentException("unsupported clip type");
            }

            var sharedBuffer = new SharedBuffer();
            var rtspClient = new RTSPClient(sharedBuffer);
            var demuxer = new FFmpegDemuxer(sharedBuffer);

            return new RTSPDataProvider(demuxer, rtspClient, clip);
        }

        public bool SupportsClip(ClipDefinition clip)
        {
            if (clip == null)
            {
                throw new ArgumentNullException("clip cannot be null");
            }

            return clip.Type == "RTP" || clip.Type == "RTSP";
        }
    }
}