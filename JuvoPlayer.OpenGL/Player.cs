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
using System.Reactive.Linq;
using ElmSharp;

namespace JuvoPlayer.OpenGL
{
    class Player : PlayerServiceProxy
    {
        public new PlayerState State => ToPlayerState(base.State);

        public Player(Window playerWindow)
            : base(new PlayerServiceImpl(playerWindow))
        {
        }

        public new IObservable<PlayerState> StateChanged()
        {
            return base.StateChanged().Select(ToPlayerState);
        }

        private PlayerState ToPlayerState(Common.PlayerState state)
        {
            switch (state)
            {
                case Common.PlayerState.Idle:
                    return PlayerState.Idle;
                case Common.PlayerState.Prepared:
                    return PlayerState.Prepared;
                case Common.PlayerState.Paused:
                    return PlayerState.Paused;
                case Common.PlayerState.Playing:
                    return PlayerState.Playing;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}