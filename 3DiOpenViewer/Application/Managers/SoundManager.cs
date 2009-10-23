/*
 * Copyright (c) 2008-2009, 3Di, Inc. (http://3di.jp/) and contributors.
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of 3Di, Inc., nor the name of the 3Di Viewer
 *       "Rei" project, nor the names of its contributors may be used to
 *       endorse or promote products derived from this software without
 *       specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY 3Di, Inc. AND CONTRIBUTORS ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL 3Di, Inc. OR THE
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using OpenMetaverse;
using IrrlichtNETCP;

namespace OpenViewer.Managers
{
    public class SoundManager : BaseComponent
    {
        private IrrKlang.ISoundEngine engine = null;
        private List<UUID> soundList = new List<UUID>();
        private List<UUID> requestList = new List<UUID>();
        private List<UUID> reserveList = new List<UUID>();
        private string workDirectory = string.Empty;

        private string initWindowBackgroundMusicURL = string.Empty;
        public string InitWindowBackgroundMusicURL { get { return (initWindowBackgroundMusicURL); } set { initWindowBackgroundMusicURL = value; } }

        public SoundManager(Viewer _viewer)
            : base(_viewer, -1)
        {
            try
            {
                engine = new IrrKlang.ISoundEngine();

                if (engine == null)
                    Reference.Log.Debug("Constructor: Engine NULL");
            }
            catch (Exception e)
            {
                Reference.Log.Debug("Constructor: ERROR:" + e.Message);
                engine = null;
            }

            ChangeWorkDirectory(Util.SoundFolder);
        }

        public override void Cleanup()
        {
            if (engine != null)
                engine.StopAllSounds();

            base.Cleanup();
        }

        public void Get(UUID _uuid)
        {
            // Already uuid exsit.
            if (soundList.Contains(_uuid))
                return;

            if (requestList.Contains(_uuid) == false)
                requestList.Add(_uuid);

            Reference.Log.Debug("RequestAsset:" + _uuid.ToString());

            // Future Threading.
            GetAsset(_uuid);
        }

        public bool Contain(UUID _uuid)
        {
            return soundList.Contains(_uuid);
        }

        private void GetAsset(UUID _uuid)
        {
            Reference.Viewer.IrrManager.IrrFileCreateCache(_uuid.ToString() + Util.SoundExtension, workDirectory);

            if (requestList.Contains(_uuid))
                requestList.Remove(_uuid);

            if (soundList.Contains(_uuid) == false)
                soundList.Add(_uuid);

            if (reserveList.Contains(_uuid))
            {
                PlaySE(_uuid);

                reserveList.Remove(_uuid);
            }

            Reference.Log.Debug("CompleteAsset:" + _uuid.ToString());
        }

        public bool PlayBGM(UUID _uuid)
        {
            return Play2D(_uuid, true);
        }

        public bool PlaySE(UUID _uuid)
        {
            bool flag = Play2D(_uuid, false);

            if (flag == false)
            {
                Get(_uuid);

                reserveList.Add(_uuid);
            }

            return flag;
        }

        public bool PlaySE(UUID _uuid, Vector3D _position)
        {
            return Play3D(_uuid, true, _position);
        }

        public bool Play2D(UUID _uuid, bool _loop)
        {
            return Play(_uuid, _loop, false, new Vector3D());
        }

        public bool Play3D(UUID _uuid, bool _loop, Vector3D _position)
        {

            return Play(_uuid, _loop, true, _position);
        }

        private bool Play(UUID _uuid, bool _loop, bool _is3d, Vector3D _position)
        {
            bool flag = false;

            if (soundList.Contains(_uuid))
            {
                string path = workDirectory + "/" + _uuid.ToString() + Util.SoundExtension;

                flag = Play(path, _loop, _is3d, _position);
            }

            return flag;
        }

        private bool Play(string _fileName, bool _loop, bool _is3d, Vector3D _position)
        {
            bool flag = false;

            if (engine == null)
            {
                Reference.Log.Debug("Play: Engine NULL");
                return flag;
            }

            if (System.IO.File.Exists(_fileName))
            {
                if (_is3d)
                {
                    engine.Play3D(_fileName, _position.X, _position.Y, _position.Z, _loop);
                }
                else
                {
                    engine.Play2D(_fileName, _loop);
                }

                Reference.Log.Debug("Play sound" + _fileName);

                flag = true;
            }

            return flag;
        }

        public void Stop()
        {
            if (engine == null)
            {
                Reference.Log.Debug("Stop: Engine NULL");
                return;
            }

            engine.StopAllSounds();
        }

        private void ChangeWorkDirectory(string _directory)
        {
            workDirectory = _directory;

            // If asset directory don't exists, create directory.
            if (System.IO.Directory.Exists(workDirectory) == false)
            {
                System.IO.Directory.CreateDirectory(workDirectory);
            }
        }

        public void LoadBackgroundMusic()
        {
            if (engine == null)
            {
                Reference.Log.Debug("Play: Engine NULL");
                return;
            }

            if (string.IsNullOrEmpty(initWindowBackgroundMusicURL) == false)
            {
                string filename = Util.SoundFolder + "bgm.ogg";
                Util.DownloadImage(initWindowBackgroundMusicURL, filename);
                initWindowBackgroundMusicURL = filename;
            }
            //// [YK:DON'T USE DEFALUT MUSIC]
            //else
            //{
            //    initWindowBackgroundMusicURL = Util.ApplicationDataDirectory + @"\media\sounds\bgm.ogg";
            //}

            if (System.IO.File.Exists(initWindowBackgroundMusicURL))
            {
                engine.Play2D(initWindowBackgroundMusicURL, true);
            }
        }

        public string WorkDirectory
        {
            get { return workDirectory; }
            set { ChangeWorkDirectory(value); }
        }
    }
}
