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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JuvoLogger;
using JuvoPlayer.Common;
using JuvoPlayer.Utils;

namespace JuvoPlayer.OpenGL
{
    internal class ResourceLoader
    {
        public List<ClipDefinition> ContentList { get; private set; }
        public ILogger Logger { private get; set; }
        public int TilesCount => ContentList?.Count ?? 0;

        public bool IsLoadingFinished { get; private set; }

        private int _resourcesLoadedCount;
        private int _resourcesTargetCount;
        private readonly SynchronizationContext _synchronizationContext = SynchronizationContext.Current; // If "Current" is null, then the thread's current context is "new SynchronizationContext()", by convention.
        List<Task> baseTasks = new List<Task>();
        private Action _doAfterFinishedLoading;

        public void LoadResources(string fullExecutablePath, Action doAfterFinishedLoading = null)
        {
            IsLoadingFinished = false;
            _doAfterFinishedLoading = doAfterFinishedLoading;

            InitLoadingScreen();
            var clipsFilePath = Path.Combine(fullExecutablePath, "shared", "res", "videoclips.json");
            LoadContentList(clipsFilePath);

            var resourcesDirPath = Path.Combine(fullExecutablePath, "res");

            LoadFonts(resourcesDirPath);
            LoadIcons(resourcesDirPath);
            LoadTiles(resourcesDirPath);
            foreach (var baseTask in baseTasks)
                baseTask.Start();
        }

        public static byte[] GetBytes(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        private void ScheduleToBeLoadedInMainThread(Action lambda)
        {
            _synchronizationContext.Post(delegate { lambda.Invoke(); }, null);
        }

        private void LoadAndSchedule(Resource resource)
        {
            var baseTask = new Task<Resource>(() =>
            {
                resource.Load();
                return resource;
            });
            baseTask.ContinueWith(task =>
            {
                ScheduleToBeLoadedInMainThread(() =>
                {
                    task.Result.Push();
                    ++_resourcesLoadedCount;
                    UpdateLoadingState();
                });
            });
            baseTasks.Add(baseTask);
        }

        private void FinishLoading()
        {
            IsLoadingFinished = true;
            if(_doAfterFinishedLoading != null)
                ScheduleToBeLoadedInMainThread(_doAfterFinishedLoading); // it's already called from the main thread since last job calls this method, but just to be safe let's schedule it for the main thread
        }

        private void LoadContentList(string filePath)
        {
            ContentList = JSONFileReader.DeserializeJsonFile<List<ClipDefinition>>(filePath).ToList();
        }

        private void LoadTiles(string dirPath)
        {
            _resourcesTargetCount += ContentList.Count;
            foreach (var contentItem in ContentList)
                LoadAndSchedule(new TileResource(DllImports.AddTile(), Path.Combine(dirPath, "tiles", contentItem.Poster), contentItem.Title ?? "", contentItem.Description ?? ""));
        }

        private void LoadIcons(string dirPath)
        {
            _resourcesTargetCount += Icons.Length;
            foreach (var icon in Icons)
                LoadAndSchedule(new IconResource(icon.Id, Path.Combine(dirPath, "icons", icon.Image.Path)));
        }

        private void LoadFonts(string dirPath)
        {
            _resourcesTargetCount += 1;
            LoadAndSchedule(new FontResource(Path.Combine(dirPath, "fonts/akashi.ttf")));
        }

        private void InitLoadingScreen()
        {
            DllImports.ShowLoader(1, 0);
        }

        private void UpdateLoadingState()
        {
            UpdateLoadingScreen();
            if (_resourcesLoadedCount >= _resourcesTargetCount)
            {
                baseTasks.Clear();
                FinishLoading();
            }
        }

        private void UpdateLoadingScreen()
        {
            DllImports.ShowLoader(_resourcesLoadedCount < _resourcesTargetCount ? 1 : 0, _resourcesTargetCount > 0 ? 100 * _resourcesLoadedCount / _resourcesTargetCount : 0);
        }

        private static readonly Icon[] Icons =
        {
            new Icon
            {
                Id = IconType.Play,
                Image = new ImageData { Path = "play.png" }
            },
            new Icon
            {
                Id = IconType.Resume,
                Image = new ImageData { Path = "resume.png" }
            },
            new Icon
            {
                Id = IconType.Stop,
                Image = new ImageData { Path = "stop.png" }
            },
            new Icon
            {
                Id = IconType.Pause,
                Image = new ImageData { Path = "pause.png" }
            },
            new Icon
            {
                Id = IconType.FastForward,
                Image = new ImageData { Path = "fast-forward.png" }
            },
            new Icon
            {
                Id = IconType.Rewind,
                Image = new ImageData { Path = "rewind.png" }
            },
            new Icon
            {
                Id = IconType.SkipToEnd,
                Image = new ImageData { Path = "skip-to-end.png" }
            },
            new Icon
            {
                Id = IconType.SkipToStart,
                Image = new ImageData { Path = "skip-to-start.png" }
            },
            new Icon
            {
                Id = IconType.Options,
                Image = new ImageData { Path = "options.png" }
            }
        };
    }
}
