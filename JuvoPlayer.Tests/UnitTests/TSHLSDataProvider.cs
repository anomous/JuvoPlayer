/*!
 * https://github.com/SamsungDForum/JuvoPlayer
 * Copyright 2018, Samsung Electronics Co., Ltd
 * Licensed under the MIT license
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

﻿using System;
using JuvoPlayer.Common;
using JuvoPlayer.DataProviders.HLS;
using JuvoPlayer.Demuxers;
using NSubstitute;
using NUnit.Framework;

namespace JuvoPlayer.Tests.UnitTests
{
    [TestFixture]
    class TSHLSDataProvider
    {

        [Test]
        public void GetStreamsDescription_ClipWithNoSubtitles_ReturnsEmptyList()
        {
            var demuxerMock = Substitute.For<IDemuxerController>();
            var clipDefinition = new ClipDefinition();
            var dataProvider = new HLSDataProvider(demuxerMock, clipDefinition);

            var streamsDescription = dataProvider.GetStreamsDescription(StreamType.Subtitle);

            Assert.That(streamsDescription, Is.Empty);
        }

        [Test]
        public void OnChangeActiveStream_UnknownSubtitles_ThrowsArgumentException()
        {
            var demuxerMock = Substitute.For<IDemuxerController>();
            var clipDefinition = new ClipDefinition();
            var dataProvider = new HLSDataProvider(demuxerMock, clipDefinition);
            var streamDescription = new StreamDescription
            {
                StreamType = StreamType.Subtitle
            };

            Assert.Throws<ArgumentException>(() => dataProvider.OnChangeActiveStream(streamDescription));
        }
    }
}
