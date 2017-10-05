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
using System.Collections.Generic;
using Tizen;

namespace JuvoPlayer.Player
{
    public class AudioPacketStream : IPacketStream
    {
        private IPlayerAdapter playerAdapter;
        private StreamConfig config;

        public AudioPacketStream(IPlayerAdapter player, StreamConfig config)
        {
            Log.Info("JuvoPlayer", "AudioPacketStream");

            playerAdapter = player ?? throw new ArgumentNullException("player cannot be null");
            this.config = config ?? throw new ArgumentNullException("config cannot be null");

            if (!(config is AudioStreamConfig))
                throw new ArgumentException("config should be audioconfig");

            playerAdapter.SetAudioStreamConfig(config as AudioStreamConfig);
        }

        public void OnAppendPacket(StreamPacket packet)
        {
            if (packet.StreamType != StreamType.Audio)
                throw new ArgumentException("packet should be audio");

            playerAdapter.AppendPacket(packet);
        }

        public void OnClearStream()
        {

        }

        public void OnDRMFound(DRMData data)
        {

        }
    }
}