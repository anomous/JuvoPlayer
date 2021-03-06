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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using JuvoPlayer.Common;
using JuvoPlayer.Demuxers;
using JuvoPlayer.Subtitles;

namespace JuvoPlayer.DataProviders.HLS
{
    internal class HLSDataProvider : IDataProvider
    {
        private static readonly TimeSpan MaxBufferHealth = TimeSpan.FromSeconds(10);

        private readonly IDemuxerController demuxerController;
        private readonly ClipDefinition currentClip;

        private TimeSpan lastReceivedPts;
        private TimeSpan currentTime;

        private CuesMap cuesMap;

        public HLSDataProvider(IDemuxerController demuxerController, ClipDefinition currentClip)
        {
            this.demuxerController = demuxerController ??
                                     throw new ArgumentNullException(nameof(demuxerController),
                                         "demuxerController cannot be null");
            this.currentClip =
                currentClip ?? throw new ArgumentNullException(nameof(currentClip), "clip cannot be null");
        }

        private bool ShouldPauseDemuxer()
        {
            return lastReceivedPts - currentTime > MaxBufferHealth;
        }

        public void OnChangeActiveStream(StreamDescription stream)
        {
            if (stream.StreamType == StreamType.Subtitle)
            {
                OnChangeActiveSubtitleStream(stream);
                return;
            }

            throw new NotImplementedException();
        }

        public void OnDeactivateStream(StreamType streamType)
        {
            if (streamType == StreamType.Subtitle)
            {
                OnDeactivateSubtitleStream();
                return;
            }

            throw new NotImplementedException();
        }

        public void OnStateChanged(PlayerState state)
        {
            switch (state)
            {
                case PlayerState.Paused:
                    demuxerController.Pause();
                    break;
                case PlayerState.Playing:
                    demuxerController.Resume();
                    break;
            }
        }

        private void OnDeactivateSubtitleStream()
        {
            cuesMap = null;
        }

        public void OnSeekStarted(TimeSpan time, uint seekId)
        {
            throw new NotImplementedException();
        }

        public void OnSeekCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnStopped()
        {
        }

        public bool IsSeekingSupported()
        {
            return false;
        }

        public void Stop()
        {
        }

        public void Start()
        {
            demuxerController.StartForUrl(currentClip.Url);
        }

        public Cue CurrentCue => cuesMap?.Get(currentTime);

        public void OnTimeUpdated(TimeSpan time)
        {
            currentTime = time;
            ResumeDemuxerIfNecessary();
        }

        private void ResumeDemuxerIfNecessary()
        {
            var shouldResumeDemuxer = lastReceivedPts - currentTime <
                                      MaxBufferHealth - TimeSpan.FromTicks(MaxBufferHealth.Ticks / 2);
            if (shouldResumeDemuxer)
            {
                demuxerController.Resume();
            }
        }

        public void Dispose()
        {
            demuxerController.Dispose();
        }

        public List<StreamDescription> GetStreamsDescription(StreamType streamType)
        {
            if (streamType == StreamType.Subtitle)
                return GetSubtitleStreamsDescription();
            return new List<StreamDescription>();
        }

        public IObservable<TimeSpan> ClipDurationChanged()
        {
            return demuxerController.ClipDurationFound();
        }

        public IObservable<DRMInitData> DRMInitDataFound()
        {
            return demuxerController.DrmInitDataFound();
        }

        public IObservable<DRMDescription> SetDrmConfiguration()
        {
            return Observable.Empty<DRMDescription>();
        }

        public IObservable<StreamConfig> StreamConfigReady()
        {
            return demuxerController.StreamConfigReady();
        }

        public IObservable<Packet> PacketReady()
        {
            return demuxerController.PacketReady()
                .Do(packet =>
                {
                    if (packet == null) return;
                    lastReceivedPts = packet.Pts;
                    if (ShouldPauseDemuxer())
                        demuxerController.Pause();
                }).SelectMany(packet =>
                {
                    if (packet != null)
                        return Observable.Return(packet);
                    // found empty packet which means EOS. We need to send two fake
                    // eos packets, one for audio and one for video
                    return Observable.Return(Packet.CreateEOS(StreamType.Audio))
                        .Merge(Observable.Return(Packet.CreateEOS(StreamType.Video)));
                }); 
        }

        public IObservable<string> StreamError()
        {
            return Observable.Empty<string>();
        }

        public IObservable<Unit> BufferingStarted()
        {
            return Observable.Empty<Unit>();
        }

        public IObservable<Unit> BufferingCompleted()
        {
            return Observable.Empty<Unit>();
        }

        private List<StreamDescription> GetSubtitleStreamsDescription()
        {
            var subtitles = currentClip.Subtitles ?? new List<SubtitleInfo>();
            return subtitles.Select(info => info.ToStreamDescription()).ToList();
        }

        private void OnChangeActiveSubtitleStream(StreamDescription description)
        {
            var found = currentClip.Subtitles?.First(info => info.Id == description.Id);
            if (found == null)
                throw new ArgumentException();
            cuesMap = new SubtitleFacade().LoadSubtitles(found);
        }
    }
}